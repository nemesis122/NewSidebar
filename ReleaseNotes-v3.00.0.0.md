# Sidebar by NemeSIS v3.00.0.0

Portable Windows x64 hardware-monitor Sidebar. **EXE and matching source ZIP are both attached.** This release uses the restored v2.22.8.0 baseline, with91 layouts; the six subsequent concept experiments are excluded and the18 removed designs remain removed.

## Make it yours

**The previews are example configurations. Layout, text, colors and columns are customizable.**

| Area | What you can change |
| --- | --- |
| Layout and placement | 91 layouts; monitor; left/right/top/bottom placement; column width; 1–4 columns; fit-to-view or scrolling; readings beneath the instrument |
| Typography | Font family, general size, separate widget-title, hardware-name, label, reading, instrument-number and unit sizes; individual visibility |
| Hardware icons | Show/hide and size |
| Colors and graphics | Color theme, text/label/hardware-name/reading colors, category graph colors, secondary color, graph track, graphic size and thickness |
| Backgrounds | Widget background opacity while foreground text remains opaque |
| Content | Device visibility, selected readings, custom sensors on their matching widget, custom hardware names, card order and optional shared HW Sensors frame |
| Sensors and operation | Sensor source, CPU temperature source, °C/°F, refresh interval, warning thresholds, startup on/off, taskbar-edge reservation and tray behavior |
| Network | Selected adapter, DL/UL, link speed and optional internal/external IPs; IPs refresh only when you press the fetch button |

Available controls are shared across the designs; a layout without a main instrument or history chart has no such element to resize. The Sidebar uses English labels. Settings support German, English, French, Italian, Spanish and Portuguese.

## Selected designs: one and two columns

These are example configurations, not fixed presets: change the layout, text, colors, graphics, opacity, size, columns and visible sensors in Settings. All preview readings are simulated demo data.

<table>
<tr><th>Design</th><th>One column</th><th>Two columns</th></tr>
<tr><td>Violet Monitor</td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletMonitor-1Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletMonitor-1Column.png" width="140" alt="Violet Monitor, one column, demo data"></a></td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletMonitor-2Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletMonitor-2Column.png" width="280" alt="Violet Monitor, two columns, demo data"></a></td></tr>
<tr><td>Violet Twin</td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletTwin-1Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletTwin-1Column.png" width="140" alt="Violet Twin, one column, demo data"></a></td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletTwin-2Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/VioletTwin-2Column.png" width="280" alt="Violet Twin, two columns, demo data"></a></td></tr>
<tr><td>Fluent Glass</td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/FluentGlass-1Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/FluentGlass-1Column.png" width="140" alt="Fluent Glass, one column, demo data"></a></td><td><a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/FluentGlass-2Column.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/FluentGlass-2Column.png" width="280" alt="Fluent Glass, two columns, demo data"></a></td></tr>
</table>


### English settings

<a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/Settings-Appearance.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/Settings-Appearance.png" width="440" alt="Appearance settings"></a>
<a href="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/Settings-TextSizes.png"><img src="https://github.com/nemesis122/NewSidebar/releases/download/v3.00.0.0/Settings-TextSizes.png" width="440" alt="Text and icon size/visibility controls"></a>

## Hardware and sensor support

Hardware and available sensors are detected on the computer running the EXE. The app does not reuse fixed sensor addresses from the developer PC. It supports many AMD Ryzen/X3D and Intel CPU systems, NVIDIA/AMD/Intel GPUs, storage devices, motherboard and cooling sensors through the bundled libraries. Exact readings depend on the model, BIOS/firmware, installed vendor drivers and access rights; not every sensor is exposed on every machine.

- **LibreHardwareMonitor + PawnIO** is the default source.
- **Open Hardware Monitor + PawnIO** is a hybrid option: a modified driverless OHM component supplies supported GPU/SMART data and Libre/PawnIO bridges CPU/motherboard and missing readings. It is not an entirely independent CPU sensor engine.
- RAM, volumes and network use Windows APIs. Advanced CPU/motherboard sensors need PawnIO and appropriate access; Setup provides status and installation/configuration actions. Setup may need administrator approval once.
- Missing readings display `—`; a stopped fan or idle load can correctly be zero. First sensor samples may need initialization time.
- No WinRing0 or InpOut driver is bundled. The existing driver/resource checks are rerun for this release.

The source includes synthetic mapping tests for Ryzen 5900X, 9800X3D and 9950X3D, Intel Core i7-14700K and Core Ultra 9 285K patterns, with NVIDIA, AMD Radeon and Intel Arc GPU patterns under both sources. These are simulated mapping profiles, **not physical tests or guarantees for every listed model**. Fresh physical testing was performed only on the available Ryzen 9 5900X / RTX 3080 / ASUS X570 system. See [VALIDATION.md](VALIDATION.md) for exact evidence and limits.

## Checked for this release

- 127,903 native UI checks /5,096 render combinations;181 data/configuration checks;336 sensor fixture/safety checks passed.
- 30 simulated CPU/GPU/source profiles, including AMD/X3D, Intel hybrid-core naming, NVIDIA, Radeon and Arc mappings.
- Fresh Ryzen9 5900X /RTX3080 /ASUS X570 source-switch smoke test: Libre145 sensors → hybrid146 → Libre145; no warnings. Temperature availability increased after initialization. Physical coverage is limited to this available system; other profiles are simulated, not physical certification.
- Tray/startup-path handling, settings reopening, worker lifetime, selected/custom sensors, manual IP fetching, widths, text/icon controls, shared HW Sensors, one-to-four columns, fit and scrolling are covered by automated functional checks.
- Product display version3.00.0.0 and actualEXE/configpaths verified; bundled.NET8.0.33. NumericCLR version3.0.0.0 is expected. No WinRing0/InpOut payload. Two inherited upstream warnings remain. No fresh physical reboot/sign-in/long-duration test.

## Download and update

Download **Sidebar-by-NemeSIS-v3.00.0.0.exe** for the portable app. **Sidebar-by-NemeSIS-v3.00.0.0-source.zip** is the matching source. Close the previous instance, keep your settings beside the chosen EXE, and review the startup target after moving/renaming the app. Advanced CPU/motherboard access requires PawnIO and the appropriate rights; Setup guides the one-time configuration. The EXE is unsigned. See **VALIDATION.md** and **SHA256SUMS.txt** for scope and integrity.
