# Sidebar by NemeSIS v2.22.0.0

## Compact, readable original designs

- Narrow instrument cards reserve space for readings and reduce instrument size when needed. This fixes CPU clock and fan values extending beyond the edge of Performance HUD at 200 px.
- Long labels use compact single-line presentation when enough space remains for the complete reading. Very narrow rows still use stacked presentation.
- Readings stay right-aligned in stacked rows as well as inline rows.
- All 69 existing designs retain their identity. No new layout collection, extra tiles or additional settings are introduced.

The regression suite includes 139 additional readability checks across all 69 layouts at 200 and 300 px. Existing native UI, grouping, history, typography and lifecycle checks remain enabled.

## Retained functionality

Optional HW Sensors shared frame, two-decimal CPU clocks, new sidebar action icons, hardware visibility/reset fixes and both embedded hardware readers from v2.21.2.4 remain included. Existing adjacent `aurora-settings.json` settings are compatible.

## Download and upgrade

Exit the old Sidebar through its tray menu, extract **Sidebar-v2.22.0.0-Portable.zip** into a writable folder, and keep your settings file beside **Sidebar-by-NemeSIS-v2.22.0.0.exe**. System information reports **2.22.0.0**.

Windows 10/11 x64. .NET 8.0.33 is bundled. The EXE is unsigned. The source archive includes the matching project, tests and dependency licenses.

The separately reviewed security-hardening proposals are not implemented by this design release. Hardware support depends on the available readers, firmware, drivers and permissions; missing readings remain unavailable.

Package temperature captions are shortened to **Temp**. Sensor-card tooltips are disabled; settings and application controls retain their normal help.
