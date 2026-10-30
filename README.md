# Sidebar by NemeSIS v2.21.2

A portable Windows hardware sidebar with **69 layouts**, **14 color themes**, native **WPF UI 4.3.0 settings** and selectable embedded sensor sources. This release includes the designs and text-visibility controls developed since v2.18.0.

## New: hardware icon size and visibility

**Settings → Text sizes → Hardware icons** now has the same **50–200% slider, numeric input and Show switch** as Widget titles. Resize or hide hardware header icons independently of text. Hidden icons reserve no width or trailing margin; their saved size is retained. Reset text sizes restores icon size to 100%. Existing settings keep icons enabled at 100%; layouts without header icons retain their appearance. Settings-navigation icons, top buttons and the notification-area icon are unaffected.

## Sensor sources without WinRing0

Choose **Settings → Setup → Sensor detection**:

- **LibreHardwareMonitor + PawnIO** — the existing default sensor engine.
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

[Download Sidebar-by-NemeSIS-v2.21.2.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Sidebar-by-NemeSIS-v2.21.2.exe). **Windows 10/11 x64; .NET 8.0.32 is bundled.** No application installer or separate .NET installation is required. **The EXE is unsigned**; Windows reputation warnings can still appear.

1. Exit the previous copy through its notification-area icon.
2. Put the new EXE in your chosen writable folder. Keep **aurora-settings.json** beside it to retain your settings; different folders have separate settings.
3. Start the new EXE and verify **2.21.2.0** and the launched path under **System information**.

Some CPU/mainboard readings require **PawnIO and administrator access**, configured under **Setup**. Moving an existing startup task to a new EXE may require one Windows approval; startup on/off and delay are retained. No startup task is created automatically on a fresh PC. Available readings depend on hardware, firmware, vendor drivers and account permissions.

## Previews

All layout previews use **demo hardware and simulated readings**, rendered with the production layouts. Click a thumbnail for full size.

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/New-Designs.png" width="960" alt="Six additional modern designs with demo hardware">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/New-Designs.png)

| Dashboard Pro — two columns | Hardware icons and text sizes |
| --- | --- |
| [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/DashboardPro-2Columns.png" width="300" alt="Dashboard Pro with demo readings in two columns">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/DashboardPro-2Columns.png) | [<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-HardwareIcons.png" width="620" alt="Hardware icon size slider and independent Show switches">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-HardwareIcons.png) |

[<img src="https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-SensorSource.png" width="960" alt="English sensor setup showing the explicitly labelled Open Hardware Monitor and PawnIO bridge">](https://github.com/nemesis122/NewSidebar/releases/download/v2.21.2/Settings-SensorSource.png)

## Validation and source

**84,128 native UI checks**, **3,864 layout/color/width render combinations**, **128 data/configuration checks** and **31 sensor/driver-safety fixture checks** passed. Icon sizes are checked at 50% and 200% across all layouts, including repeated application without cumulative scaling. Existing regression checks cover source-selection fit behavior, settings controls and portable identity.

A real Libre → OHM/PawnIO → Libre switch passed on the test PC: 6 devices, 145/144/145 sensors and 9 available temperature readings per capture, with no backend warnings. The hybrid read one device directly and five via the bridge. Two relocated/renamed EXEs correctly report their own executable and adjacent configuration paths. The source archive is byte-checked against all 155 project files. These checks do not establish compatibility with every physical PC; no fresh-machine, reboot or long-duration test was performed for this build.

The portable EXE independently reports **2.21.2.0**, LibreHardwareMonitor **0.9.6.0**, the modified OHM assembly **1.0.9513.1** and **zero OHM embedded resources**. The repository tag and **Sidebar-by-NemeSIS-v2.21.2-source.zip** contain matching source, tests, build script, dependency notices and modified MPL-2.0 source.

Build on Windows with the .NET 8 SDK:

~~~powershell
./Build-Portable.ps1
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release
dotnet run --project SensorTests/SensorChecks.csproj -c Release
~~~

Sensor checks use fixtures by default; hardware polling is an explicit optional test. See **THIRD-PARTY-NOTICES.txt**, **Licenses/**, **OpenHardwareMonitor.Driverless/README.md** and **SHA256SUMS.txt** for sources, modifications, licenses and uploaded asset checksums.
