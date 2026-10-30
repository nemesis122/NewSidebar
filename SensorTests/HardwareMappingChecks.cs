using Aurora;
using LibreHardwareMonitor.Hardware;

static class HardwareMappingChecks
{
 public static void Run(Action<bool,string> check)
 {
  var units=new Dictionary<string,string>{{"Voltage","V"},{"Current","A"},{"Power","W"},{"Clock","MHz"},{"Temperature","°C"},{"Load","%"},{"Frequency","Hz"},{"Fan","RPM"},{"Flow","L/h"},{"Control","%"},{"Level","%"},{"Factor",""},{"IntFactor",""},{"Data","GB"},{"SmallData","MB"},{"Throughput","B/s"},{"TimeSpan","s"},{"Timing","ns"},{"Energy","mWh"},{"Noise","dBA"},{"Conductivity","µS/cm"},{"Humidity","%"}};
  foreach(var type in Enum.GetValues<SensorType>())check(units.TryGetValue(type.ToString(),out var unit)&&SensorService.Unit(type)==unit,"Every supported sensor kind has its physical unit: "+type);
  HardwareSample Drive(string id,string name)=>new(id,name,"Storage",new[]{new SensorSample(id+"/read","Read Rate",SensorType.Throughput,0)},"Storage fixture");
  var supplemented=SensorBackendMerge.AddMissingStorage(new[]{Drive("/ssd/0","Intel SSD")},new[]{Drive("/hdd/0","Intel SSD"),Drive("/hdd/1","Samsung NVMe")});
  check(supplemented.Length==2&&supplemented[0].Identifier=="/ssd/0"&&supplemented[1].Identifier=="ohm/fallback/hdd/1","Libre storage fallback adds an otherwise missing NVMe and preserves the existing SSD");
  check(supplemented[1].Sensors.Single().Identifier=="ohm/fallback/hdd/1/read","Fallback storage sensors retain ownership and stable IDs");
  var unmatchedStorage=SensorBackendMerge.Combine(new[]{Drive("ohm/direct/hdd/1","Samsung NVMe")},Array.Empty<HardwareSample>()).Single();
  check(unmatchedStorage.PreferenceDeviceId=="ohm/fallback/hdd/1"&&unmatchedStorage.PreferenceSensorIds!["ohm/direct/hdd/1/read"]=="ohm/fallback/hdd/1/read","New fallback drive selections migrate to the corresponding direct OHM drive");
  check(SensorBackendMerge.AddMissingStorage(new[]{Drive("/ssd/0","Same SSD")},new[]{Drive("/hdd/0","Same SSD"),Drive("/hdd/1","Same SSD")}).Length==2,"Storage fallback matches repeated SSD models one at a time");
  var direct=new HardwareSample("ohm/direct/gpu/0","Partial GPU","GpuNvidia",new[]{new SensorSample("ohm/direct/gpu/0/load","GPU Core",SensorType.Load,25),new SensorSample("ohm/direct/gpu/0/temp","GPU Core",SensorType.Temperature,null)},"OHM");
  var bridge=new HardwareSample("/gpu/0","Partial GPU","GpuNvidia",new[]{new SensorSample("/gpu/0/load","GPU Core",SensorType.Load,40),new SensorSample("/gpu/0/temp","GPU Core",SensorType.Temperature,51),new SensorSample("/gpu/0/power","GPU Package",SensorType.Power,75)},"Libre");
  var merged=SensorBackendMerge.Combine(new[]{direct},new[]{bridge}).Single();
  check(merged.Sensors.Single(s=>s.SensorType==SensorType.Load).Value==25,"Hybrid keeps an available direct GPU reading instead of overwriting it");
  check(merged.Sensors.Single(s=>s.SensorType==SensorType.Temperature).Value==51,"Hybrid fills an unavailable direct GPU sensor from the matching bridge");
  check(merged.Sensors.Single(s=>s.SensorType==SensorType.Power).Value==75,"Hybrid retains additional sensors of a matched GPU");
  check(merged.Sensors.Select(s=>s.Identifier).Distinct().Count()==3&&merged.Sensors.All(s=>s.Identifier.StartsWith(merged.Identifier+"/")),"Supplemented GPU sensors have unique stable IDs belonging to their GPU");
  check(merged.PreferenceSensorIds![merged.Sensors.Single(s=>s.SensorType==SensorType.Power).Identifier]=="/gpu/0/power","Supplemented sensors retain their source preference alias");
  using(var session=new SensorBackendSession(_=>new FailingBackend()))
  {
   var snapshot=new SensorService(session).Read();
   check(snapshot.Devices.Any(d=>d.Id=="windows/ram")&&snapshot.Devices.Any(d=>d.Id=="windows/cpu")&&snapshot.Warnings.Any(w=>w.Contains("Fixture access failure")),"Backend access failure preserves Windows CPU/RAM and reports the failure");
   check(!snapshot.Sensors.Any(s=>s.Kind=="Temperature"&&s.Value.HasValue),"Backend failure never fabricates a temperature");
  }
  foreach(var source in new[]{SensorSources.Libre,SensorSources.OpenPawn})
  {
   string Id(string value)=>source==SensorSources.OpenPawn?SensorSources.BridgeId(value):value;
   SensorSample S(string hw,string suffix,string name,SensorType type,double? value)=>new(Id(hw)+"/"+suffix,name,type,value);
   HardwareSample H(string id,string name,string kind,params SensorSample[] sensors)=>new(Id(id),name,kind,sensors,"Mapping fixture");
   var hardware=new[]{
    H("/cpu/0","AMD CPU","Cpu",S("/cpu/0","load","CPU Total",SensorType.Load,23),S("/cpu/0","temp","Core (Tctl/Tdie)",SensorType.Temperature,61),S("/cpu/0","clock","Core #1",SensorType.Clock,4111),S("/cpu/0","power","Package",SensorType.Power,65),S("/cpu/0","voltage","Core (SVI2 TFN)",SensorType.Voltage,1.2)),
    H("/gpu/0","NVIDIA GPU","GpuNvidia",S("/gpu/0","load","GPU Core",SensorType.Load,0),S("/gpu/0","temp","GPU Core",SensorType.Temperature,45),S("/gpu/0","clock0","GPU Core",SensorType.Clock,null),S("/gpu/0","clock1","GPU Render",SensorType.Clock,1500),S("/gpu/0","memused","GPU Memory Used",SensorType.SmallData,2048),S("/gpu/0","memtotal","GPU Memory Total",SensorType.SmallData,8192),S("/gpu/0","memload","GPU Memory",SensorType.Load,25),S("/gpu/0","fan","GPU Fan",SensorType.Fan,0)),
    H("/gpu/1","Intel GPU","GpuIntel",S("/gpu/1","decode","D3D Video Decode",SensorType.Load,80),S("/gpu/1","load","D3D 3D",SensorType.Load,12),S("/gpu/1","clock","GPU Render",SensorType.Clock,800)),
    H("/gpu/2","AMD GPU","GpuAmd",S("/gpu/2","load","GPU Core",SensorType.Load,35),S("/gpu/2","clock","GPU Core",SensorType.Clock,2000)),
    H("/disk/0","Same SSD","Storage",S("/disk/0","temp","Composite",SensorType.Temperature,35),S("/disk/0","read","Read Rate",SensorType.Throughput,0),S("/disk/0","write","Write Rate",SensorType.Throughput,1048576),S("/disk/0","life","Remaining Life",SensorType.Level,98)),
    H("/disk/1","Same SSD","Storage",S("/disk/1","temp","Temperature",SensorType.Temperature,41)),
    H("/board/0","Board","SuperIO",S("/board/0","fan","CPU Fan",SensorType.Fan,950),S("/board/0","voltage","Vcore",SensorType.Voltage,1.1)),
    H("/memory/0","Memory","Memory",S("/memory/0","load","Memory",SensorType.Load,20)),
    H("/controller/0","Cooling","Cooler",S("/controller/0","fan","Pump",SensorType.Fan,1800))};
   var backend=new MappingBackend(hardware);
   using var session=new SensorBackendSession(_=>backend);
   var service=new SensorService(session){Source=source};var snapshot=service.Read();
   var cpu=snapshot.Devices.Single(d=>d.Id==Id("/cpu/0"));var gpu=snapshot.Devices.Single(d=>d.Id==Id("/gpu/0"));
   check(cpu.Metrics.Single(m=>m.Label=="Auslastung").Value==23&&cpu.Metrics.Single(m=>m.Id.EndsWith("/display-temperature")).Value==61,"AMD CPU load and die temperature map correctly: "+source);
   check(cpu.Metrics.Single(m=>m.IsCpuClock).Display(false)=="4.11 GHz","AMD CPU clocks retain GHz precision: "+source);
   check(gpu.Metrics.Single(m=>m.Label=="Auslastung").Value==0&&gpu.Metrics.Single(m=>m.Label=="Lüfter").Value==0,"Stopped GPU fans and idle loads remain genuine zero: "+source);
   check(gpu.Metrics.Single(m=>m.Label=="Takt").Value==1500,"An unavailable preferred GPU clock falls back to a valid render clock: "+source);
   check(gpu.Metrics.Single(m=>m.Label=="VRAM belegt").Maximum==8192,"VRAM scale follows the detected GPU capacity: "+source);
   check(snapshot.Devices.Count(d=>d.Category=="GPU")==3,"Intel, AMD and NVIDIA GPUs remain separate: "+source);
   check(snapshot.Devices.Single(d=>d.Id==Id("/gpu/1")).Metrics.Single(m=>m.Label=="Auslastung").Value==12,"Intel D3D GPU load mapping: "+source);
   check(snapshot.Devices.Count(d=>d.Name=="Same SSD")==2,"Identical SSD models remain separate by stable ID: "+source);
   var disk=snapshot.Devices.Single(d=>d.Id==Id("/disk/0"));
   check(disk.Metrics.Single(m=>m.Label=="Lesen").Value==0&&disk.Metrics.Single(m=>m.Label=="Schreiben").Display(false)=="1 MB/s","Storage throughput uses bytes per second and preserves zero: "+source);
   check(disk.Metrics.Single(m=>m.Label=="Verbleibende Lebensdauer").Value==98,"SSD remaining life is a percent: "+source);
   check(snapshot.Devices.Count(d=>d.Category=="RAM")==1&&snapshot.Devices.Single(d=>d.Category=="RAM").Id=="windows/ram","Windows RAM does not duplicate the backend memory card: "+source);
   var ram=snapshot.Devices.Single(d=>d.Category=="RAM");double R(string suffix)=>ram.Metrics.Single(m=>m.Id.EndsWith(suffix)).Value!.Value;
   check(Math.Abs(R("/used")+R("/free")-R("/total"))<.001,"RAM used plus available equals total: "+source);
   foreach(var volume in snapshot.Devices.Where(d=>d.Id.StartsWith("volume/")))
   {
    double V(string suffix)=>volume.Metrics.Single(m=>m.Id.EndsWith(suffix)).Value!.Value;
    check(V("total")>0&&V("free")>=0&&V("used")>=0&&Math.Abs(V("used")+V("free")-V("total"))<.001&&Math.Abs(V("load")-100*V("used")/V("total"))<.001,"Windows volume capacity arithmetic: "+source+" / "+volume.Id);
   }
   check(snapshot.Devices.Single(d=>d.Id==Id("/board/0")).Category=="Mainboard"&&snapshot.Devices.Single(d=>d.Id==Id("/controller/0")).Category=="Controller","Board and cooling sensors retain their categories: "+source);
   check(snapshot.Sensors.Select(s=>s.Id).Distinct().Count()==snapshot.Sensors.Count,"All raw and synthetic sensor IDs are unique: "+source);
   backend.Hardware=hardware.Select(h=>h with{Sensors=h.Sensors.Select(s=>s.SensorType==SensorType.Temperature?s with{Value=null}:s).ToArray()}).ToArray();
   var missing=service.Read();check(missing.Devices.Single(d=>d.Id==cpu.Id).Metrics.Single(m=>m.Id.EndsWith("/display-temperature")).Display(false)=="—","A disappearing temperature never returns a stale value: "+source);
   foreach(var network in missing.Devices.Where(d=>d.Category=="Network"))
   {
    check(network.Metrics.Where(m=>m.Id.EndsWith("rx")||m.Id.EndsWith("tx")).All(m=>m.Value is double value&&double.IsFinite(value)&&value>=0),"Network counter intervals produce finite nonnegative rates: "+source+" / "+network.Name);
    check(network.Metrics.Where(m=>m.Id.EndsWith("link")).All(m=>m.Value==null||m.Value>0),"Unknown network link speed remains unavailable: "+source+" / "+network.Name);
   }
   backend.Hardware=hardware.Select(h=>h with{Sensors=h.Sensors.Select(s=>s.Name=="GPU Render"?s with{Value=1700}:s).ToArray()}).ToArray();
   check(service.Read().Devices.Single(d=>d.Id==gpu.Id).Metrics.Single(m=>m.Label=="Takt").Value==1700,"GPU clock updates propagate through snapshots: "+source);
  }
 }
 sealed class MappingBackend(HardwareSample[] hardware):ISensorBackend
 {public HardwareSample[] Hardware=hardware;public void Open(){}public void Dispose(){}public HardwareSample[] Capture(List<string> warnings)=>Hardware;}
 sealed class FailingBackend:ISensorBackend
 {public void Open()=>throw new InvalidOperationException("Fixture access failure");public void Dispose(){}public HardwareSample[] Capture(List<string> warnings)=>throw new Exception("Not opened");}
}
