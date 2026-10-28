# Sidebar by NemeSIS v2.15.2

A portable Windows hardware sidebar for CPU, memory, GPU, drives, network and custom sensors.

## New in v2.15.2

- **Nano:** a new ultra-compact design with aligned reading rows, highlighted hardware names, small gaps and no duplicated hero reading or reserved chart area. Select **Settings → Appearance & text → Card layout → Nano**. All previous layouts remain available: **48 layouts and 14 color themes** in total.
- **Whole-number readings:** values with an absolute magnitude of at least 1 are truncated toward zero after unit conversion. Examples: **31.7% → 31%**, **13.9 GB → 13 GB**, **3.87 GHz → 3 GHz**. Values below 1 keep up to two decimal places, such as **0.73 V**. Existing whole-MB/s network formatting is preserved. The rule applies to readings in all designs, including pinned sensors; raw sensor data and history are unchanged. IP addresses and hardware model names retain their punctuation.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.15.2.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.2/Sidebar-by-NemeSIS-v2.15.2.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. No application installer or separate .NET installation is required.

1. Exit the old sidebar through its notification-area icon.
2. Place the new EXE in a writable folder. Keep **aurora-settings.json** beside it to preserve preferences. Different folders have separate settings.
3. Start the new copy. Verify **2.15.2.0** and the launched EXE path under **System information**. An existing startup task pointing to another copy may require one Windows approval to move it to this EXE; startup on/off and delay are preserved. A fresh PC does not get a startup task automatically.

Some CPU/motherboard sensors require PawnIO and administrator access; **Settings → Setup** offers sensor prerequisites. Availability depends on the hardware, firmware, drivers and access rights. Software checks do not prove compatibility with every PC.

## Preview

Demo hardware and simulated readings; click a thumbnail to enlarge.

| Nano · one column | Nano · two columns |
|---|---|
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.2/Nano-Compact.png" width="240" alt="Nano compact layout with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.2/Nano-Compact.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.2/Nano-2Columns.png" width="400" alt="Nano layout in two columns with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.2/Nano-2Columns.png) |

The [six live-history designs from v2.15.0](https://github.com/nemesis122/NewSidebar/releases/tag/v2.15.0) remain available.

## Existing controls

- 1–4 columns, multi-monitor placement, fit-to-view and hover scrollbar.
- Independent font sizes, hardware name overrides, selected devices/readings and custom sensors attached to their matching widgets.
- Text/graph colors, chart size and thickness, and widget-background opacity that leaves text visible.
- Optional internal/external IP addresses update only when requested and retain the last saved result. External lookup uses api64.ipify.org via the system default route.
- Settings in German, English, French, Italian, Spanish and Portuguese; sidebar labels stay English. Notification-area icon only.

## Validation and source

**110 data checks and 55,271 native UI checks passed**, including **2,688 layout/color/width render combinations**. The portable EXE independently reports **2.15.2.0**. Nano is at least 20% shorter than Signal Strip for the CPU and network test fixtures at a 300-pixel width. LibreHardwareMonitor 0.9.6 and WPF UI 4.3.0 are included; see the dependency notices in the source.

The matching source archive includes tests, dependency notices and **Build-Portable.ps1**. Build on Windows using the .NET 8 SDK:

```powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
```

**SHA256SUMS.txt** lists the uploaded asset checksums.
