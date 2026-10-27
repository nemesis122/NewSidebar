Sidebar by NemeSIS v2.10.0

WHAT IS NEW
- Optional expanded presentation for all 48 layout variants, active only with
  two or more columns. Reading labels and values are 20 percent larger.
- Instrument layouts place the instrument above the readings. Values use the
  full widget width below it. One-column layouts remain unchanged.
- Enable the option in Settings > Appearance & text > Multiple columns:
  larger, spaced readings. It is off by default for existing installations.
- Width per column now supports 50 to 440 logical pixels. Very narrow widgets
  scale uniformly to prevent horizontal overflow; the settings button remains
  available at narrow widths.
- Expanded presentation and width choices persist across application restarts.

PORTABLE USE
Windows 10/11 x64. Run Sidebar-by-NemeSIS.exe from a writable folder.
The .NET runtime is bundled. Settings are stored in aurora-settings.json beside
this EXE. To keep your settings, replace your existing EXE in the same folder.
Some hardware sensors require PawnIO and one-time administrator setup via the
Setup page. Available readings depend on hardware, drivers and access rights.

NETWORK IP ADDRESSES
Internal and external IP addresses remain optional. Use the manual refresh
button under Devices & readings. No automatic lookup runs on startup or a
schedule. The external lookup uses api64.ipify.org and the Windows default
internet route, which may differ from the selected adapter with VPNs.

BUILD AND VALIDATION
Requires the .NET 8 SDK on Windows. Run Build-Portable.ps1 from the source root.
73 data checks, 23,039 native checks and 2,688 render combinations passed.
Software tests do not constitute physical validation of every hardware model.
See THIRD-PARTY-NOTICES.txt and Licenses/ for dependency notices.
