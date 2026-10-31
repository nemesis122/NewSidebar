using Aurora;
using LibreHardwareMonitor.Hardware;
// Synthetic vendor/name/sensor-shape regression, not a physical certification of these models.
static class ReleaseHardwareMatrixChecks
{
 public static void Run(Action<bool,string> check)
 {
  var cpus=new[]{("AMD Ryzen 7 9800X3D","Core (Tctl/Tdie)","Core #1"),("AMD Ryzen 9 9950X3D","Core (Tctl/Tdie)","Core #1"),("AMD Ryzen 9 5900X","CPU Package","Core #1"),("Intel Core i7-14700K","CPU Package","P-Core #1"),("Intel Core Ultra 9 285K","CPU Package","E-Core #1")};
  var gpus=new[]{("NVIDIA GeForce RTX 5090","GpuNvidia","GPU Core"),("AMD Radeon RX 7900 XT","GpuAmd","GPU Core"),("Intel Arc B580","GpuIntel","D3D 3D")};
  foreach(string source in new[]{SensorSources.Libre,SensorSources.OpenPawn})foreach(var cpu in cpus)foreach(var gpu in gpus)
  {
   string Id(string v)=>source==SensorSources.OpenPawn?SensorSources.BridgeId(v):v;
   SensorSample S(string id,string name,SensorType kind,double? value)=>new(Id(id),name,kind,value);
   var raw=new[]{new HardwareSample(Id("/cpu/matrix"),cpu.Item1,"Cpu",new[]{S("/cpu/matrix/load","CPU Total",SensorType.Load,24),S("/cpu/matrix/temp",cpu.Item2,SensorType.Temperature,62),S("/cpu/matrix/clock",cpu.Item3,SensorType.Clock,4800),S("/cpu/matrix/power","Package",SensorType.Power,78)},"Synthetic matrix"),new HardwareSample(Id("/board/matrix"),"Fixture board","SuperIO",new[]{S("/board/matrix/cpu","CPU",SensorType.Temperature,50)},"Synthetic matrix"),new HardwareSample(Id("/gpu/matrix"),gpu.Item1,gpu.Item2,new[]{S("/gpu/matrix/load",gpu.Item3,SensorType.Load,36),S("/gpu/matrix/temp","GPU Core",SensorType.Temperature,46),S("/gpu/matrix/clock","GPU Core",SensorType.Clock,1320),S("/gpu/matrix/fan","GPU Fan",SensorType.Fan,0)},"Synthetic matrix")};
   var backend=new Backend(raw);using var session=new SensorBackendSession(_=>backend);var service=new SensorService(session){Source=source};var result=service.Read();string label=source+" / "+cpu.Item1+" / "+gpu.Item1;
   var c=result.Devices.Single(d=>d.Id==Id("/cpu/matrix"));var g=result.Devices.Single(d=>d.Id==Id("/gpu/matrix"));
   check(c.Name==cpu.Item1&&g.Name==gpu.Item1,"CPU/GPU vendor names preserved: "+label);
   check(c.Metrics.Single(m=>m.Id.EndsWith("/display-temperature")).Value==50,"Available CPU socket is preferred over package/die: "+label);
   check(c.Metrics.Single(m=>m.IsCpuClock).Display(false)=="4.80 GHz","AMD and Intel hybrid clock naming keeps actual core precision: "+label);
   check(g.Metrics.Single(m=>m.Label=="Auslastung").Value==36,"Vendor GPU load maps the proper 3D/core sensor: "+label);
   check(g.Metrics.Single(m=>m.Unit=="°C").Value==46&&g.Metrics.Single(m=>m.Label=="Lüfter").Value==0,"GPU temperature and stopped fan are preserved: "+label);
   check(result.Devices.Any(d=>d.Id=="windows/ram")&&result.Sensors.Select(s=>s.Id).Distinct().Count()==result.Sensors.Count,"Windows RAM and unique sensor IDs survive source mapping: "+label);
   backend.Hardware=raw.Select(h=>h with{Sensors=h.Sensors.Select(s=>s.SensorType==SensorType.Temperature?s with{Value=null}:s).ToArray()}).ToArray();
   check(service.Read().Devices.Single(d=>d.Id==c.Id).Metrics.Single(m=>m.Id.EndsWith("/display-temperature")).Value==null,"Disappearing CPU temperatures clear for every vendor profile: "+label);
  }
 }
 sealed class Backend(HardwareSample[] hardware):ISensorBackend{public HardwareSample[] Hardware=hardware;public void Open(){}public void Dispose(){}public HardwareSample[] Capture(List<string> warnings)=>Hardware;}
}
