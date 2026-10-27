Sidebar by NemeSIS v2.11.0

NEW IN THIS RELEASE
- Six independent text size sliders, from 50 to 200 percent, on the new Text
  sizes settings page: widget titles, hardware names, labels, readings,
  instrument numbers and instrument units. The general text size still applies.
- Widget titles and hardware names follow the selected text sizes. Long
  hardware names wrap instead of being cut off.
- Graphic size (%) is directly below Text size in Appearance & text. It scales
  the whole instrument, including the number and unit, from 50 to 200 percent.
  Available space limits enlargement in narrow widgets. Bar layouts also use
  the graphic size setting. Text sizes remain separately adjustable.
- The option for instruments above readings is now named Text below the graph
  in English, with translations for all six supported settings languages.
  It applies with 2 or more columns and is disabled with one column.
- Text sizes can be reset to 100 percent. Choices are saved across restarts.
- Notification-area icon only: sidebar and settings have no taskbar entry.
  Right-click the notification-area icon to open Settings, show or exit.
- Fixed clipped instrument digits when combining text and graphic scaling.

PORTABLE UPGRADE
Windows 10/11 x64. Exit via the notification-area menu before replacing the
existing EXE. Keep the EXE in the same writable folder to preserve the adjacent
aurora-settings.json. No application installer or separate .NET installation
is needed. Some sensors require PawnIO and one-time administrator setup.

NETWORK ADDRESSES
Internal and external IP displays remain optional. Addresses are fetched and
saved only when the user presses the refresh button in Devices & readings.
No automatic lookup runs on startup or a schedule. External lookup uses
api64.ipify.org and the Windows default internet route.

VALIDATION
78 data checks, 25,801 native checks and 2,688 render combinations passed.
Checks cover independent text roles, persistence, reset, all 48 layouts,
narrow widths, multiple columns, graphic scaling and notification-area mode.
Sensor availability depends on hardware, motherboard, drivers and access rights.
Software tests do not constitute physical validation of every hardware model.
Build on Windows using the .NET 8 SDK and Build-Portable.ps1.
See THIRD-PARTY-NOTICES.txt and Licenses/ for dependency notices.
