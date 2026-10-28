Sidebar by NemeSIS v2.15.0 — Six new history designs

Panoramic Flow, Split Wave, Signal Strip, Dual Trace, Floating Chart and Telemetry Grid.
47 normal layouts and 14 themes; all previous designs remain, without large-reading duplicates.
Dual Trace uses two independently labeled/scaled actual metrics when available.
All six layouts reuse bounded memory-only live history; no added hardware polling, file
logging, automatic IP lookup or invented warm-up data. Missing readings remain gaps.

Existing text roles, graph/text colors, chart-size and stroke controls, background-only
opacity, selected readings and multi-column graph placement are supported. Charts fill
the actual available width even in fit-to-view.

Validation: 102 data/startup/history checks; 54,319 native UI checks; 2,632 render combinations.
Demo previews show six distinct compositions, plus eight-device two-column sidebars.
Portable metadata verified as 2.15.0.0. Windows 10/11 x64; .NET 8.0.32 bundled.

Exit older copies before upgrading. Keep aurora-settings.json beside the new EXE in a
writable folder to preserve settings. Existing startup relocation may require one Windows
approval when moving to another EXE path. Some sensors need PawnIO and administrator access.
Sensor availability depends on hardware, motherboard, firmware, drivers and permissions.
See README.md for controls, previews and build instructions; dependency licenses included.
