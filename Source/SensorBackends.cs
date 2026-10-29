using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using OHM = OpenHardwareMonitor.Hardware;
namespace Aurora;

public sealed record HardwareSample(string Identifier,string Name,string HardwareType,SensorSample[] Sensors,string Provider="",string PreferenceDeviceId="",Dictionary<string,string>? PreferenceSensorIds=null);
public sealed record SensorSample(string Identifier,string Name,SensorType SensorType,double? Value);
public interface ISensorBackend : IDisposable {void Open();HardwareSample[] Capture(List<string> warnings);}

// Owned exclusively by SensorService's worker. Close before switching, never from the UI thread.
public sealed class SensorBackendSession : IDisposable
{
    private readonly Func<string,ISensorBackend> create;
    private ISensorBackend? backend;
    public string ActiveSource {get;private set;}="";
    public SensorBackendSession(Func<string,ISensorBackend>? factory=null)=>create=factory??(source=>source==SensorSources.OpenPawn?new OpenPawnIoSensorBackend():new LibreSensorBackend());
    public void Select(string source)
    {
        source=SensorSources.Normalize(source);
        if(backend!=null&&source==ActiveSource)return;
        Dispose();
        var next=create(source);
        try{next.Open();backend=next;ActiveSource=source;}
        catch{next.Dispose();throw;}
    }
    public HardwareSample[] Capture(List<string> warnings)=>backend?.Capture(warnings)??Array.Empty<HardwareSample>();
    public void Dispose(){backend?.Dispose();backend=null;ActiveSource="";}
}

internal sealed class LibreSensorBackend : ISensorBackend
{
    private readonly Computer computer=new(){IsCpuEnabled=true,IsGpuEnabled=true,IsMemoryEnabled=true,IsMotherboardEnabled=true,IsStorageEnabled=true,IsNetworkEnabled=false,IsControllerEnabled=true};
    public void Open()=>computer.Open();
    public void Dispose()=>computer.Close();
    private static IEnumerable<IHardware> Flatten(IHardware hw){yield return hw;foreach(var sub in hw.SubHardware)foreach(var item in Flatten(sub))yield return item;}
    public HardwareSample[] Capture(List<string> warnings)
    {
        var result=new List<HardwareSample>();
        foreach(var hw in computer.Hardware.SelectMany(Flatten))
        {
            try{hw.Update();result.Add(new(hw.Identifier.ToString(),hw.Name,hw.HardwareType.ToString(),hw.Sensors.Select(s=>new SensorSample(s.Identifier.ToString(),s.Name,s.SensorType,SensorSources.Finite(s.Value))).ToArray(),"LibreHardwareMonitor/PawnIO"));}
            catch(Exception error){warnings.Add(hw.Name+": "+error.Message);}
        }
        return result.ToArray();
    }
}

// A deliberately labelled hybrid, not an upstream OHM PawnIO port. The reduced OHM
// source has no kernel access. Libre owns all CPU/mainboard access through PawnIO.
internal sealed class OpenPawnIoSensorBackend : ISensorBackend
{
    private readonly OHM.DriverlessComputer direct=new();
    private readonly LibreSensorBackend pawn=new();
    private bool directAvailable;
    private string? directError;
    public void Open()
    {
        try{direct.Open();directAvailable=true;}
        catch(Exception error){directError=error.Message;direct.Dispose();}
        try{pawn.Open();}catch{direct.Dispose();throw;}
    }
    public void Dispose(){try{direct.Dispose();}finally{pawn.Dispose();}}
    public HardwareSample[] Capture(List<string> warnings)
    {
        var ohm=new List<HardwareSample>();
        if(directError!=null)warnings.Add("Open Hardware Monitor: "+directError);
        if(directAvailable)
        {
            warnings.AddRange(direct.Diagnostics.Select(s=>"Open Hardware Monitor: "+s));
            foreach(var hw in direct.Hardware)
            {
                try
                {
                    hw.Update();
                    var sensors=hw.Sensors.Where(s=>Enum.TryParse<SensorType>(s.SensorType.ToString(),out _)).Select(s=>new SensorSample(SensorSources.OpenId(s.Identifier.ToString()),s.Name,Enum.Parse<SensorType>(s.SensorType.ToString()),SensorSources.Finite(s.Value))).ToArray();
                    ohm.Add(new(SensorSources.OpenId(hw.Identifier.ToString()),hw.Name,SensorSources.HardwareKind(hw.HardwareType.ToString()),sensors,"Open Hardware Monitor (vendor API/SMART)"));
                }
                catch(Exception error){warnings.Add("Open Hardware Monitor · "+hw.Name+": "+error.Message);}
            }
        }
        return SensorBackendMerge.Combine(ohm,pawn.Capture(warnings));
    }
}

public static class SensorBackendMerge
{
    // Match repeated identical models one at a time; don't drop unmatched disks or a second GPU.
    public static HardwareSample[] Combine(IEnumerable<HardwareSample> direct,IEnumerable<HardwareSample> bridge)
    {
        var result=direct.Where(h=>h.Sensors.Any(s=>s.Value.HasValue)).ToList();
        var unmatched=new List<HardwareSample>(result);
        var fallback=bridge.ToArray();
        static string Key(string text)=>new(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        foreach(var hw in fallback)
        {
            bool directKind=hw.HardwareType=="Storage"||hw.HardwareType.StartsWith("Gpu",StringComparison.Ordinal);
            var match=directKind?unmatched.FirstOrDefault(h=>h.HardwareType==hw.HardwareType&&Key(h.Name)==Key(hw.Name)):null;
            if(match==null&&directKind&&result.Count(h=>h.HardwareType==hw.HardwareType)==1&&fallback.Count(h=>h.HardwareType==hw.HardwareType)==1)
                match=unmatched.FirstOrDefault(h=>h.HardwareType==hw.HardwareType);
            if(match!=null)
            {
                var aliases=new Dictionary<string,string>();
                foreach(var sensor in match.Sensors)
                {
                    var reference=hw.Sensors.FirstOrDefault(s=>s.SensorType==sensor.SensorType&&s.Name.Equals(sensor.Name,StringComparison.OrdinalIgnoreCase));
                    if(reference!=null)aliases[sensor.Identifier]=reference.Identifier;
                }
                int index=result.IndexOf(match);
                result[index]=match with{PreferenceDeviceId=hw.Identifier,PreferenceSensorIds=aliases};
                unmatched.Remove(match);continue;
            }
            result.Add(hw with{Identifier=SensorSources.BridgeId(hw.Identifier),Sensors=hw.Sensors.Select(s=>s with{Identifier=SensorSources.BridgeId(s.Identifier)}).ToArray(),Provider=directKind?"LibreHardwareMonitor fallback":"LibreHardwareMonitor/PawnIO bridge",PreferenceDeviceId=hw.Identifier,PreferenceSensorIds=hw.Sensors.ToDictionary(s=>SensorSources.BridgeId(s.Identifier),s=>s.Identifier)});
        }
        return result.ToArray();
    }
}
