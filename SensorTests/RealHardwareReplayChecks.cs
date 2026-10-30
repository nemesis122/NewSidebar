using Aurora;
using LibreHardwareMonitor.Hardware;
using System.Text.Json;

static class RealHardwareReplayChecks
{
 public static void Run(string path,Action<bool,string> check)
 {
  using var document=JsonDocument.Parse(File.ReadAllText(path));
  var snapshots=document.RootElement.GetProperty("Samples").EnumerateArray().Select(s=>s.GetProperty("Snapshot").Deserialize<Snapshot>()!).ToArray();
  HardwareSample[] Storage(Snapshot snapshot,bool stripPrefix)=>snapshot.Devices.Where(d=>d.Category=="Drives"&&!d.Id.StartsWith("volume/")).Select(d=>new HardwareSample(stripPrefix?d.Id.Replace("ohm/direct",""):d.Id,d.Name,"Storage",snapshot.Sensors.Where(s=>s.HardwareKind=="Storage"&&s.Id.StartsWith(d.Id+"/")).Select(s=>new SensorSample(stripPrefix?s.Id.Replace("ohm/direct",""):s.Id,s.Name,Enum.Parse<SensorType>(s.Kind),s.Value)).ToArray(),stripPrefix?"Open Hardware Monitor storage fallback":"LibreHardwareMonitor/PawnIO")).ToArray();
  var libre=snapshots.Where(s=>s.SensorSource==SensorSources.Libre).ToArray();var ohm=snapshots.Where(s=>s.SensorSource==SensorSources.OpenPawn).ToArray();
  check(libre.Length>0&&ohm.Length>0,"Live replay includes both administrator-measured sources");
  foreach(var pair in libre.Zip(ohm))
  {
   var original=Storage(pair.First,false);var alternative=Storage(pair.Second,true);
   var supplemented=SensorBackendMerge.AddMissingStorage(original,alternative);
   check(supplemented.Count(d=>d.Name.Contains("PM981a"))==1&&supplemented.Count(d=>d.Name=="SSDSC2KB019T7R")==1,"Real live storage replay recovers Samsung NVMe without duplicating Intel SSD");
   check(original.All(d=>supplemented.Any(s=>s.Identifier==d.Identifier&&s.Sensors.SequenceEqual(d.Sensors))),"Real live storage replay leaves existing Libre sensor IDs and values intact");
   var direct=alternative.Select(d=>d with{Identifier=SensorSources.OpenId(d.Identifier),Sensors=d.Sensors.Select(s=>s with{Identifier=SensorSources.OpenId(s.Identifier)}).ToArray()});
   var hybrid=SensorBackendMerge.Combine(direct,original);
   var intel=hybrid.Single(d=>d.Name=="SSDSC2KB019T7R");
   check(intel.Sensors.Any(s=>s.SensorType==SensorType.Temperature&&s.Value==35),"Real OHM replay retains the Intel SSD temperature from Libre");
   using var session=new SensorBackendSession(_=>new ReplayBackend(supplemented));
   var snapshot=new SensorService(session).Read();
   var nvme=snapshot.Devices.Single(d=>d.Name.Contains("PM981a"));
   var measured=alternative.Single(d=>d.Name.Contains("PM981a"));
   check(nvme.Metrics.Single(m=>m.Label=="Lesen").Value==measured.Sensors.Single(s=>s.Name=="Read Rate").Value&&nvme.Metrics.Single(m=>m.Label=="Schreiben").Value==measured.Sensors.Single(s=>s.Name=="Write Rate").Value&&!nvme.Metrics.Any(m=>m.Unit=="°C"),"Real NVMe replay preserves throughput including first-sample gaps without inventing a temperature");
  }
 }
 sealed class ReplayBackend(HardwareSample[] hardware):ISensorBackend{public void Open(){}public void Dispose(){}public HardwareSample[] Capture(List<string> warnings)=>hardware;}
}
