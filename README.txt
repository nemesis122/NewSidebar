Sidebar by NemeSIS v2.12.0 — Hardware names & fonts

CHANGES
- Set a custom Hardware name in Settings > Devices & readings. Select a device,
  enter a name, then choose Apply or press Enter. Restore original name resets it.
- Choose an installed Windows font in Settings > Appearance & text > Font family
  (all text). The sidebar, instrument text and settings use the selected font.
- Names and fonts persist in aurora-settings.json. Renaming does not change sensor IDs.
- Corrected System information: the displayed version comes from the running assembly.
- Includes the existing independent text and whole-graphic size controls.

PORTABLE UPGRADE
Windows 10/11 x64; .NET 8.0.32 is bundled. Exit the existing sidebar through its
notification-area menu before replacing its EXE. Keep the previous executable path
and filename if an autostart or sensor-access task already points there. Preserve
the adjacent aurora-settings.json. Opening another downloaded copy can redirect to
the old installation when that instance or its configured task is still active.

VALIDATION
83 data checks and 25,904 native UI checks passed; 2,688 render combinations.
The corrected portable EXE passed startup verification as 2.12.0.0.
Sensor access depends on hardware, firmware, drivers and permissions. Some readings
require PawnIO and one-time administrator setup. IP addresses refresh only on request.

Build instructions: Build-Portable.ps1. Dependency notices: Licenses/ and
THIRD-PARTY-NOTICES.txt. SHA256SUMS.txt identifies the corrected release assets.
