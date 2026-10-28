using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;

namespace Aurora;

// All calls into LHM live on one worker. The UI only sees immutable snapshots.
public sealed class SensorService
{
    private readonly CancellationTokenSource stop = new();
    private readonly Dictionary<string, (long Rx, long Tx, long Stamp)> networks = new();
    private Computer? computer;
    private string? initWarning;
    private ulong previousIdle, previousKernel, previousUser;
    public int Interval = 1000;
    public IReadOnlyDictionary<string,string> CpuTemperatureSources = new Dictionary<string,string>();
    private int readCount;
    public event Action<Snapshot>? Updated;
    public Task? Worker { get; private set; }
    public void Start() => Worker = Task.Run(Run);
    public void Stop() => stop.Cancel();
    private async Task Run()
    {
        initWarning = SensorAccess.Describe();
        try
        {
            computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true, IsMemoryEnabled = true,
                IsMotherboardEnabled = true, IsStorageEnabled = true, IsNetworkEnabled = false,
                IsControllerEnabled = true };
            computer.Open();
        }
        catch (Exception e) { initWarning += "\nSensorzugriff: " + e.Message; }
        try
        {
            while (!stop.IsCancellationRequested)
            {
                try { Updated?.Invoke(Read()); }
                catch (Exception e) { Updated?.Invoke(new(new(), new(), new() { e.Message }, DateTime.Now)); }
                await Task.Delay(Interval, stop.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        finally { try { computer?.Close(); } catch { } }
    }
    private static IEnumerable<IHardware> Flatten(IHardware hw)
    {
        yield return hw;
        foreach (var sub in hw.SubHardware) foreach (var item in Flatten(sub)) yield return item;
    }
    public Snapshot Read()
    {
        var devices = new List<Device>(); var rows = new List<SensorRow>(); var warnings = new List<string>();
        if (initWarning != null) warnings.Add(initWarning);
        var hardware = computer?.Hardware.SelectMany(Flatten).ToArray() ?? Array.Empty<IHardware>();
        foreach (var hw in hardware)
        {
            try { hw.Update(); }
            catch (Exception e) { warnings.Add(hw.Name + ": " + e.Message); continue; }
            var sensors = hw.Sensors;
            double? Reading(ISensor s)
            {
                // LHM can expose uninitialized privileged registers as 0. Do not present
                // these as real CPU temperatures, clocks, package power or voltages.
                if (s.Value == 0 && ((hw.HardwareType.ToString() == "Cpu" && s.SensorType is SensorType.Temperature or SensorType.Clock or SensorType.Power) || (s.SensorType is SensorType.Temperature or SensorType.Voltage && hw.HardwareType.ToString() is "EmbeddedController" or "SuperIO"))) return null;
                if (hw.HardwareType.ToString() == "Cpu" && s.SensorType == SensorType.Voltage && !sensors.Any(clock=>clock.SensorType==SensorType.Clock && clock.Value>0)) return null;
                return s.Value;
            }
            foreach (var s in sensors)
                rows.Add(new(s.Identifier.ToString(), hw.Name, s.Name, s.SensorType.ToString(), Reading(s), Unit(s.SensorType),hw.HardwareType.ToString()));
            string kind = hw.HardwareType.ToString();
            string category = kind switch { "Cpu" => "CPU", "Memory" => "RAM", "Storage" => "Drives",
                "Motherboard" or "SuperIO" => "Mainboard", _ when kind.StartsWith("Gpu") => "GPU", _ => "Controller" };
            // Windows memory and disk-space APIs provide stable totals, independent of vendor sensor naming.
            if (category == "RAM") continue;
            ISensor? Find(SensorType type, params string[] prefer)
            {
                var candidates = sensors.Where(s => s.SensorType == type).ToArray();
                foreach (var name in prefer)
                {
                    var match = candidates.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && Reading(s).HasValue);
                    if (match != null) return match;
                }
                foreach(var name in prefer)
                {
                    var match=candidates.FirstOrDefault(s=>s.Name.Equals(name,StringComparison.OrdinalIgnoreCase));
                    if(match!=null) return match;
                }
                return candidates.FirstOrDefault(s => Reading(s).HasValue) ?? candidates.FirstOrDefault();
            }
            var metrics = new List<Metric>();
            void Add(string label, SensorType type, double max, params string[] names)
            {
                var s = Find(type, names);
                if (s != null) metrics.Add(new(s.Identifier.ToString(), label, Reading(s), Unit(type), max));
            }
            void AddExact(string label, SensorType type, double max, params string[] names)
            {
                var s = sensors.FirstOrDefault(s => s.SensorType == type && names.Contains(s.Name,StringComparer.OrdinalIgnoreCase));
                if (s != null) metrics.Add(new(s.Identifier.ToString(), label, Reading(s), Unit(type), max));
            }
            if (category is "CPU" or "GPU")
            {
                Add("Auslastung", SensorType.Load, 100, "CPU Total", "GPU Core", "GPU D3D 3D");
                Add("Temperatur", SensorType.Temperature, 100, "Core (Tctl/Tdie)", "CPU Package", "Core (Tdie)", "GPU Core", "Core Average");
                if(category=="CPU")
                {
                    var average=sensors.FirstOrDefault(s=>s.SensorType==SensorType.Clock && s.Name=="Cores (Average)" && Reading(s).HasValue);
                    var cores=sensors.Where(s=>s.SensorType==SensorType.Clock && s.Name.StartsWith("Core #",StringComparison.OrdinalIgnoreCase) && !s.Name.Contains("Effective",StringComparison.OrdinalIgnoreCase)).Select(Reading).Where(v=>v.HasValue).ToArray();
                    double? clock=average!=null ? Reading(average) : cores.Length>0 ? cores.Average(v=>v!.Value) : null;
                    metrics.Add(new(hw.Identifier+"/clocks-average","Takt Ø",clock,"MHz",6000));
                }
                else AddExact("Takt",SensorType.Clock,6000,"GPU Core","GPU Render");
                Add("Leistung", SensorType.Power, 450, "Package", "CPU Package", "GPU Package", "GPU Power");
                Add("Spannung", SensorType.Voltage, 2, "Core (SVI2 TFN)", "CPU Core", "GPU Core");
                Add("Lüfter", SensorType.Fan, 3500);
                if (category == "GPU")
                {
                    AddExact("VRAM belegt", SensorType.SmallData, 24576, "GPU Memory Used");
                    AddExact("VRAM Last", SensorType.Load, 100, "GPU Memory");
                }
            }
            else if (category == "Drives")
            {
                Add("Temperatur", SensorType.Temperature, 80, "Temperature", "Composite");
                AddExact("Belegt", SensorType.Load, 100, "Used Space");
                AddExact("Lesen", SensorType.Throughput, 1000000000, "Read Rate");
                AddExact("Schreiben", SensorType.Throughput, 1000000000, "Write Rate");
                AddExact("Verbleibende Lebensdauer", SensorType.Level, 100, "Remaining Life");
            }
            else
            {
                foreach (var s in sensors.Where(s => s.SensorType is SensorType.Temperature or SensorType.Fan or SensorType.Voltage).Take(6))
                    metrics.Add(new(s.Identifier.ToString(), s.Name, Reading(s), Unit(s.SensorType), s.SensorType == SensorType.Fan ? 3500 : 100));
            }
            if (metrics.Count > 0) devices.Add(new(hw.Identifier.ToString(), category, hw.Name, metrics));
        }
        var cpus=devices.Where(d=>d.Category=="CPU").ToArray();
        var overrides=CpuTemperatureSources;
        for(int i=0;i<cpus.Length;i++)
        {
            overrides.TryGetValue(cpus[i].Id,out string? preferred);
            var chosen=CpuTemperatureSelector.Select(cpus[i].Id,rows,preferred,i+1,cpus.Length);
            var metrics=cpus[i].Metrics;
            int index=metrics.FindIndex(m=>m.Label=="Temperatur");
            var value=chosen==null ? new Metric(cpus[i].Id+"/display-temperature","Temperatur",null,"°C",100,"Keine passende CPU-Temperatur verfügbar")
                : new Metric(cpus[i].Id+"/display-temperature",CpuTemperatureSelector.Label(chosen),chosen.Value,"°C",100,chosen.Device+" · "+chosen.Name+" · "+chosen.Id);
            if(index>=0)metrics[index]=value;else metrics.Insert(Math.Min(1,metrics.Count),value);
        }
        if(++readCount>=3 && File.Exists(Path.Combine(AppContext.BaseDirectory,"cpu-temperature-check.request")))
        {
            try
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"cpu-temperature-check.json"),System.Text.Json.JsonSerializer.Serialize(new {Time=DateTime.Now,Selected=cpus.Select(d=>new{d.Name,Temperature=d.Metrics.FirstOrDefault(m=>m.Id.EndsWith("/display-temperature"))}),Sensors=rows.Where(s=>s.Kind=="Temperature")},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
                File.Delete(Path.Combine(AppContext.BaseDirectory,"cpu-temperature-check.request"));
            }catch(Exception error){warnings.Add("CPU-Diagnose: "+error.Message);}
        }
        if (!devices.Any(d => d.Category == "CPU"))
        {
            var name = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString", "Prozessor")?.ToString()?.Trim() ?? "Prozessor";
            devices.Insert(0, new("windows/cpu", "CPU", name, new() { new("windows/cpu/load", "Auslastung", CpuLoad(), "%"), new("windows/cpu/temp", "Temperatur", null, "°C") }));
        }
        var mem = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (GlobalMemoryStatusEx(ref mem))
        {
            double total = mem.TotalPhysical / 1073741824d, free = mem.AvailablePhysical / 1073741824d;
            devices.Add(new("windows/ram", "RAM", $"{Metric.FormatNumber(total)} GB · Physischer Arbeitsspeicher", new() {
                new("windows/ram/load", "Auslastung", Math.Round((total-free)/total*100,MidpointRounding.AwayFromZero), "%"),
                new("windows/ram/used", "Belegt", total-free, "GB", total),
                new("windows/ram/free", "Verfügbar", free, "GB", total),
                new("windows/ram/total", "Gesamt", total, "GB", total) }));
        }
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
        {
            try
            {
                if (!drive.IsReady) continue;
                double total = drive.TotalSize / 1073741824d, free = drive.TotalFreeSpace / 1073741824d;
                string id = "volume/" + drive.Name;
                devices.Add(new(id, "Drives", drive.Name + "  " + drive.VolumeLabel, new() {
                    new(id+"load", "Auslastung", (total-free)/total*100, "%"),
                    new(id+"used", "Belegt", total-free, "GB", total),
                    new(id+"free", "Frei", free, "GB", total),
                    new(id+"total", "Kapazität", total, "GB", total) }, drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0,2), StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception e) { warnings.Add(drive.Name + ": " + e.Message); }
        }
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
        {
            try
            {
                var st = nic.GetIPStatistics(); long now = Stopwatch.GetTimestamp();
                double? rx = null, tx = null;
                if (networks.TryGetValue(nic.Id, out var old))
                {
                    double elapsed = (now-old.Stamp)/(double)Stopwatch.Frequency;
                    if (elapsed > 0 && st.BytesReceived >= old.Rx && st.BytesSent >= old.Tx)
                    { rx = (st.BytesReceived-old.Rx)/elapsed; tx = (st.BytesSent-old.Tx)/elapsed; }
                }
                networks[nic.Id] = (st.BytesReceived, st.BytesSent, now);
                string id = "network/" + nic.Id;
                bool physical = !System.Text.RegularExpressions.Regex.IsMatch(nic.Description, "virtual|vmware|hyper-v|tap-|tunnel|loopback", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                devices.Add(new(id, "Network", nic.Description, new() {
                    new(id+"rx", "Download", rx/1000000d, "MB/s", Math.Max(1, nic.Speed/8000000d)),
                    new(id+"tx", "Upload", tx/1000000d, "MB/s", Math.Max(1, nic.Speed/8000000d)),
                    new(id+"link", "Adapter-Link", nic.Speed>0?nic.Speed/(nic.Speed>=1000000000?1000000000d:1000000d):(double?)null, nic.Speed>=1000000000?"Gbit/s":"Mbit/s", Math.Max(1,nic.Speed/(nic.Speed>=1000000000?1000000000d:1000000d))) }, physical));
            }
            catch (Exception e) { warnings.Add(nic.Name + ": " + e.Message); }
        }
        foreach (var d in devices)
            foreach (var m in d.Metrics.Where(m => !rows.Any(r => r.Id == m.Id))) rows.Add(new(m.Id, d.Name, m.Label, d.Category, m.Value, m.Unit));
        return new(devices, rows, warnings, DateTime.Now);
    }
    private double? CpuLoad()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user)) return null;
        ulong i = idle.Value, k = kernel.Value, u = user.Value;
        double? result = null;
        if (previousKernel != 0)
        {
            ulong total = k - previousKernel + u - previousUser;
            if (total > 0) result = Math.Clamp(100d*(total-(i-previousIdle))/total, 0, 100);
        }
        previousIdle = i; previousKernel = k; previousUser = u;
        return result;
    }
    public static string Unit(SensorType type) => type.ToString() switch {
        "Temperature" => "°C", "Load" or "Level" or "Control" or "Humidity" => "%", "Clock" => "MHz", "Voltage" => "V", "Power" => "W", "Fan" => "RPM", "Flow" => "L/h", "Data" => "GB", "SmallData" => "MB", "Throughput" => "B/s", "Current" => "A", "Frequency" => "Hz", "Energy" => "mWh", _ => "" };
    [StructLayout(LayoutKind.Sequential)] private struct FileTime { public uint Low, High; public ulong Value => ((ulong)High << 32) | Low; }
    [DllImport("kernel32.dll")] private static extern bool GetSystemTimes(out FileTime idle, out FileTime kernel, out FileTime user);
    [StructLayout(LayoutKind.Sequential)] private struct MemoryStatus { public uint Length, Load; public ulong TotalPhysical, AvailablePhysical, TotalPage, AvailablePage, TotalVirtual, AvailableVirtual, AvailableExtended; }
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}
