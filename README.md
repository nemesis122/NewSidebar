# Sidebar by NemeSIS v2.15.6

A portable Windows hardware sidebar for CPU, memory, GPU, drives, network and custom sensors.

## New since v2.15.2

- **Performance HUD:** angular black panels, green–amber–red segmented instruments, available mini-instruments and aligned reading bars. CPU/GPU show load with temperature when available. Extra instruments are created only for detected metrics.
- **Cyber Pulse:** neon borders, a large colored dial, an inline hardware header and prominent reading blocks. Additional selected readings remain visible as compact rows. RAM uses its available used-memory reading in the dial.
- **Titan HUD:** a dark metal-style frame, a segmented dial and aligned readings.
- **No lower history graphs in Cyber Pulse or Titan HUD:** the bars and reserved history space have been removed; the live sensor instruments remain.
- **Text below the graph now supports 1–4 columns**, including a one-column sidebar. Use **Settings → Appearance & text → Text below the graph**. Layouts already placing readings underneath a graphic keep that structure; reading-only layouts remain reading-only.

There are now **51 layouts and 14 color themes**. Existing Nano and history layouts remain available. The four recent Cyan Circuit / Dark Wireframe / Tri-Color Ink / Clean Dark concepts are preview experiments and are **not part of this release**.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.15.6.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/Sidebar-by-NemeSIS-v2.15.6.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. No application installer or separate .NET installation is needed.

1. Exit the previous sidebar through its notification-area icon.
2. Place the new EXE in a writable folder. Keep **aurora-settings.json** beside it to preserve preferences; separate folders have separate settings.
3. Start the new copy. **System information** should report **2.15.6.0** and the launched EXE path. An existing startup task pointing to another copy may require one Windows approval to move it to this executable; startup on/off and delay are preserved. A fresh PC does not receive a startup task automatically.

Some CPU/motherboard sensors require PawnIO and administrator access; see **Settings → Setup**. Readings depend on hardware, firmware, drivers and permissions. Unavailable sensors are not fabricated.

## Preserved controls

- Independent text sizes, installed font selection, hardware names and per-device sensor selection/pinning.
- Text and graph colors, complete graphic size, line thickness and widget-background opacity that leaves readings visible.
- Multi-monitor placement, 1–4 columns, fit-to-view and hover scrollbar.
- Whole readings truncate toward zero after unit conversion; sub-unit values retain up to two decimals, for example **31.7% → 31%** and **0.73 V → 0.73 V**. Existing whole-MB/s formatting, IP addresses, raw measurements and history remain unchanged.
- Internal/external IP values update only when requested and retain the saved result. External lookup uses api64.ipify.org through the default internet route.
- Settings in German, English, French, Italian, Spanish and Portuguese; sidebar labels stay English. Notification-area icon only.

## Preview

All previews use **demo hardware and simulated readings**. Click a thumbnail for full size.

| Performance HUD | Cyber Pulse | Titan HUD |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/PerformanceHUD-Sidebar.png" width="240" alt="Performance HUD sidebar with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/PerformanceHUD-Sidebar.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/CyberPulse-Sidebar.png" width="240" alt="Cyber Pulse sidebar without lower history graphs">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/CyberPulse-Sidebar.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/TitanHUD-Sidebar.png" width="240" alt="Titan HUD sidebar without lower history graphs">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/TitanHUD-Sidebar.png)|

### Two columns

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/PerformanceHUD-2Columns.png" width="500" alt="Performance HUD in two columns with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/PerformanceHUD-2Columns.png)

### Text below the instrument with one column

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/Single-Column-Below.png" width="680" alt="One-column comparison with readings beside or below the instrument">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/Single-Column-Below.png)

### Settings

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/Settings.png" width="680" alt="English appearance settings with text below the graph enabled">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.6/Settings.png)

## Validation and source

**58,936 native UI checks passed**, including **2,856 layout/color/width render combinations**. Existing **110 data checks** remain passing. The packaged EXE independently reports **2.15.6.0**. Software checks do not establish compatibility with every physical PC.

The matching source archive includes the tests, dependency notices and **Build-Portable.ps1**. Build on Windows with the .NET 8 SDK:

```powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
```

Dependencies include LibreHardwareMonitor 0.9.6 and WPF UI 4.3.0; see **THIRD-PARTY-NOTICES.txt** and **Licenses/**. **SHA256SUMS.txt** lists the uploaded asset checksums.
