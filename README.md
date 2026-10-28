# Sidebar by NemeSIS v2.15.0

A portable Windows hardware sidebar for CPU, memory, GPU, drives, network and custom sensors.

## Six new history layouts

- **Panoramic Flow:** a large primary reading, a wide history chart and readings underneath.
- **Split Wave:** readings on the left and a tall history chart on the right.
- **Signal Strip:** a compact card with a narrow history strip along the bottom.
- **Dual Trace:** two separately labeled histories, each with its own scale and units. Examples include CPU/GPU load and temperature, network DL/UL, memory load/used, and drive used/write readings when available.
- **Floating Chart:** a subtle background curve behind the readings.
- **Telemetry Grid:** primary reading and history above organized reading tiles.

There are now **47 normal layouts and 14 color themes**. All previous designs remain available; no duplicate large-reading variants were reintroduced.

The new layouts reuse the existing memory-only 60-second history. They do not add hardware polling or invent warm-up data. Missing readings leave gaps; if a second metric is unavailable, Dual Trace shows one real series. Text roles, colors, chart size, line thickness and background-only opacity remain adjustable.

Charts adapt to the actual available width, including fit-to-view. Full-width charts remain inside the card when enlarged and use additional height. With 2–4 columns, **Text below the graph** places the chart above the readings; switch it off for the default compositions shown below.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.15.0.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/Sidebar-by-NemeSIS-v2.15.0.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. No application installer or separate .NET installation is required.

1. Exit the previous sidebar through its notification-area icon.
2. Put the new EXE in a **writable folder**. Keep your existing **aurora-settings.json** beside it to retain your preferences. Different folders have separate settings.
3. Start the new EXE. If an existing startup task points to another executable, Windows may ask once to move that task to the selected EXE and its folder. Startup on/off and logon delay are preserved. Declining approval still opens the selected copy with standard-user sensor access.

On a fresh PC, use **Settings → Setup** for sensor prerequisites and **Position & size** for optional startup. A new startup task is not created automatically on a fresh PC. Some CPU and motherboard sensors require PawnIO and administrator access.

## Features and controls

- Choose devices and readings independently; pin custom sensors to their matching widgets. Rename detected hardware in **Devices & readings → Hardware name**.
- Choose an installed font and set independent sizes for titles, hardware names, labels, readings, instrument numbers and units. **Graphic size (%)** resizes the complete instrument or history chart.
- Customize text and graph colors. Widget-background opacity leaves readings and graphs visible.
- 1–4 columns, widths of 50–440 logical pixels, multiple monitors, fit-to-view and a hover scrollbar. In 2–4 columns, **Text below the graph** moves instruments above the readings; turn it off for readings beside the instrument.
- Network **DL / UL** use whole MB/s; **Link** uses Mbit/s or Gbit/s. Optional internal and external IPs refresh only when the button is pressed, and are saved until the next request. External lookup uses api64.ipify.org through the system default internet route.
- Settings in German, English, French, Italian, Spanish and Portuguese. Sidebar labels stay English. Notification-area icon only.

## Validation and hardware

**102 data/startup/history checks and 54,319 native UI checks passed**, including **2,632 layout/color/width render combinations**. Additional tests check all 47 designs with 2–4 columns, both graph positions, equal card boundaries, consumed spare height and unchanged fonts. All 41 designs were also rendered with eight demo devices. The packaged EXE reports **2.15.0.0**.

Sensors use LibreHardwareMonitor 0.9.6. Readings depend on the hardware model, motherboard, firmware, drivers and permissions; these software tests do not prove compatibility with every PC. Test hardware used during development includes Ryzen 9 5900X and GeForce RTX 3080.

## Preview

All images use **demo hardware and simulated history**. Click a thumbnail for the full image.

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/History-Designs.png" width="720" alt="All six new history layouts">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/History-Designs.png)

| Panoramic Flow | Split Wave | Signal Strip |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/PanoramicFlow-2Columns.png" width="235" alt="Panoramic Flow in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/PanoramicFlow-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/SplitWave-2Columns.png" width="235" alt="Split Wave in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/SplitWave-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/SignalStrip-2Columns.png" width="235" alt="Signal Strip in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/SignalStrip-2Columns.png)|

| Dual Trace | Floating Chart | Telemetry Grid |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/DualTrace-2Columns.png" width="235" alt="Dual Trace with separately labeled histories">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/DualTrace-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/FloatingChart-2Columns.png" width="235" alt="Floating Chart in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/FloatingChart-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/TelemetryGrid-2Columns.png" width="235" alt="Telemetry Grid in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/TelemetryGrid-2Columns.png)|

### Settings

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/Settings.png" width="600" alt="Appearance settings with the new history layouts">](https://github.com/nemesis122/NewSidebar/releases/download/v2.15.0/Settings.png)

## Source and build

The repository and **Sidebar-by-NemeSIS-v2.15.0-source.zip** contain the matching source, tests, build script and dependency notices. Build on Windows with the .NET 8 SDK:

```powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
```

Main dependencies: LibreHardwareMonitor 0.9.6 and WPF UI 4.3.0. See **THIRD-PARTY-NOTICES.txt** and **Licenses/**. **SHA256SUMS.txt** contains the uploaded asset checksums.
