using Aurora;
using System.Text.Json;
if(OperatingSystem.IsWindows())TestProcessSafety.SetErrorMode(3);
if(OperatingSystem.IsWindows() && args.Length==2 && args[0]=="--signal-startup-instance")
{
    using var signal=EventWaitHandle.OpenExisting(StartupIdentity.ShowEventName(args[1]));
    signal.Set();return;
}
int passed=0;
void Require(bool condition,string label) { if(!condition) throw new Exception(label); Console.WriteLine("PASS "+label); passed++; }
CombinedSensorsChecks.Run(Require);
TypographyChecks.Run(Require);
CpuClockChecks.Run(Require);
Require(JsonSerializer.Deserialize<Preferences>("{}")!.SensorSource==SensorSources.Libre,"Existing configurations keep Libre as the default");
Require(JsonSerializer.Deserialize<Preferences>(JsonSerializer.Serialize(new Preferences{SensorSource=SensorSources.OpenPawn}))!.SensorSource==SensorSources.OpenPawn,"Embedded source selection survives restart");
Require(SensorSources.Normalize("invalid")==SensorSources.Libre,"Unknown sources normalize to Libre");
Require(SensorSources.OpenId("/cpu/0")!=SensorSources.BridgeId("/cpu/0"),"Direct and bridge sensor IDs are distinct");
Require(SensorSources.Finite(double.NaN)==null&&SensorSources.Finite(double.PositiveInfinity)==null&&SensorSources.Finite(0)==0,"Invalid sensor numbers stay unavailable while zero remains valid");
Require(JsonSerializer.Deserialize<Preferences>("{}")!.HardwareIconScale==1,"Existing settings keep hardware icons at 100 percent");
Require(JsonSerializer.Deserialize<Preferences>(JsonSerializer.Serialize(new Preferences{HardwareIconScale=.5,ShowHardwareIcons=false}))!.HardwareIconScale==.5,"Hardware icon size survives restart while hidden");
Require(JsonSerializer.Deserialize<Preferences>("{}")!.ShowHardwareIcons,"Existing settings show hardware icons by default");
Require(!JsonSerializer.Deserialize<Preferences>(JsonSerializer.Serialize(new Preferences{ShowHardwareIcons=false}))!.ShowHardwareIcons,"Hidden hardware icons survive restart");
Require(new Preferences{ShowHardwareIcons=false}.ShowWidgetTitles&&new Preferences{ShowHardwareIcons=false}.ShowHardwareNames,"Icon visibility does not change text visibility defaults");
string portableRoot=Path.Combine(Path.GetTempPath(),"NemeSIS-startup-tests-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(Path.Combine(portableRoot,"OEM"));Directory.CreateDirectory(Path.Combine(portableRoot,"Downloads"));
string oemExe=Path.Combine(portableRoot,"OEM","Sidebar.exe"),downloadExe=Path.Combine(portableRoot,"Downloads","Sidebar.exe");
File.WriteAllText(oemExe,"");File.WriteAllText(downloadExe,"");
try
{
    Require(StartupIdentity.MutexName(oemExe)!=StartupIdentity.MutexName(downloadExe),"A downloaded portable copy cannot activate the OEM instance");
    Require(StartupIdentity.MutexName(oemExe)==StartupIdentity.MutexName(oemExe.ToLowerInvariant()),"Windows path casing does not create duplicate instances");
    Require(StartupIdentity.SameExecutable(oemExe,Path.Combine(portableRoot,"OEM","..","OEM","Sidebar.exe")),"Equivalent executable paths identify one installation");
    Require(!StartupIdentity.MatchesTaskTarget(downloadExe,oemExe),"An OEM launch cannot redirect to a Downloads startup task");
    Require(StartupIdentity.MatchesTaskTarget(oemExe,oemExe),"The matching installed executable can use elevated startup");
    Require(!StartupIdentity.MatchesTaskTarget(oemExe+".missing",oemExe+".missing"),"A stale startup task cannot stop a portable launch");
    Require(!StartupIdentity.SameExecutable(null,oemExe),"An unavailable executable path never matches another copy");
    using var oemSignal=new EventWaitHandle(false,EventResetMode.AutoReset,StartupIdentity.ShowEventName(oemExe));
    using var downloadSignal=new EventWaitHandle(false,EventResetMode.AutoReset,StartupIdentity.ShowEventName(downloadExe));
    var childStart=new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true};
    childStart.ArgumentList.Add("--signal-startup-instance");childStart.ArgumentList.Add(downloadExe);
    using var child=System.Diagnostics.Process.Start(childStart)!;
    Require(downloadSignal.WaitOne(10000),"A second process reaches its own portable instance");
    Require(child.WaitForExit(10000)&&child.ExitCode==0&&!oemSignal.WaitOne(0),"A running OEM instance receives no activation from Downloads");
}
finally { Directory.Delete(portableRoot,true); }
Require(new Metric("","",null,"°C").Display(true)=="—","Unavailable values remain unavailable");
Require(new Metric("","",double.NaN,"%").Display(false)=="—","Invalid numeric data is not displayed");
Require(new Metric("","",100,"°C").Display(true)=="212 °F","Celsius to Fahrenheit");
Require(new Metric("","",1024,"B/s").Display(false)=="1 KB/s","Network byte-rate scaling");
Require(new Metric("","",0,"RPM").Display(false)=="0 RPM","Stopped fan is a valid zero");
Require(new Metric("","",3600,"MHz").Display(false).EndsWith("GHz"),"CPU clock unit conversion");
var prefs=new Preferences { Monitor="DISPLAY-TEST",ColumnWidth=310,Columns=2,Opacity=.8,Pins=new(){"gpu/test"},HiddenDevices=new(){"volume/test"} };
prefs.Save(); var loaded=Preferences.Load();
Require(loaded.Monitor=="DISPLAY-TEST" && loaded.Columns==2 && loaded.Pins.Contains("gpu/test") && loaded.HiddenDevices.Contains("volume/test"),"Settings survive restart");
File.WriteAllText(Preferences.Path,"{\"ColumnWidth\":-1,\"Columns\":99,\"Opacity\":0,\"Interval\":-100}"); loaded=Preferences.Load();
Require(loaded.ColumnWidth>=50 && loaded.Columns<=4 && loaded.Opacity==0 && loaded.Interval>=500,"Invalid settings cannot break layout or polling");
File.WriteAllText(Preferences.Path,"{broken");
Require(Preferences.Load().Columns==1,"Corrupt configuration recovers to defaults");
Require(DesignCatalog.All.Count>=12 && DesignCatalog.All.Select(d=>d.Key).Distinct().Count()==DesignCatalog.All.Count,"Design choices are uniquely addressable");
Require(DesignCatalog.Get("Original").Layout=="Original" && DesignCatalog.Get("Graphite").Key=="Graphite" && DesignCatalog.Get("Light").Key=="Light","Original and existing saved designs resolve correctly");
double Luminance(string color)
{
 double Component(int offset){double v=Convert.ToInt32(color.Substring(offset,2),16)/255d;return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4);}
 return .2126*Component(1)+.7152*Component(3)+.0722*Component(5);
}
double Contrast(string a,string b){double x=Luminance(a),y=Luminance(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
foreach(var design in DesignCatalog.All)
 Require(Contrast(design.Text,design.Panel)>=4.5 && Contrast(design.Muted,design.Panel)>=4.5,"Readable text contrast: "+design.Name);
var temps=new[]{new SensorRow("/cpu/0/die","Ryzen X3D","Core (Tctl/Tdie)","Temperature",64,"°C","Cpu"),new SensorRow("/lpc/cpu","Nuvoton","CPU","Temperature",52,"°C","SuperIO"),new SensorRow("/gpu/temp","RTX","GPU Core","Temperature",48,"°C","GpuNvidia")};
Require(CpuTemperatureSelector.Select("/cpu/0",temps,null)?.Value==52,"CPU socket is preferred over CPU diode");
Require(CpuTemperatureSelector.Select("/cpu/0",temps,"/cpu/0/die")?.Value==64,"Explicit CPU sensor selection is honored");
Require(CpuTemperatureSelector.Select("/cpu/0",temps,"/missing")==null,"Missing explicit CPU sensor is not silently substituted");
Require(CpuTemperatureSelector.Select("/cpu/0",temps.Where(s=>s.HardwareKind!="SuperIO"),null)?.Value==64,"Die fallback remains available on boards without a socket sensor");
Require(CpuTemperatureSelector.Select("/cpu/0",temps.Where(s=>s.HardwareKind=="GpuNvidia"),null)==null,"GPU temperature cannot become the CPU temperature");
var multi=new[]{new SensorRow("/board/cpu1","Board","CPU 1","Temperature",40,"°C","SuperIO"),new SensorRow("/board/cpu2","Board","CPU 2","Temperature",50,"°C","SuperIO")};
Require(CpuTemperatureSelector.Select("/cpu/1",multi,null,2,2)?.Value==50,"Multiple CPU sockets remain distinct");
var pin=new Preferences {HiddenCategories=new(){"Custom"},HiddenDevices=new(){"custom"}};pin.SetPinned("/cpu/ccd",true);
Require(pin.Pins.Contains("/cpu/ccd")&&!pin.HiddenCategories.Contains("Custom")&&!pin.HiddenDevices.Contains("custom"),"Pinning reveals a previously hidden custom card");
pin.Layout="Dashboard";pin.CpuTemperatureSources["/cpu/0"]="/lpc/cpu";pin.Save();var restored=Preferences.Load();
Require(restored.Pins.Contains("/cpu/ccd")&&restored.Layout=="Dashboard"&&restored.CpuTemperatureSources["/cpu/0"]=="/lpc/cpu","Pins, layout and CPU choice survive restart");
pin.SetPinned("/cpu/ccd",false);Require(pin.Pins.Count==0,"Unpinning removes the sensor");
Require(new Metric("n","Download",.1684,"MB/s").Display(false)=="0 MB/s","Small network rates display zero without threshold symbols");
Require(new Metric("n","Download",21.6,"MB/s").Display(false)=="21 MB/s","Network speed displays whole MB/s");
Require(new Metric("n","Download",0,"MB/s").Display(false)=="0 MB/s","No traffic displays zero MB/s");
File.WriteAllText(Preferences.Path,"{\"Theme\":\"OLED\",\"Pins\":[\"/cpu/ccd\"],\"HiddenCategories\":[\"Custom\"],\"HiddenDevices\":[\"custom\"]}");var oldPin=Preferences.Load();
Require(oldPin.Pins.Contains("/cpu/ccd")&&!oldPin.HiddenCategories.Contains("Custom")&&!oldPin.HiddenDevices.Contains("custom")&&oldPin.Layout=="Modern","Existing invisible pins and saved layout migrate correctly");
var metricPrefs=new Preferences();metricPrefs.SetMetricVisible("cpu","ccd1",false);
Require(!metricPrefs.IsMetricVisible("cpu","ccd1")&&metricPrefs.IsMetricVisible("custom","ccd1")&&metricPrefs.IsMetricVisible("cpu","temperature"),"Hiding a card value is scoped to that card");
metricPrefs.Save();Require(!Preferences.Load().IsMetricVisible("cpu","ccd1"),"Hidden card values survive restart");
metricPrefs.SetMetricVisible("cpu","ccd1",true);Require(metricPrefs.IsMetricVisible("cpu","ccd1"),"Hidden card value can be enabled again");
Require(!new Metric("n","Download",.001,"MB/s").Display(false).Contains('<'),"Network formatting never inserts a less-than marker");
var pinSnapshot=new Snapshot(new(){new("/cpu/0","CPU","CPU",new(){new("/cpu/0/load","Auslastung",20,"%")}),new("/gpu/0","GPU","GPU A",new()),new("/gpu/1","GPU","GPU B",new()),new("windows/ram","RAM","RAM",new())},new(){new("/cpu/0/ccd","CPU","CCD1","Temperature",52,"°C","Cpu"),new("/gpu/1/hotspot","GPU B","Hotspot","Temperature",70,"°C","GpuNvidia"),new("/board/fan","Board","CPU Fan","Fan",1200,"RPM","SuperIO"),new("/dimm/0/temp","DIMM","Temperatur","Temperature",42,"°C","Memory")},new(),DateTime.Now);
var widgetPins=new Preferences{Pins=new(){"/cpu/0/ccd","/gpu/1/hotspot","/board/fan","/dimm/0/temp"}};
var composed=PinPlacement.Compose(pinSnapshot,widgetPins);
Require(composed.First(d=>d.Id=="/cpu/0").Metrics.Any(m=>m.Id=="/cpu/0/ccd"),"CPU pins appear in their own CPU widget");
Require(composed.First(d=>d.Id=="/gpu/1").Metrics.Count==1&&composed.First(d=>d.Id=="/gpu/0").Metrics.Count==0,"Pins keep separate GPUs distinct");
Require(composed.First(d=>d.Id=="windows/ram").Metrics.Any(m=>m.Value==42),"Memory sensor pins can join the Windows RAM widget");
Require(composed.First(d=>d.Id=="custom").Metrics.Single().Id=="/board/fan","Unassigned board sensors remain in a separate card");
widgetPins.PinTargets["/board/fan"]="/cpu/0";composed=PinPlacement.Compose(pinSnapshot,widgetPins);
Require(composed.All(d=>d.Id!="custom")&&composed.First(d=>d.Id=="/cpu/0").Metrics.Any(m=>m.Unit=="RPM"),"Manual widget assignment attaches board fans to CPU");
widgetPins.Pins.Add("/cpu/0/load");composed=PinPlacement.Compose(pinSnapshot,widgetPins);Require(composed.First(d=>d.Id=="/cpu/0").Metrics.Count(m=>m.Id=="/cpu/0/load")==1,"Pinning an existing widget value never duplicates its row");
widgetPins.Save();Require(Preferences.Load().PinTargets["/board/fan"]=="/cpu/0","Widget assignments survive restart");
widgetPins.PinTargets["/board/fan"]="offline-device";composed=PinPlacement.Compose(pinSnapshot,widgetPins);Require(composed.First(d=>d.Id=="custom").Metrics.Any(m=>m.Id=="/board/fan"),"Offline target keeps pinned values accessible");
widgetPins.PinTargets.Remove("/board/fan");PinPlacement.SetPinned(pinSnapshot,widgetPins,"/cpu/0/load",false);
Require(!widgetPins.Pins.Contains("/cpu/0/load")&&!widgetPins.IsMetricVisible("/cpu/0","/cpu/0/load"),"Removing a pinned built-in value also hides its original widget row");
widgetPins.Save();Require(!Preferences.Load().IsMetricVisible("/cpu/0","/cpu/0/load"),"Removed widget rows stay removed after restart");
PinPlacement.SetPinned(pinSnapshot,widgetPins,"/cpu/0/load",true);Require(widgetPins.IsMetricVisible("/cpu/0","/cpu/0/load"),"Explicitly adding the sensor restores its widget row");
Require(DesignCatalog.All.All(d=>d.ToString()==d.Name),"Theme selection displays readable names instead of technical record data");
widgetPins.SettingsLanguage="fr";widgetPins.Save();Require(Preferences.Load().SettingsLanguage=="fr","Settings language survives restart independently of sidebar rendering");
Console.WriteLine($"{passed} checks passed");

var customColors=new Preferences{Theme="Graphite",TextColor="#ABCDEF",LabelColor="#8899AA",HardwareNameColor="#FFFFCC",ValueColor="#F1F2F3",GraphTrackColor="#112233",SecondaryGraphColor="#FFCC99",GraphScale=1.6,GraphColors=new(){{"CPU","#33CCFF"}}};customColors.Save();var restoredColors=Preferences.Load();
Require(restoredColors.HardwareNameColor=="#FFFFCC"&&restoredColors.ValueColor=="#F1F2F3"&&restoredColors.GraphColors["CPU"]=="#33CCFF"&&restoredColors.GraphScale==1.6,"All custom colors and graph thickness survive restart");
var resolvedColors=DesignCatalog.Resolve(restoredColors);Require(resolvedColors.Text=="#ABCDEF"&&resolvedColors.Muted=="#8899AA"&&resolvedColors.Track=="#112233","Custom text, label and track colors override the selected theme");
Require(DesignCatalog.CategoryColor(resolvedColors,"CPU",restoredColors)=="#33CCFF"&&DesignCatalog.CategoryColor(resolvedColors,"GPU",restoredColors)==DesignCatalog.CategoryColor(resolvedColors,"GPU"),"Graph accents override only the selected hardware category");
Require(DesignCatalog.ColorOr("#a1b2c3","#000000")=="#A1B2C3"&&DesignCatalog.ColorOr("not-a-color","#000000")=="#000000"&&DesignCatalog.ColorOr(null,"#000000")=="#000000","Invalid or missing colors fall back safely");
Require(DesignCatalog.Resolve(new Preferences{Theme="Graphite"})==DesignCatalog.Get("Graphite"),"Without customizations the original theme stays identical");
File.WriteAllText(Preferences.Path,"{\"GraphColors\":null,\"GraphScale\":999}");var repairedColors=Preferences.Load();Require(repairedColors.GraphColors.Count==0&&repairedColors.GraphScale==1.6,"Malformed customization settings cannot break chart geometry");
Console.WriteLine($"{passed} total checks passed");

var namedHardware=new Preferences{TextFontFamily="Arial"};
namedHardware.SetHardwareName("gpu/0","  Gaming GPU  ");namedHardware.SetHardwareName("gpu/1","Second GPU");namedHardware.Save();var namedReloaded=Preferences.Load();
Require(namedReloaded.HardwareName("gpu/0","Original GPU")=="Gaming GPU"&&namedReloaded.HardwareName("gpu/1","Original GPU")=="Second GPU"&&namedReloaded.TextFontFamily=="Arial","Custom names are trimmed, scoped by stable device ID and persist with the font");
Require(namedReloaded.HardwareName("gpu/2","New GPU")=="New GPU","Unrenamed devices retain their detected names");
namedReloaded.SetHardwareName("gpu/0","  ");Require(namedReloaded.HardwareName("gpu/0","Original GPU")=="Original GPU"&&namedReloaded.HardwareName("gpu/1","Original GPU")=="Second GPU","Blank names restore only the selected device");
namedReloaded.SetHardwareName("gpu/1",null);namedReloaded.Save();Require(Preferences.Load().HardwareNames.Count==0,"Reset hardware names remain reset after restart");
File.WriteAllText(Preferences.Path,"{\"HardwareNames\":null,\"TextFontFamily\":null}");var legacyNames=Preferences.Load();Require(legacyNames.HardwareNames.Count==0&&legacyNames.TextFontFamily=="","Null customization settings recover safely");
var typography=new Preferences{TitleTextScale=1.2,HardwareTextScale=1.3,LabelTextScale=.8,ValueTextScale=1.5,InstrumentTextScale=1.8,UnitTextScale=.7};typography.Save();var typographySaved=Preferences.Load();Require(typographySaved.TitleTextScale==1.2&&typographySaved.HardwareTextScale==1.3&&typographySaved.LabelTextScale==.8&&typographySaved.ValueTextScale==1.5&&typographySaved.InstrumentTextScale==1.8&&typographySaved.UnitTextScale==.7,"All six individual text sizes survive restart");File.WriteAllText(Preferences.Path,"{\"TitleTextScale\":999,\"HardwareTextScale\":-2,\"LabelTextScale\":0,\"ValueTextScale\":99,\"InstrumentTextScale\":-1,\"UnitTextScale\":99}");var safeTypography=Preferences.Load();Require(safeTypography.TitleTextScale==2&&safeTypography.HardwareTextScale==.5&&safeTypography.LabelTextScale==.5&&safeTypography.ValueTextScale==2&&safeTypography.InstrumentTextScale==.5&&safeTypography.UnitTextScale==2,"Invalid individual text sizes clamp safely");Require(Preferences.TextFactor(double.NaN)==1&&Preferences.TextFactor(double.PositiveInfinity)==1,"Non-finite individual text sizes use the default");
var figurePrefs=new Preferences{InstrumentScale=1.5};figurePrefs.Save();Require(Preferences.Load().InstrumentScale==1.5,"Whole instrument size survives restart");File.WriteAllText(Preferences.Path,"{\"InstrumentScale\":999}");Require(Preferences.Load().InstrumentScale==2,"Whole instrument size clamps safely");
var narrowPrefs=new Preferences{ColumnWidth=50};narrowPrefs.Save();Require(Preferences.Load().ColumnWidth==50,"50 pixel column width survives restart");
var netPrefs=new Preferences();string adapter="network/test";Require(!netPrefs.IsMetricVisible(adapter,adapter+"/internal-ip")&&!netPrefs.IsMetricVisible(adapter,adapter+"/external-ip"),"IP address display is optional and off by default");netPrefs.SetMetricVisible(adapter,adapter+"/internal-ip",true);netPrefs.NetworkAddresses[adapter]=new("192.0.2.10","203.0.113.20",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow);netPrefs.ColumnWidth=200;netPrefs.FontScale=.65;netPrefs.Save();var savedNet=Preferences.Load();Require(savedNet.NetworkAddresses[adapter].ExternalIP=="203.0.113.20"&&savedNet.IsMetricVisible(adapter,adapter+"/internal-ip")&&savedNet.ColumnWidth==200&&savedNet.FontScale==.65,"Addresses, visibility and smaller widget sizes survive restart");
var addressSnapshot=new Snapshot(new(){new(adapter,"Network","Test adapter",new(){new(adapter+"rx","Download",1,"MB/s")})},new(),new(),DateTime.Now);var addressMetrics=PinPlacement.Compose(addressSnapshot,savedNet)[0].Metrics;Require(addressMetrics.Count==3&&addressMetrics.Last().Display(false)=="203.0.113.20","Network composition displays saved IP text without a live lookup");
var slowHandler=new AddressHandler{Slow=true};using var slowClient=new System.Net.Http.HttpClient(slowHandler);var timedLookup=new NetworkAddressLookup(slowClient,_=>"192.0.2.12",TimeSpan.FromMilliseconds(40));var timeoutResult=await timedLookup.FetchAsync(adapter,savedNet.NetworkAddresses[adapter],true);Require(timeoutResult.ExternalFailed&&timeoutResult.State.ExternalIP=="203.0.113.20","A timed-out IP request ends without removing the saved address");
var fakeHandler=new AddressHandler();using var fakeClient=new System.Net.Http.HttpClient(fakeHandler);var lookup=new NetworkAddressLookup(fakeClient,_=>"192.0.2.11");Require(fakeHandler.Calls==0,"Creating the IP lookup helper sends no request");var localOnly=await lookup.FetchAsync(adapter,savedNet.NetworkAddresses[adapter],false);Require(fakeHandler.Calls==0&&localOnly.State.InternalIP=="192.0.2.11"&&localOnly.State.ExternalIP=="203.0.113.20","Local-only refresh sends no public IP request and preserves the saved external IP");var manual=await lookup.FetchAsync(adapter,localOnly.State,true);Require(fakeHandler.Calls==1&&manual.State.ExternalIP=="203.0.113.99"&&!manual.ExternalFailed,"One explicit external refresh sends exactly one request");fakeHandler.Fail=true;var failed=await lookup.FetchAsync(adapter,manual.State,true);Require(failed.ExternalFailed&&failed.State.ExternalIP==manual.State.ExternalIP&&failed.State.ExternalUpdated==manual.State.ExternalUpdated,"Network failure preserves the previous external address and timestamp");fakeHandler.Fail=false;fakeHandler.Reply=new string('x',200);var malformed=await lookup.FetchAsync(adapter,manual.State,true);Require(malformed.ExternalFailed&&malformed.State.ExternalIP==manual.State.ExternalIP,"Oversized or malformed public IP responses cannot overwrite stored values");fakeHandler.Reply="2001:db8:abcd:1234:ffff:ffff:ffff:abcd";var ipv6=await lookup.FetchAsync(adapter,manual.State,true);Require(ipv6.State.ExternalIP==fakeHandler.Reply,"IPv6 text is supported without numeric formatting");using var cancellation=new CancellationTokenSource();cancellation.Cancel();try{await lookup.FetchAsync(adapter,manual.State,true,cancellation.Token);Require(false,"Canceled lookup must not run");}catch(OperationCanceledException){Require(true,"Closing or cancellation prevents further IP work");}
var history=new MetricHistory();var historyTime=new DateTime(2026,10,27,12,0,0);var historyDevice=new Device("history/cpu","CPU","History CPU",new(){new("load","Auslastung",0,"%")});
Require(history.Read(historyDevice.Id,"load").Count==0,"History starts empty without synthetic samples");history.Record(new[]{historyDevice},historyTime);history.Record(new[]{historyDevice},historyTime);Require(history.Read(historyDevice.Id,"load").Count==1&&history.Read(historyDevice.Id,"load")[0].Value==0,"History preserves a real zero and deduplicates rebuilt snapshots");
history.Record(new[]{historyDevice with{Id="history/other",Metrics=new(){new("load","Auslastung",80,"%")}}},historyTime);Require(history.Read(historyDevice.Id,"load")[0].Value==0&&history.Read("history/other","load")[0].Value==80,"History isolates equal metric IDs on different devices");
history.Record(new[]{historyDevice with{Metrics=new(){new("load","Auslastung",double.NaN,"%")}}},historyTime.AddSeconds(1));history.Record(new[]{historyDevice with{Metrics=new(){new("load","Auslastung",null,"%")}}},historyTime.AddSeconds(2));Require(history.Read(historyDevice.Id,"load").Skip(1).All(p=>p.Value==null),"Missing and invalid history samples remain gaps rather than zeros");
for(int i=3;i<1000;i++)history.Record(new[]{historyDevice},historyTime.AddMilliseconds(i*250));var points=history.Read(historyDevice.Id,"load");Require(points.Count<=MetricHistory.MaximumSamples&&points.All(p=>p.Time>=history.LatestTime-MetricHistory.Window),"History retains only a bounded 60-second window");var unchanged=points.Count;history.Record(new[]{historyDevice},history.LatestTime.AddSeconds(-1));Require(history.Read(historyDevice.Id,"load").Count==unchanged,"An out-of-order snapshot cannot corrupt current history");history.Record(new[]{historyDevice},historyTime.AddHours(-1));Require(history.Read(historyDevice.Id,"load").Count==1,"A backwards clock adjustment resets the window instead of freezing graphs");
var manyDevices=Enumerable.Range(0,700).Select(i=>historyDevice with{Id="bounded/"+i}).ToArray();history.Record(manyDevices,historyTime.AddHours(1));Require(history.SeriesCount==MetricHistory.MaximumSeries,"History series count stays bounded across device changes");history.Record(Array.Empty<Device>(),historyTime.AddHours(2));Require(history.SeriesCount==0,"Expired disconnected devices are removed from history");
var legacyLargePrefs=new Preferences{Layout="TwinRings.Large",FontScale=1.2,ValueTextScale=1.3};legacyLargePrefs.Save();var migratedLargePrefs=Preferences.Load();Require(migratedLargePrefs.Layout=="TwinRings"&&migratedLargePrefs.FontScale==1.2&&migratedLargePrefs.ValueTextScale==1.3,"Removed large-reading design migrates without changing the user text sliders");
Require(new Metric("fraction","Load",31.7,"%").Display(false)=="31 %","Whole displayed readings truncate rather than round");
Require(new Metric("voltage","Voltage",.73,"V").Display(false)=="0.730 V","Sub-unit voltage shows three decimals");
Require(new Metric("voltage","Voltage",1.2564,"V").Display(false)=="1.256 V","Voltages above one keep three decimals");
Require(new Metric("clock","Clock",3870,"MHz").Display(false)=="3.87 GHz","Generic clocks show two decimals in GHz");Require(new Metric("clock","Clock",795,"MHz").Display(false)=="795 MHz","Clocks below one GHz stay whole MHz");
Require(new Metric("bytes","Read",31.7*1024,"B/s").Display(false)=="31 KB/s","Byte rates of ten and more truncate after unit scaling");Require(new Metric("bytes","Read",1.5*1024*1024*1024,"B/s").Display(false)=="1.5 GB/s","Byte rates below ten keep one decimal");
Require(new Metric("memory","Used",13.94,"GB").Display(false)=="13.9 GB","Memory amounts keep one decimal");Require(new Metric("drive","Capacity",1863.02,"GB").Display(false)=="1863 GB","Large capacities use whole gigabytes");Require(new Metric("link","Adapter-Link",2.5,"Gbit/s").Display(false)=="2.5 Gbit/s","Link speeds keep their fraction");Require(new Metric("power","Power",88.7,"W").Display(false)=="88 W","Whole-number units are unchanged");
Require(new Metric("negative","Temperature",-3.7,"°C").Display(false)=="-3 °C","Negative readings truncate toward zero");
Require(new Metric("ip","IP",null,"IP",TextValue:"192.168.0.119").Display(false)=="192.168.0.119","Text and IP addresses preserve punctuation");


var roleFlags=new[]{"ShowWidgetTitles","ShowHardwareNames","ShowLabels","ShowReadings","ShowInstrumentNumbers","ShowInstrumentUnits"};File.WriteAllText(Preferences.Path,"{}");var legacyVisibility=Preferences.Load();Require(roleFlags.All(name=>(bool)typeof(Preferences).GetProperty(name)!.GetValue(legacyVisibility)!),"Old configurations keep all sidebar text visible by default");foreach(var name in roleFlags){var p=new Preferences();typeof(Preferences).GetProperty(name)!.SetValue(p,false);p.Save();var read=Preferences.Load();Require(roleFlags.All(other=>(bool)typeof(Preferences).GetProperty(other)!.GetValue(read)! == (other!=name)),"Text visibility persists independently: "+name);}var allHidden=new Preferences{ShowWidgetTitles=false,ShowHardwareNames=false,ShowLabels=false,ShowReadings=false,ShowInstrumentNumbers=false,ShowInstrumentUnits=false};allHidden.Save();Require(roleFlags.All(name=>!(bool)typeof(Preferences).GetProperty(name)!.GetValue(Preferences.Load())!),"All hidden text choices survive restart");
Console.WriteLine($"{passed} total checks passed");
sealed class AddressHandler:System.Net.Http.HttpMessageHandler{
 public int Calls;public bool Fail;public bool Slow;public string Reply="203.0.113.99";
 protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request,CancellationToken cancellation){Calls++;if(Slow)await Task.Delay(1000,cancellation);if(request.RequestUri?.Host!="api64.ipify.org")throw new Exception("Unexpected IP endpoint");if(Fail)throw new System.Net.Http.HttpRequestException("Offline");return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new System.Net.Http.StringContent(Reply)};}
}

static class TestProcessSafety{[System.Runtime.InteropServices.DllImport("kernel32.dll")]internal static extern uint SetErrorMode(uint mode);}



 
