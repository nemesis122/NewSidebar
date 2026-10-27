using System;
using System.Collections.Generic;
using System.Linq;
namespace Aurora;
public static class CpuTemperatureSelector
{
    public static SensorRow? Select(string cpuId,IEnumerable<SensorRow> all,string? preferred,int cpuNumber=1,int cpuCount=1)
    {
        var temperatures=all.Where(s=>s.Kind=="Temperature" && s.Value.HasValue && double.IsFinite(s.Value.Value) && s.Value!=0).ToArray();
        if(!string.IsNullOrWhiteSpace(preferred)) return temperatures.FirstOrDefault(s=>s.Id==preferred);
        string[] socketNames=cpuCount==1
            ? new[]{"CPU","CPU Socket","CPU (Socket)","CPU Temperature","CPU Core"}
            : new[]{"CPU #"+cpuNumber,"CPU "+cpuNumber,"CPU"+cpuNumber,"CPU "+cpuNumber+" Socket"};
        var boards=temperatures.Where(s=>s.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController").ToArray();
        foreach(var name in socketNames)
        {
            var match=boards.Where(s=>s.Name.Equals(name,StringComparison.OrdinalIgnoreCase)).OrderBy(s=>s.HardwareKind=="SuperIO"?0:s.HardwareKind=="EmbeddedController"?1:2).FirstOrDefault();
            if(match!=null)return match;
        }
        var own=temperatures.Where(s=>s.Id.StartsWith(cpuId+"/",StringComparison.Ordinal)).ToArray();
        foreach(var name in new[]{"CPU","CPU Socket","CPU Package","Core (Tctl/Tdie)","Core (Tdie)","Core Average"})
        {var match=own.FirstOrDefault(s=>s.Name.Equals(name,StringComparison.OrdinalIgnoreCase));if(match!=null)return match;}
        return own.FirstOrDefault();
    }
    public static string Label(SensorRow sensor)=>sensor.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController" ? "Temperatur" : sensor.Name.Contains("Package",StringComparison.OrdinalIgnoreCase)?"Temperatur (Package)":sensor.Name.Contains("Tdie",StringComparison.OrdinalIgnoreCase)||sensor.Name.Contains("Tctl",StringComparison.OrdinalIgnoreCase)||sensor.Name.Contains("Diode",StringComparison.OrdinalIgnoreCase)?"Temperatur (Die)":"Temperatur";
}
