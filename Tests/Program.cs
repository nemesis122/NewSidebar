using Aurora;
using System.Text.Json;
if(OperatingSystem.IsWindows())TestProcessSafety.SetErrorMode(3);
int passed=0;
void Require(bool condition,string label) { if(!condition) throw new Exception(label); Console.WriteLine("PASS "+label); passed++; }
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
Require(loaded.ColumnWidth>=200 && loaded.Columns<=4 && loaded.Opacity==0 && loaded.Interval>=500,"Invalid settings cannot break layout or polling");
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

var netPrefs=new Preferences();string adapter="network/test";Require(!netPrefs.IsMetricVisible(adapter,adapter+"/internal-ip")&&!netPrefs.IsMetricVisible(adapter,adapter+"/external-ip"),"IP address display is optional and off by default");netPrefs.SetMetricVisible(adapter,adapter+"/internal-ip",true);netPrefs.NetworkAddresses[adapter]=new("192.0.2.10","203.0.113.20",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow);netPrefs.ColumnWidth=200;netPrefs.FontScale=.65;netPrefs.Save();var savedNet=Preferences.Load();Require(savedNet.NetworkAddresses[adapter].ExternalIP=="203.0.113.20"&&savedNet.IsMetricVisible(adapter,adapter+"/internal-ip")&&savedNet.ColumnWidth==200&&savedNet.FontScale==.65,"Addresses, visibility and smaller widget sizes survive restart");
var addressSnapshot=new Snapshot(new(){new(adapter,"Network","Test adapter",new(){new(adapter+"rx","Download",1,"MB/s")})},new(),new(),DateTime.Now);var addressMetrics=PinPlacement.Compose(addressSnapshot,savedNet)[0].Metrics;Require(addressMetrics.Count==3&&addressMetrics.Last().Display(false)=="203.0.113.20","Network composition displays saved IP text without a live lookup");
var slowHandler=new AddressHandler{Slow=true};using var slowClient=new System.Net.Http.HttpClient(slowHandler);var timedLookup=new NetworkAddressLookup(slowClient,_=>"192.0.2.12",TimeSpan.FromMilliseconds(40));var timeoutResult=await timedLookup.FetchAsync(adapter,savedNet.NetworkAddresses[adapter],true);Require(timeoutResult.ExternalFailed&&timeoutResult.State.ExternalIP=="203.0.113.20","A timed-out IP request ends without removing the saved address");
var fakeHandler=new AddressHandler();using var fakeClient=new System.Net.Http.HttpClient(fakeHandler);var lookup=new NetworkAddressLookup(fakeClient,_=>"192.0.2.11");Require(fakeHandler.Calls==0,"Creating the IP lookup helper sends no request");var localOnly=await lookup.FetchAsync(adapter,savedNet.NetworkAddresses[adapter],false);Require(fakeHandler.Calls==0&&localOnly.State.InternalIP=="192.0.2.11"&&localOnly.State.ExternalIP=="203.0.113.20","Local-only refresh sends no public IP request and preserves the saved external IP");var manual=await lookup.FetchAsync(adapter,localOnly.State,true);Require(fakeHandler.Calls==1&&manual.State.ExternalIP=="203.0.113.99"&&!manual.ExternalFailed,"One explicit external refresh sends exactly one request");fakeHandler.Fail=true;var failed=await lookup.FetchAsync(adapter,manual.State,true);Require(failed.ExternalFailed&&failed.State.ExternalIP==manual.State.ExternalIP&&failed.State.ExternalUpdated==manual.State.ExternalUpdated,"Network failure preserves the previous external address and timestamp");fakeHandler.Fail=false;fakeHandler.Reply=new string('x',200);var malformed=await lookup.FetchAsync(adapter,manual.State,true);Require(malformed.ExternalFailed&&malformed.State.ExternalIP==manual.State.ExternalIP,"Oversized or malformed public IP responses cannot overwrite stored values");fakeHandler.Reply="2001:db8:abcd:1234:ffff:ffff:ffff:abcd";var ipv6=await lookup.FetchAsync(adapter,manual.State,true);Require(ipv6.State.ExternalIP==fakeHandler.Reply,"IPv6 text is supported without numeric formatting");using var cancellation=new CancellationTokenSource();cancellation.Cancel();try{await lookup.FetchAsync(adapter,manual.State,true,cancellation.Token);Require(false,"Canceled lookup must not run");}catch(OperationCanceledException){Require(true,"Closing or cancellation prevents further IP work");}
Console.WriteLine($"{passed} total checks passed");
sealed class AddressHandler:System.Net.Http.HttpMessageHandler{
 public int Calls;public bool Fail;public bool Slow;public string Reply="203.0.113.99";
 protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request,CancellationToken cancellation){Calls++;if(Slow)await Task.Delay(1000,cancellation);if(request.RequestUri?.Host!="api64.ipify.org")throw new Exception("Unexpected IP endpoint");if(Fail)throw new System.Net.Http.HttpRequestException("Offline");return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new System.Net.Http.StringContent(Reply)};}
}

static class TestProcessSafety{[System.Runtime.InteropServices.DllImport("kernel32.dll")]internal static extern uint SetErrorMode(uint mode);}
