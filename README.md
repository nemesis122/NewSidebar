# Sidebar by NemeSIS v2.12.2

A portable Windows hardware sidebar for CPU, RAM, GPU, drives, network and custom sensors.

## Startup and portable-location fixes

- Opening a portable copy no longer activates an unrelated copy in another folder.
- If an existing startup task points to another EXE, the app requests Windows approval to update its executable path and working directory to the copy you selected. Your startup on/off choice and logon delay are preserved. Windows may require administrator approval once when the location changes.
- If approval is declined, the selected copy still opens with the sensor access available to a standard user. An unsuccessful repair is reported instead of silently exiting.
- System information separately identifies the **Running EXE**, the **Startup EXE** and the actual running version.
- Startup exceptions produce a visible message and a diagnostic log, with a temporary-folder fallback if the EXE folder cannot be written.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.12.2.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Sidebar-by-NemeSIS-v2.12.2.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. The app is unsigned; Windows may display a reputation warning.

1. Exit older copies through their notification-area icon before upgrading.
2. Put the EXE in your chosen writable folder. To retain preferences, keep your existing **aurora-settings.json** beside it. Different folders have separate preferences.
3. Start this EXE. If the existing startup task points elsewhere, approve the Windows administrator request to update that task. On subsequent starts from the registered location, no further relocation request is needed.

On a fresh PC, configure sensor access and optional Windows startup through **Settings → Setup** and **Position & size**. No startup task is created automatically on a fresh PC. Some CPU/motherboard sensors need PawnIO and administrator access.

## Features and controls

- 48 layout variants and 14 independent color themes, including Original, Twin Rings, Hex-Core, Dot Matrix, Rackmount, compact lists and large-reading variants.
- CPU, memory, GPU, individual drives and network widgets; select devices and visible readings independently. Custom sensors can join their matching widgets.
- **Devices & readings → Hardware name:** select a device, enter a name, then Apply or Enter. Restore original name resets it without changing sensor identity.
- **Appearance & text → Font family (all text):** choose an installed Windows font. Text sizes independently controls widget titles, hardware names, labels, readings, instrument numbers and units (50–200%).
- **Graphic size (%)** resizes the whole instrument. Widget background opacity leaves text and readings opaque.
- 1–4 columns, widths of 50–440 logical pixels, multiple monitors, DPI scaling, fit-to-view and a hover scrollbar. With 2–4 columns, **Text below the graph** places readings underneath the instrument.
- Network **DL / UL** use whole MB/s; **Link** uses Mbit/s or Gbit/s. Optional internal/external IPs are fetched only by the refresh button and saved until the next request. No startup or scheduled IP lookup runs. External lookup uses api64.ipify.org and the system default internet route.
- Settings in German, English, French, Italian, Spanish and Portuguese; sidebar labels stay English. Notification-area icon only, with Settings, Show and Exit.

## Validation and hardware

92 data/startup checks and 25,924 native UI checks passed, with 2,688 layout/color/width render combinations. Checks include child-process activation isolation, stale startup targets, all settings languages, and updating an unregistered Windows Task Scheduler definition while preserving on/off state and delay. The portable EXE reports version 2.12.2.0.

The complete Windows approval/logon workflow on the second PC still needs real-machine validation. These software tests do not prove compatibility with every hardware model. LibreHardwareMonitor 0.9.6 supports AMD/Intel CPUs and NVIDIA/AMD/Intel GPUs; actual readings depend on motherboard, firmware, drivers and permissions. Physically checked hardware includes Ryzen 9 5900X and RTX 3080.

## Preview

Click a thumbnail for the full image. Previews use demo hardware and simulated readings. Theme/layout examples remain representative; older example screenshots may show earlier settings labels.

| OLED Black | Midnight | Nord |
|---|---|---|
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-OLED.png" width="145" alt="OLED Black">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-OLED.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Midnight.png" width="145" alt="Midnight">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Midnight.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Nord.png" width="145" alt="Nord">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Nord.png)|
| Dracula | Terminal | Light |
|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Dracula.png" width="145" alt="Dracula">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Dracula.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Terminal.png" width="145" alt="Terminal">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Terminal.png)|[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Light.png" width="145" alt="Light">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Theme-Light.png)|

### Two columns and nerd designs

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/TwinRings-2Columns.png" width="300" alt="Twin Rings in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/TwinRings-2Columns.png)

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Nerd-Designs.png" width="550" alt="Hex-Core, Bitfield and Rackmount">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Nerd-Designs.png)

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/MultiColumn-Preview.png" width="300" alt="Instruments above readings in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/MultiColumn-Preview.png)

### Current settings and hardware names

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Hardware-Names.png" width="320" alt="Custom hardware name and selectable readings">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Hardware-Names.png) [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Font-Selection.png" width="320" alt="Font family and appearance controls">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Font-Selection.png)

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Text-Size-Settings.png" width="300" alt="Independent text sizes">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Text-Size-Settings.png)

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Graphic-Size-Comparison.png" width="430" alt="Whole instrument size comparison">](https://github.com/nemesis122/NewSidebar/releases/download/v2.12.2/Graphic-Size-Comparison.png)

## Source and build

The repository and **Sidebar-by-NemeSIS-v2.12.2-source.zip** contain the matching source, tests, build script and third-party notices. Build on Windows with the .NET 8 SDK using **Build-Portable.ps1**.

```powershell
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
```

**SHA256SUMS.txt** contains checksums for the uploaded assets. See **THIRD-PARTY-NOTICES.txt** and **Licenses/** for dependencies.
