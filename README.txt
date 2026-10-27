Sidebar by NemeSIS v2.12.2 — Portable startup & path repair

Opening this portable EXE no longer redirects to an unrelated copy in another folder.
An existing startup task can be moved to the selected EXE and its directory with a
one-time Windows administrator approval. Startup on/off and logon delay are preserved.
Declining approval still opens the selected copy with standard-user sensor access.
System information separates Running EXE and Startup EXE and reads the actual version.
Startup failures are reported with a diagnostic log.

Exit older copies before upgrading. Keep aurora-settings.json beside the chosen EXE
to preserve preferences. On a fresh PC, configure sensor access and startup in Settings;
no new task is created automatically. Some sensors need PawnIO and administrator access.
Windows 10/11 x64. .NET 8.0.32 is bundled. No application installer is required.

Includes custom hardware names, installed font selection, six independent text size
controls, whole-graphic scaling, widget-background opacity, 48 layouts, 14 color themes,
1–4 columns, per-device readings, own sensors, and manually refreshed optional IPs.
Settings: de/en/fr/it/es/pt; sidebar labels remain English.

Validation: 92 data/startup checks, 25,924 native checks, 2,688 render combinations.
Task relocation was tested on an unregistered Windows task definition. Full Windows
approval/logon validation on the second PC remains outstanding. Sensor availability
depends on hardware, motherboard, drivers and permissions; not all hardware was tested.

See README.md for settings and build instructions. Build on Windows with the .NET 8 SDK
and Build-Portable.ps1. Dependency notices are in Licenses/ and THIRD-PARTY-NOTICES.txt.
