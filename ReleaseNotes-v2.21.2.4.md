# Sidebar by NemeSIS v2.21.2.4

## HW Sensors: original designs in one tile

Optional shared outer frame for the selected CPU, GPU, RAM, drive and network sections. All 69 original layouts retain their instruments, readings and histories. Enable **Cards & order → All readings in one card (HW Sensors)**. Existing device, reading, monitor and column selections still apply. The option is off by default.

## Fixes and improvements

- CPU clock detection accepts Intel, AMD and P-/E-core sensor names. CPU clocks display two decimal GHz digits, for example **4.11 GHz**, including individually pinned core clocks.
- **Show all text** also restores hardware icons. Size resets restore 100% while preserving visibility choices.
- New scalable vector settings and minimize-to-tray buttons with hover, pressed and keyboard focus states.
- Libre mode supplements otherwise undetected drives through the embedded OHM storage reader. Existing Libre drive identifiers and values stay intact.
- OHM mode retains available direct readings while supplementing missing values and additional sensors from Libre, including drive temperatures. Added sensors retain device ownership and selection aliases.
- A missing preferred GPU clock no longer hides a valid alternative. Intel D3D-3D load is selected explicitly. VRAM scales use detected capacity. Duration, timing, noise and conductivity sensors have their units.
- Repeated start requests keep one sensor worker. Hiding all cards removes old content and shows the empty-selection hint.

## Validation

Local validation passed **92,654 native UI checks**, **3,864 rendering combinations**, **181 data/configuration checks** and **124 sensor checks**. Optional replay of recorded hardware snapshots passes **185 sensor checks in total**, including the 124 base checks. The UI total includes 18 application lifecycle checks. This release workflow also runs the data, sensor and native UI suites before publishing.

Hardware coverage depends on firmware, vendor drivers and permissions. Missing sensors stay unavailable. Recorded hardware data and fixtures do not establish compatibility with every PC. Driver installation, Windows logon/reboot and a physical multi-monitor swap were not performed. No personal recordings or configuration files are included.

## Download and upgrade

Use **Sidebar-v2.21.2.4-Portable.zip** for the executable, licenses and documentation. The standalone EXE is also supplied; dependency notices are embedded in its bundle. **Windows 10/11 x64; .NET 8.0.33 included. The EXE is unsigned.**

Exit the old Sidebar through its tray menu, extract the package into a writable folder and keep your `aurora-settings.json` beside the new EXE. Check version **2.21.2.4** in System information. No driver or startup task is installed automatically.

The source ZIP includes the matching project, tests, build script and modified MPL-2.0 OHM source. `SHA256SUMS.txt` lists uploaded asset checksums.
