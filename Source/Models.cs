using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Aurora;

public sealed class Preferences
{
    public Dictionary<string,string> HardwareNames { get; set; } = new();
    public string TextFontFamily { get; set; } = "";
    public string HardwareName(string deviceId,string originalName) => HardwareNames.TryGetValue(deviceId,out var name) && !string.IsNullOrWhiteSpace(name) ? name : originalName;
    public void SetHardwareName(string deviceId,string? name)
    {
        if(string.IsNullOrWhiteSpace(name)) HardwareNames.Remove(deviceId);
        else HardwareNames[deviceId]=name.Trim();
    }
    public string Monitor { get; set; } = "";
    public string Side { get; set; } = "Rechts";
    public double ColumnWidth { get; set; } = 290;
    public int Columns { get; set; } = 1;
    public bool ExpandMultiColumnReadings { get; set; }
    public double TitleTextScale { get; set; } = 1;
    public double HardwareTextScale { get; set; } = 1;
    public double LabelTextScale { get; set; } = 1;
    public double ValueTextScale { get; set; } = 1;
    public double InstrumentTextScale { get; set; } = 1;
    public double UnitTextScale { get; set; } = 1;
    public double InstrumentScale { get; set; } = 1;
    public static double TextFactor(double value)=>double.IsFinite(value)?Math.Clamp(value,.5,2):1;
    public bool Topmost { get; set; } = false;
    public bool ReserveSpace { get; set; }
    public double Opacity { get; set; } = 1;
    public string TextColor { get; set; } = "";
    public string LabelColor { get; set; } = "";
    public string HardwareNameColor { get; set; } = "";
    public string ValueColor { get; set; } = "";
    public string GraphTrackColor { get; set; } = "";
    public string SecondaryGraphColor { get; set; } = "";
    public Dictionary<string,string> GraphColors { get; set; } = new();
    public double GraphScale { get; set; } = 1;
    public int Interval { get; set; } = 1000;
    public Dictionary<string,NetworkAddressState> NetworkAddresses { get; set; } = new();
    public List<string> VisibleNetworkAddresses { get; set; } = new();
    public bool Fit { get; set; } = false;
    public bool Fahrenheit { get; set; }
    public string Theme { get; set; } = "Original";
    public string Layout { get; set; } = "Original";
    public string SettingsLanguage { get; set; } = "de";
    public Dictionary<string,string> CpuTemperatureSources { get; set; } = new();
    public List<string> HiddenMetrics { get; set; } = new();
    public double FontScale { get; set; } = 1;
    public double TempLimit { get; set; } = 85;
    public double LoadLimit { get; set; } = 95;
    public List<string> HiddenCategories { get; set; } = new() { "Mainboard", "Controller" };
    public bool DeviceDefaultsApplied { get; set; }
    public List<string> HiddenDevices { get; set; } = new();
    public List<string> Pins { get; set; } = new();
    public Dictionary<string,string> PinTargets { get; set; } = new();
    public int PinVisibilityVersion { get; set; }
    public List<string> Order { get; set; } = new() { "CPU", "RAM", "GPU", "Drives", "Network", "Mainboard", "Controller", "Custom" };
    public static string Directory => System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
    public static string Path => System.IO.Path.Combine(Directory, "aurora-settings.json");
    public static Preferences Load()
    {
        try
        {
            var json=File.ReadAllText(Path);
            var p = JsonSerializer.Deserialize<Preferences>(json) ?? new();
            p.HardwareNames ??= new();
            p.TextFontFamily = p.TextFontFamily?.Trim() ?? "";
            using var document=JsonDocument.Parse(json);
            if(!document.RootElement.TryGetProperty("Layout",out _))p.Layout=p.Theme switch {"Original"=>"Original","Compact" or "Terminal"=>"Compact","Minimal"=>"Text",_=>"Modern"};
            if(p.Layout?.EndsWith(".Large",StringComparison.Ordinal)==true)p.Layout=p.Layout[..^6];
            p.ColumnWidth = double.IsFinite(p.ColumnWidth)?Math.Clamp(p.ColumnWidth, 50, 440):290;
            p.Columns = Math.Clamp(p.Columns, 1, 4);
            p.Opacity = Math.Clamp(p.Opacity, 0, 1);
            p.Interval = Math.Clamp(p.Interval, 500, 10000);
            p.FontScale = Math.Clamp(p.FontScale, .65, 1.4);
            p.TitleTextScale=TextFactor(p.TitleTextScale);p.HardwareTextScale=TextFactor(p.HardwareTextScale);p.LabelTextScale=TextFactor(p.LabelTextScale);p.ValueTextScale=TextFactor(p.ValueTextScale);p.InstrumentTextScale=TextFactor(p.InstrumentTextScale);p.UnitTextScale=TextFactor(p.UnitTextScale);
            p.InstrumentScale=TextFactor(p.InstrumentScale);
            p.GraphScale = double.IsFinite(p.GraphScale)?Math.Clamp(p.GraphScale,.5,1.6):1; p.GraphColors ??= new();
            p.NetworkAddresses ??= new();p.VisibleNetworkAddresses ??= new();
            p.HiddenCategories ??= new(); p.HiddenDevices ??= new(); p.Pins ??= new(); p.PinTargets ??= new(); p.CpuTemperatureSources ??= new(); p.HiddenMetrics ??= new();
            if(p.PinVisibilityVersion<1){if(p.Pins.Count>0){p.HiddenCategories.RemoveAll(id=>id=="Custom");p.HiddenDevices.RemoveAll(id=>id=="custom");}p.PinVisibilityVersion=1;}
            p.Order ??= new() { "CPU", "RAM", "GPU", "Drives", "Network", "Mainboard", "Controller", "Custom" };
            return p;
        }
        catch { return new(); }
    }
    public void Save()
    {
        var temporary = Path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, Path, true);
    }
    public void SetPinned(string sensorId,bool pinned)
    {
        Pins.RemoveAll(id=>id==sensorId);
        if(pinned) { Pins.Add(sensorId); HiddenCategories.RemoveAll(id=>id=="Custom"); HiddenDevices.RemoveAll(id=>id=="custom"); }
    }
    public static bool IsAddressMetric(string metricId)=>metricId.EndsWith("/internal-ip",StringComparison.Ordinal)||metricId.EndsWith("/external-ip",StringComparison.Ordinal);
    public bool IsMetricVisible(string deviceId,string metricId)=>IsAddressMetric(metricId)?VisibleNetworkAddresses.Contains(deviceId+"|"+metricId):!HiddenMetrics.Contains(deviceId+"|"+metricId);
    public void SetMetricVisible(string deviceId,string metricId,bool visible)
    {
        if(IsAddressMetric(metricId)){string addressKey=deviceId+"|"+metricId;VisibleNetworkAddresses.RemoveAll(item=>item==addressKey);if(visible)VisibleNetworkAddresses.Add(addressKey);return;}
        string key=deviceId+"|"+metricId;HiddenMetrics.RemoveAll(item=>item==key);if(!visible)HiddenMetrics.Add(key);
    }
}

public record NetworkAddressState(string? InternalIP=null,string? ExternalIP=null,DateTimeOffset? InternalUpdated=null,DateTimeOffset? ExternalUpdated=null);

public record Metric(string Id, string Label, double? Value, string Unit, double Maximum = 100,string? Source=null,string? TextValue=null)
{
    public static string FormatNumber(double value)=>Math.Abs(value)>=1?Math.Truncate(value).ToString("0"):value.ToString("0.##");
    public string Display(bool fahrenheit)
    {
        if(TextValue!=null)return TextValue;
        if (!Value.HasValue || !double.IsFinite(Value.Value)) return "—";
        double v = Value.Value;
        string u = Unit;
        if (u == "°C" && fahrenheit) { v = v * 1.8 + 32; u = "°F"; }
        if(u=="MB/s")return Math.Floor(Math.Max(0,v)).ToString("0")+" MB/s";
        if (u == "B/s")
        {
            string[] units = { "B/s", "KB/s", "MB/s", "GB/s" }; int i = 0;
            while (v >= 1024 && i < 3) { v /= 1024; i++; }
            return FormatNumber(v)+" "+units[i];
        }
        if (u == "MHz" && v >= 1000) return FormatNumber(v / 1000)+" GHz";
        return FormatNumber(v) + " " + u;
    }
}
public record Device(string Id, string Category, string Name, List<Metric> Metrics, bool DefaultVisible = true);
public record SensorRow(string Id, string Device, string Name, string Kind, double? Value, string Unit,string HardwareKind="");
public record Snapshot(List<Device> Devices, List<SensorRow> Sensors, List<string> Warnings, DateTime Time);

