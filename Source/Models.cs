using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Aurora;

public enum SidebarTextRole { None,WidgetTitles,HardwareNames,Labels,Readings,InstrumentNumbers,InstrumentUnits }

public sealed class Preferences
{
    public bool CombineSensorCards { get; set; }
    public string SensorSource {get;set;}=SensorSources.Libre;
    public List<string> SeededSourceDevices {get;set;}=new();
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
    public bool ShowWidgetTitles {get;set;}=true;
    public bool ShowHardwareIcons {get;set;}=true;
    public double HardwareIconScale {get;set;}=1;
    public bool ShowHardwareNames {get;set;}=true;
    public bool ShowLabels {get;set;}=true;
    public bool ShowReadings {get;set;}=true;
    public bool ShowInstrumentNumbers {get;set;}=true;
    public bool ShowInstrumentUnits {get;set;}=true;
    public bool IsTextVisible(SidebarTextRole role)=>role switch{SidebarTextRole.WidgetTitles=>ShowWidgetTitles,SidebarTextRole.HardwareNames=>ShowHardwareNames,SidebarTextRole.Labels=>ShowLabels,SidebarTextRole.Readings=>ShowReadings,SidebarTextRole.InstrumentNumbers=>ShowInstrumentNumbers,SidebarTextRole.InstrumentUnits=>ShowInstrumentUnits,_=>true};
    public double TitleTextScale { get; set; } = 1;
    public double HardwareTextScale { get; set; } = 1;
    public double LabelTextScale { get; set; } = 1;
    public double ValueTextScale { get; set; } = 1;
    public double InstrumentTextScale { get; set; } = 1;
    public double UnitTextScale { get; set; } = 1;
    public void ResetTypography()
    {
        HardwareIconScale=FontScale=TitleTextScale=HardwareTextScale=LabelTextScale=ValueTextScale=InstrumentTextScale=UnitTextScale=1;
    }
    public void ShowAllTextAndIcons()
    {
        ShowHardwareIcons=ShowWidgetTitles=ShowHardwareNames=ShowLabels=ShowReadings=ShowInstrumentNumbers=ShowInstrumentUnits=true;
    }
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
            p.SensorSource=SensorSources.Normalize(p.SensorSource);
            p.SeededSourceDevices ??= new();
            p.TextFontFamily = p.TextFontFamily?.Trim() ?? "";
            using var document=JsonDocument.Parse(json);
            if(!document.RootElement.TryGetProperty("Layout",out _))p.Layout=p.Theme switch {"Original"=>"Original","Compact" or "Terminal"=>"Compact","Minimal"=>"Text",_=>"Modern"};
            if(p.Layout?.EndsWith(".Large",StringComparison.Ordinal)==true)p.Layout=p.Layout[..^6];
            p.ColumnWidth = double.IsFinite(p.ColumnWidth)?Math.Clamp(p.ColumnWidth, 50, 440):290;
            p.Columns = Math.Clamp(p.Columns, 1, 4);
            p.Opacity = Math.Clamp(p.Opacity, 0, 1);
            p.Interval = Math.Clamp(p.Interval, 500, 10000);
            p.FontScale = Math.Clamp(p.FontScale, .65, 1.4);
            p.HardwareIconScale=TextFactor(p.HardwareIconScale);p.TitleTextScale=TextFactor(p.TitleTextScale);p.HardwareTextScale=TextFactor(p.HardwareTextScale);p.LabelTextScale=TextFactor(p.LabelTextScale);p.ValueTextScale=TextFactor(p.ValueTextScale);p.InstrumentTextScale=TextFactor(p.InstrumentTextScale);p.UnitTextScale=TextFactor(p.UnitTextScale);
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
    public bool IsCpuClock { get; init; }
    public static string FormatNumber(double value)=>Math.Abs(value)>=1?Math.Truncate(value).ToString("0"):value.ToString("0.##");
    // Units whose meaning lives in the decimals get a fixed precision; whole-number units keep FormatNumber.
    private static string Fixed(double value,string format)=>value.ToString(format,System.Globalization.CultureInfo.InvariantCulture);
    public static string FormatGigabytes(double value)=>Math.Abs(value)>=100?Fixed(Math.Truncate(value),"0"):Fixed(value,"0.0");
    public string Display(bool fahrenheit)
    {
        if(TextValue!=null)return TextValue;
        if (!Value.HasValue || !double.IsFinite(Value.Value)) return "—";
        double v = Value.Value;
        string u = Unit;
        if(IsCpuClock && u=="MHz")return (v/1000).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)+" GHz";
        if (u == "°C" && fahrenheit) { v = v * 1.8 + 32; u = "°F"; }
        if(u=="MB/s")return Math.Floor(Math.Max(0,v)).ToString("0")+" MB/s";
        if(u=="V")return Fixed(v,"0.000")+" V";
        if(u=="GB")return FormatGigabytes(v)+" GB";
        if(u is "Gbit/s" or "Mbit/s")return Fixed(v,"0.#")+" "+u;
        if (u == "B/s")
        {
            string[] units = { "B/s", "KB/s", "MB/s", "GB/s" }; int i = 0;
            while (v >= 1024 && i < 3) { v /= 1024; i++; }
            return (i>0&&Math.Abs(v)<10?Fixed(v,"0.#"):FormatNumber(v))+" "+units[i];
        }
        if (u == "MHz" && v >= 1000) return Fixed(v / 1000,"0.00")+" GHz";
        return FormatNumber(v) + " " + u;
    }
}
public record Device(string Id, string Category, string Name, List<Metric> Metrics, bool DefaultVisible = true);
public record SensorRow(string Id, string Device, string Name, string Kind, double? Value, string Unit,string HardwareKind="",string Provider="");
public record DevicePreferenceSeed(string DeviceId,string ReferenceDeviceId,Dictionary<string,string> MetricIds);
public record Snapshot(List<Device> Devices, List<SensorRow> Sensors, List<string> Warnings, DateTime Time,string SensorSource="",List<DevicePreferenceSeed>? PreferenceSeeds=null);

