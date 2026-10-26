# Sidebar by NemeSIS

A native, portable Windows hardware sidebar for CPU, RAM, GPU, drives and network.

## Download

[Download the portable EXE](https://github.com/nemesis122/NewSidebar/releases/latest)

Windows 10/11 x64. Run the EXE from a writable folder. The .NET runtime is bundled;
settings are stored in `aurora-settings.json` beside the EXE.

## Preview

### Selected color themes

Click a thumbnail to open the full image. All examples show **Twin rings** with simulated readings.

| OLED Black | Midnight | Nord |
| :---: | :---: | :---: |
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-OLED.png" width="150" alt="OLED Black">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-OLED.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Midnight.png" width="150" alt="Midnight">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Midnight.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Nord.png" width="150" alt="Nord">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Nord.png) |
| **Dracula** | **Terminal** | **Light** |
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Dracula.png" width="150" alt="Dracula">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Dracula.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Terminal.png" width="150" alt="Terminal">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Terminal.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Light.png" width="150" alt="Light">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Theme-Light.png) |

### Two columns & settings

| Twin Rings · two columns | Settings menu |
| :---: | :---: |
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/TwinRings-2Columns.png" width="180" alt="Twin Rings with two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/TwinRings-2Columns.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Settings-Preview.png" width="280" alt="Settings menu">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Settings-Preview.png) |

### Nerd designs

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Nerd-Designs.png" width="420" alt="Hex-Core, Bitfield and Rackmount">](https://github.com/nemesis122/NewSidebar/releases/download/v2.8.0/Nerd-Designs.png)

## Features

- 48 layout variants, including large-reading options; 14 independent color themes.
- Hex-Core, Bitfield, Rackmount, gauges, LED meters, console and compact lists.
- Configurable text colors, per-category instrument accents and widget background opacity.
- One to four columns, multiple monitors, fit-to-view and scrolling with hover scrollbar.
- Custom sensors in their matching widgets; selectable CPU temperature source.
- Settings in German, English, French, Italian, Spanish and Portuguese.
- Sidebar labels always remain English, with short Temp / Clock / DL / UL / Link labels.
- Optional automatic startup after sensor access has been configured.

## Manual IP addresses and compact widgets

In Settings → Devices & readings, select your network adapter and enable Internal IP
and/or External IP. Press **Fetch / refresh IP addresses** to save new values.
There is **no automatic lookup**, on startup or on a timer. Saved addresses remain
until you press the button again. The external lookup uses api64.ipify.org and
the system default internet route, which may differ from the selected adapter.
Failed or timed-out lookups preserve the previous external address.

Widget width can be reduced to **200 logical pixels** and text size to **65%**.
Long IP addresses wrap instead of overlapping nearby values.

## Hardware and sensor access

Uses LibreHardwareMonitor 0.9.6 for AMD/Intel CPUs and NVIDIA/AMD/Intel GPUs.
Actual readings depend on the hardware, motherboard, driver and access rights.
Physically checked on Ryzen 9 5900X and RTX 3080; not every model has been tested.
Some sensors require PawnIO and one-time administrator setup via Settings.
The portable application itself does not require an application installer.

## Build and validation

Requires Windows and the .NET 8 SDK. Run `Build-Portable.ps1` from this repository.

```powershell
dotnet run --project Tests/Checks.csproj -c Release --configfile Source/NuGet.Config
dotnet run --project UITests/LayoutQA.csproj -c Release --configfile UITests/NuGet.Config
```

Version 2.9.0 passed 72 data checks, 16,968 native checks and 2,688 layout/color/width combinations,
including additional 1440p/4K layout checks at 100/150/200 percent DPI.
Software layout tests do not constitute physical validation of all hardware models.

See `THIRD-PARTY-NOTICES.txt` and `Licenses/` for dependency notices.
