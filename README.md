# Sidebar by NemeSIS v2.14.3

A portable Windows hardware sidebar for CPU, memory, GPU, drives, network and custom sensors.

## What changed

- **Cards use their available height.** In 2–4 columns, shorter cards fill their row and readings or tiles use the spare space. Height distribution works with both side-by-side readings and the **Text below the graph** option. Selected text and graphic sizes are preserved.
- **Fluent settings and new layouts.** WPF UI 4.3.0 is integrated into the existing settings window. New choices since v2.12.2 include Fluent UI, Glass, Bento, Command Center, Metro Mosaic, Sensor Map, Blueprint and more.
- **Widget Board Black** adds black rounded cards with category-colored, filled **60-second history charts**. Samples come from live readings and stay in memory. Missing readings leave gaps; layout changes do not invent or duplicate samples. The three-dot button opens that device's settings.
- **41 normal layouts and 14 color themes.** Duplicate “large readings” variants were removed. Existing selections migrate to their matching normal design; text-size and graphic-size sliders remain available.
- **Cleaner Glass layout.** Removed the small decorative strokes above the circular instruments.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.14.3.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Sidebar-by-NemeSIS-v2.14.3.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. No application installer or separate .NET installation is required.

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

**102 data/startup/history checks and 47,247 native UI checks passed**, including **2,296 layout/color/width render combinations**. Additional tests check all 41 designs with 2–4 columns, both graph positions, equal card boundaries, consumed spare height and unchanged fonts. All 41 designs were also rendered with eight demo devices. The packaged EXE reports **2.14.3.0**.

Sensors use LibreHardwareMonitor 0.9.6. Readings depend on the hardware model, motherboard, firmware, drivers and permissions; these software tests do not prove compatibility with every PC. Test hardware used during development includes Ryzen 9 5900X and GeForce RTX 3080.

## Preview

Click a thumbnail to view it full size. All images below use **demo hardware and simulated readings**.

| Glass | Command Center | Widget Board Black |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Glass-2Columns.png" width="235" alt="Glass with evenly distributed readings">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Glass-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/CommandCenter-2Columns.png" width="235" alt="Full-height Command Center cards">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/CommandCenter-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/WidgetBoardBlack-2Columns.png" width="235" alt="Black widgets with filled history charts">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/WidgetBoardBlack-2Columns.png)|

| Original | Twin Rings | Blueprint |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Original-2Columns.png" width="235" alt="Original layout">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Original-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/TwinRings-2Columns.png" width="235" alt="Twin Rings layout">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/TwinRings-2Columns.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Blueprint-2Columns.png" width="235" alt="Blueprint layout">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Blueprint-2Columns.png)|

### Settings

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Settings.png" width="600" alt="Appearance, fonts and chart controls">](https://github.com/nemesis122/NewSidebar/releases/download/v2.14.3/Settings.png)

## Source and build

The repository and **Sidebar-by-NemeSIS-v2.14.3-source.zip** contain the matching source, tests, build script and dependency notices. Build on Windows with the .NET 8 SDK:

```powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
```

Main dependencies: LibreHardwareMonitor 0.9.6 and WPF UI 4.3.0. See **THIRD-PARTY-NOTICES.txt** and **Licenses/**. **SHA256SUMS.txt** contains the uploaded asset checksums.
