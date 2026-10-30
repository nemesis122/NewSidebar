Sidebar by NemeSIS v2.22.0.0

A portable Windows hardware sidebar with **69 layouts**, **14 color themes**, native **WPF UI 4.3.0 settings** and selectable embedded sensor sources. This release includes the designs and text-visibility controls developed since v2.18.0.

## New in v2.22.0.0

Original designs are more compact and readable on narrow sidebars. Instruments reserve room for complete readings, long labels use compact presentation, and stacked values align to the right. Performance HUD no longer places clock/fan readings beyond a 200 px card edge. All 69 layouts retain their original character, with 139 additional readability checks at 200 and 300 px.

## Retained from v2.21.2.4

- Optional **HW Sensors** tile: all selected hardware sections share one outer frame while retaining their original design, instruments and histories in all 69 layouts. Enable it under **Cards & order**.
- CPU clock detection recognizes Intel, AMD and hybrid-core sensor names. CPU frequencies use two decimals, for example **4.11 GHz**; unavailable readings remain unavailable.
- **Show all text** also restores hardware icons. Size resets retain visibility choices. New vector settings and minimize-to-tray buttons.
- Missing storage devices can be supplemented by the built-in OHM storage reader in Libre mode. OHM mode retains available direct readings and supplements missing values and extra sensors from Libre.
- Valid alternative GPU clocks, explicit Intel D3D-3D load selection, detected VRAM capacity and additional physical sensor units.
- One sensor worker across repeated start requests; correct empty-selection rendering; verified tray restore and shutdown.

See [HW-SENSORS.md](HW-SENSORS.md) for the shared-frame behavior and [APP-PRUEFBERICHT.txt](APP-PRUEFBERICHT.txt) for validation scope.

## Hardware icon size and visibility

**Settings → Text sizes → Hardware icons** now has the same **50–200% slider, numeric input and Show switch** as Widget titles. Resize or hide hardware header icons independently of text. Hidden icons reserve no width or trailing margin; their saved size is retained. Reset text sizes restores icon size to 100%. Existing settings keep icons enabled at 100%; layouts without header icons retain their appearance. Settings-navigation icons, top buttons and the notification-area icon are unaffected.

## Sensor sources without WinRing0

Choose **Settings → Setup → Sensor detection**:

- **LibreHardwareMonitor + PawnIO** — the default sensor engine, with an identified OHM storage fallback for otherwise undetected drives.
- **Open Hardware Monitor + PawnIO** — a modified, driverless OHM subset reads supported GPU/vendor-API and Windows SMART sensors. CPU, motherboard and controller readings use the existing **LibreHardwareMonitor/PawnIO bridge**. Unsupported GPU/storage devices use an identified Libre fallback.

The OHM option is an explicitly labelled **hybrid integration**, not a complete native upstream OHM-to-PawnIO port. Neither option needs a separate monitoring application. The modified OHM reader contains no WinRing0/InpOut driver payloads, extraction code, raw-access classes or driver-installation imports. CPU/mainboard access continues through the existing PawnIO setup flow; no driver is installed automatically.

Switching runs on the sensor worker, closes the previous session first and rejects queued snapshots from another source. Memory totals, volumes and network metrics retain their Windows APIs. The modified MPL-2.0 source, upstream commit and exact integration boundaries are documented in [OpenHardwareMonitor.Driverless](OpenHardwareMonitor.Driverless/README.md).

## Fix: consistent selection when switching sources

Previously, different sensor identifiers could make hidden cards or reading rows reappear. With fit-to-view enabled, the additional content shrank the whole Sidebar and made the text look smaller.

Matching alternative-source devices now inherit your existing **hidden-device, exactly matched hidden-reading and custom hardware-name settings once**. Later explicit changes remain source-specific and survive restarts. Font-size preferences are unchanged. Identical visible content produces the same fit scale; genuinely different available content can still affect automatic fitting. Turn off fit-to-view when a fixed display scale with scrolling is preferred.

## Six additional designs

| Design | Presentation |
| --- | --- |
| Bento Live | Asymmetric reading tiles with a real sensor-history chart. |
| Glass Edge | Tinted surfaces, fine edges and a linear glass meter. |
| Waveform | A wide filled history chart and clear reading fields. |
| Vertical Rail | Compact widgets with multiple vertical sensor meters. |
| Dashboard Pro | A prominent main reading, compact fields and separate real histories. |
| Soft OLED | Black surfaces, large typography and a fine current-value meter. |

Select them under **Settings → Appearance & text → Card layout**. All previous designs remain available. Histories use bounded in-memory sensor samples, start empty and leave gaps for missing readings. No fabricated FPS or unsupported hardware specifications are added.

## Text visibility and settings

**Settings → Text sizes** now has independent **Show** switches for widget titles, hardware names, labels, readings, instrument numbers and instrument units. Each role retains its saved size while hidden. **Show all text** restores visibility; **Reset text sizes** changes sizes only. The controls work across all 69 layouts, including live updates and legacy combined number/unit instruments.

The flat Fluent settings menu retains synchronized sliders and numeric fields, six settings languages, combined device/reading controls, editable hardware names, custom sensor assignment, CPU temperature selection and explicit prerequisite setup. Sidebar labels remain English.

## Existing customization

- Installed fonts, six independent text-size controls, whole-graphic size, text/chart colors and line thickness.
- Background-only widget opacity that leaves readings visible.
- 1–4 columns, 50–440 logical pixels per column, multiple monitors, fit-to-view and a hover scrollbar. **Text below the graph** supports one or more columns; expanded multi-column cards use the available height.
- Individual devices/readings, custom sensors in their matching widgets and configurable CPU temperature sources.
- Network **DL/UL** in whole MB/s and **Link** in Mbit/s or Gbit/s. Optional saved internal/external IPs refresh only when requested; external lookup uses api64.ipify.org through the default route.
- Whole readings truncate after unit conversion while sub-unit precision remains, for example **31.7% → 31%** and **0.73 V → 0.73 V**. Raw samples, histories and IP text are preserved.
- Notification-area icon only, per-folder instance isolation and explicit startup-path repair.

## Download and upgrade

[Download the portable v2.22.0.0 package](https://github.com/nemesis122/NewSidebar/releases/download/v2.22.0.0/Sidebar-v2.22.0.0-Portable.zip). **Windows 10/11 x64; .NET 8.0.33 is bundled.** No application installer or separate .NET installation is required. **The EXE is unsigned**; Windows reputation warnings can still appear.

1. Exit the previous copy through its notification-area icon.
2. Extract the portable package into your chosen writable folder. Keep **aurora-settings.json** beside it to retain your settings; different folders have separate settings.
3. Start the new EXE and verify **2.22.0.0** and the launched path under **System information**.

Some CPU/mainboard readings require **PawnIO and administrator access**, configured under **Setup**. Moving an existing startup task to a new EXE may require one Windows approval; startup on/off and delay are retained. No startup task is created automatically on a fresh PC. Available readings depend on hardware, firmware, vendor drivers and account permissions.

## Previews

All layout previews use **demo hardware and simulated readings**, rendered with the production layouts. Click a thumbnail for full size.

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/New-Designs.png" width="960" alt="Six additional modern designs with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/New-Designs.png)

| Dashboard Pro — two columns | Hardware icons and text sizes |
| --- | --- |
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/DashboardPro-2Columns.png" width="300" alt="Dashboard Pro with demo readings in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/DashboardPro-2Columns.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-HardwareIcons.png" width="620" alt="Hardware icon size slider and independent Show switches">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-HardwareIcons.png) |

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-SensorSource.png" width="960" alt="English sensor setup showing the explicitly labelled Open Hardware Monitor and PawnIO bridge">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-SensorSource.png)

## Validation and source

**92,793 native UI checks**, **3,864 layout/color/width render combinations**, **181 data/configuration checks** and **124 sensor checks** passed. An optional replay of recorded hardware snapshots passes **185 sensor checks in total**, including the 124 base checks. The UI suite includes 18 application lifecycle checks covering startup, tray hide/restore, reopening settings, monitor fallback, card order, warnings, empty selection and backend shutdown.

Administrator hardware captures cover both embedded sources. Storage merge fixes were verified against all 15 recorded source pairs and separate fixtures. The release contains no personal measurements or local configuration. AMD/NVIDIA and other hardware variants are covered by fixtures, not a physical compatibility lab. No fresh Windows installation, driver reinstall, actual logon/reboot or long-duration test was performed. Missing sensors remain unavailable.

The portable EXE reports **2.22.0.0** with .NET **8.0.33**, LibreHardwareMonitor **0.9.6.0**, modified OHM **1.0.9513.1** and zero OHM embedded resources. The release source archive includes the matching source, tests, build script and MPL-2.0 modifications.

Build on Windows with the .NET 8 SDK:

~~~powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
dotnet run --project SensorTests/SensorChecks.csproj -c Release
~~~

Sensor checks use fixtures by default; hardware polling is an explicit optional test. See **THIRD-PARTY-NOTICES.txt**, **Licenses/**, **OpenHardwareMonitor.Driverless/README.md** and **SHA256SUMS.txt** for sources, modifications, licenses and uploaded asset checksums.

Package temperature captions are shortened to **Temp**. Sensor-card tooltips are disabled; settings and application controls retain their normal help.
