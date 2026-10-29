using Aurora;
using LibreHardwareMonitor.Hardware;
using System.Reflection;
using System.Runtime.InteropServices;
int passed=0;
void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}

// These checks execute before any optional hardware test.
var assembly=typeof(OpenHardwareMonitor.Hardware.DriverlessComputer).Assembly;
Check(assembly.GetName().Name=="NemeSIS.OpenHardwareMonitor.Driverless","Uses the modified source build rather than the upstream binary package");
Check(assembly.GetManifestResourceNames().Length==0,"Modified OHM contains no embedded payloads or driver resources");
Check(!assembly.GetTypes().Any(t=>t.Name.Contains("Ring0",StringComparison.OrdinalIgnoreCase)||t.Name.Contains("InpOut",StringComparison.OrdinalIgnoreCase)||t.Name=="KernelDriver"),"Old raw-access and driver-installation types have been removed");
var imports=assembly.GetTypes().SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)).Select(m=>(Method:m,Import:m.GetCustomAttribute<DllImportAttribute>())).Where(p=>p.Import!=null).ToArray();
Check(!imports.Any(p=>p.Method.Name.Contains("Service",StringComparison.OrdinalIgnoreCase)||p.Import!.Value.Contains("WinRing",StringComparison.OrdinalIgnoreCase)||p.Import.Value.Contains("InpOut",StringComparison.OrdinalIgnoreCase)),"OHM cannot install a kernel service or import a removed driver");
Check(!typeof(Computer).Assembly.GetManifestResourceNames().Any(n=>n.Contains("WinRing",StringComparison.OrdinalIgnoreCase)||n.Contains("inpout",StringComparison.OrdinalIgnoreCase)),"The existing PawnIO bridge contains no legacy driver resources");
Check(SensorSources.OpenId("/gpu/0")!=SensorSources.BridgeId("/gpu/0"),"Direct and bridge identifiers can never collide");

var log=new List<string>();
using(var session=new SensorBackendSession(source=>new FakeBackend(source,log)))
{
    session.Select(SensorSources.Libre);session.Select(SensorSources.Libre);
    Check(log.SequenceEqual(new[]{"open "+SensorSources.Libre}),"Repeated selection keeps one session open");
    session.Select(SensorSources.OpenPawn);
    Check(log.SequenceEqual(new[]{"open "+SensorSources.Libre,"close "+SensorSources.Libre,"open "+SensorSources.OpenPawn}),"Previous session closes before the next source opens");
    session.Select(SensorSources.Libre);
    Check(log[^2]=="close "+SensorSources.OpenPawn&&log[^1]=="open "+SensorSources.Libre,"Can switch back without overlapping sessions");
}
Check(log[^1]=="close "+SensorSources.Libre,"Shutdown releases the selected source");
using(var session=new SensorBackendSession(source=>new FakeBackend(source,new()){FailOpen=source==SensorSources.OpenPawn}))
{
    session.Select(SensorSources.Libre);
    try{session.Select(SensorSources.OpenPawn);throw new Exception("Expected initialization failure");}catch(InvalidOperationException){}
    Check(session.ActiveSource==""&&session.Capture(new()).Length==0,"Failed initialization does not return stale readings");
    session.Select(SensorSources.Libre);Check(session.ActiveSource==SensorSources.Libre,"The default source remains recoverable after failure");
}
log.Clear();
using(var session=new SensorBackendSession(source=>new FakeBackend(source,log){FailClose=source==SensorSources.Libre}))
{
    session.Select(SensorSources.Libre);
    try{session.Select(SensorSources.OpenPawn);throw new Exception("Expected close failure");}catch(InvalidOperationException){}
    Check(!log.Contains("open "+SensorSources.OpenPawn),"A close failure never starts a second session");
}

HardwareSample Disk(string id,string name,double? value=30)=>new(id,name,"Storage",new[]{new SensorSample(id+"/temp","Temperature",SensorType.Temperature,value)},"Fixture");
var merged=SensorBackendMerge.Combine(new[]{Disk("ohm/direct/0","Same model")},new[]{Disk("/disk/0","Same model"),Disk("/disk/1","Same model"),Disk("/disk/2","Other model")});
Check(merged.Length==3&&merged.Select(h=>h.Identifier).Distinct().Count()==3,"Repeated identical drives remain separate without duplicates");
Check(merged.Count(h=>h.Provider=="LibreHardwareMonitor fallback")==2,"Unmatched drives use an explicitly identified fallback");
Check(SensorBackendMerge.Combine(new[]{Disk("ohm/direct/0","Unavailable",null)},new[]{Disk("/disk/0","Working")}).Single().Provider=="LibreHardwareMonitor fallback","An OHM device with no available readings uses the bridge fallback");
var differentlyNamed=SensorBackendMerge.Combine(new[]{Disk("ohm/direct/0","Vendor NVMe")},new[]{Disk("/disk/0","Vendor NVMe (Disk 0)")});
Check(differentlyNamed.Length==1,"A single differently named drive is not duplicated");
using(var session=new SensorBackendSession(source=>new FakeBackend(source,new())))
{
    var service=new SensorService(session);var libre=service.Read();
    Check(libre.SensorSource==SensorSources.Libre,"Snapshot identifies its source");
    Check(libre.Devices.Single(d=>d.Category=="CPU").Metrics.First(m=>m.Id.EndsWith("display-temperature")).Value==50,"Socket temperature preference remains intact");
    service.Source=SensorSources.OpenPawn;var ohm=service.Read();
    Check(ohm.SensorSource==SensorSources.OpenPawn&&ohm.Devices.Single(d=>d.Category=="CPU").Id==SensorSources.BridgeId("/cpu/0"),"Bridge CPU identifiers and source survive snapshot mapping");
    Check(!ohm.Sensors.Any(s=>s.Id.StartsWith("/cpu/")),"Old source sensors cannot leak into the next snapshot");
    Check(ohm.Devices.Any(d=>d.Id=="windows/ram"),"Windows memory stays available with both options");
    Check(ohm.Devices.Single(d=>d.Category=="CPU").Metrics.First(m=>m.Id.EndsWith("display-temperature")).Value==50,"Bridge temperatures use the same CPU selection logic");
}

var seedPrefs=new Preferences{FontScale=1.4,ValueTextScale=1.2,HiddenDevices=new(){"/board/0"},HiddenMetrics=new(){"/gpu/0|/gpu/0/temp"},HardwareNames=new(){{"/gpu/0","Custom GPU"}}};
var seeds=new List<DevicePreferenceSeed>{new("ohm/pawn/board/0","/board/0",new()),new("ohm/direct/gpu/0","/gpu/0",new(){{"ohm/direct/gpu/0/temp","/gpu/0/temp"}})};
var seededSnapshot=new Snapshot(new(),new(),new(),DateTime.Now,SensorSources.OpenPawn,seeds);
Check(SensorPreferenceMigration.Apply(seededSnapshot,seedPrefs),"First alternative snapshot seeds matching device preferences");
Check(seedPrefs.HiddenDevices.Contains("ohm/pawn/board/0"),"Hidden board/controller cards do not reappear with the bridge");
Check(!seedPrefs.IsMetricVisible("ohm/direct/gpu/0","ohm/direct/gpu/0/temp"),"Previously hidden matching GPU readings stay hidden with OHM");
Check(seedPrefs.HardwareName("ohm/direct/gpu/0","Detected")=="Custom GPU","Hardware name follows the matched device initially");
Check(seedPrefs.FontScale==1.4&&seedPrefs.ValueTextScale==1.2,"Source selection does not change font size controls");
seedPrefs.HiddenDevices.Remove("ohm/pawn/board/0");seedPrefs.SetMetricVisible("ohm/direct/gpu/0","ohm/direct/gpu/0/temp",true);
Check(!SensorPreferenceMigration.Apply(seededSnapshot,seedPrefs)&&!seedPrefs.HiddenDevices.Contains("ohm/pawn/board/0")&&seedPrefs.IsMetricVisible("ohm/direct/gpu/0","ohm/direct/gpu/0/temp"),"Later source-specific visibility changes are not overwritten by live refresh");
var restored=System.Text.Json.JsonSerializer.Deserialize<Preferences>(System.Text.Json.JsonSerializer.Serialize(seedPrefs))!;
Check(!SensorPreferenceMigration.Apply(seededSnapshot,restored),"Initialization markers survive restart");
var mapped=SensorBackendMerge.Combine(new[]{Disk("ohm/direct/0","Matched")},new[]{Disk("/disk/0","Matched")}).Single();
Check(mapped.PreferenceDeviceId=="/disk/0"&&mapped.PreferenceSensorIds!["ohm/direct/0/temp"]=="/disk/0/temp","OHM merge exposes exact matching sensor aliases for visibility migration");
if(args.Contains("--hardware"))
{
    using var session=new SensorBackendSession();
    foreach(var source in new[]{SensorSources.Libre,SensorSources.OpenPawn,SensorSources.Libre})
    {
        var watch=System.Diagnostics.Stopwatch.StartNew();session.Select(source);var warnings=new List<string>();var hw=session.Capture(warnings);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{Source=source,Devices=hw.Length,Sensors=hw.Sum(h=>h.Sensors.Length),TemperatureSensors=hw.Sum(h=>h.Sensors.Count(s=>s.SensorType==SensorType.Temperature&&s.Value.HasValue)),Providers=hw.GroupBy(h=>h.Provider).Select(g=>new{Provider=g.Key,Devices=g.Count()}),Milliseconds=watch.ElapsedMilliseconds,Warnings=warnings}));
        Check(hw.All(h=>h.Sensors.All(s=>s.Value==null||double.IsFinite(s.Value.Value))),"Actual engine returns finite readings: "+source);
    }
}
Console.WriteLine($"{passed} sensor checks passed");

sealed class FakeBackend(string source,List<string> log):ISensorBackend
{
    public bool FailOpen,FailClose;
    string Id(string id)=>source==SensorSources.OpenPawn?SensorSources.BridgeId(id):id;
    public void Open(){log.Add("open "+source);if(FailOpen)throw new InvalidOperationException("Open failed");}
    public void Dispose(){log.Add("close "+source);if(FailClose){FailClose=false;throw new InvalidOperationException("Close failed");}}
    public HardwareSample[] Capture(List<string> warnings)=>new[]{new HardwareSample(Id("/cpu/0"),"Fixture CPU","Cpu",new[]{new SensorSample(Id("/cpu/0/temp/0"),"CPU Package",SensorType.Temperature,70),new SensorSample(Id("/cpu/0/load/0"),"CPU Total",SensorType.Load,31.7),new SensorSample(Id("/cpu/0/clock/0"),"Core #1",SensorType.Clock,4000)},"Fixture"),new HardwareSample(Id("/board/0"),"Fixture board","SuperIO",new[]{new SensorSample(Id("/board/0/temp/0"),"CPU",SensorType.Temperature,50)},"Fixture")};
}
