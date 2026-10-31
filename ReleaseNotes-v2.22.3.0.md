Sidebar by NemeSIS v2.22.3.0 — local build, not published

Eight additional structural designs:
- Radar Mesh: normalized sensor polygon and compact reading fields. Uses up to the first eight selected numeric sensors, clockwise starting at the top; all selected readings remain in the fields. Missing axes are not replaced with fabricated zero readings.
- Honeycomb: individual sensors in hexagonal cells with a short actual-value meter.
- Layer Stack: staggered, angled data strips with an open outer surface.
- Fader Bank: vertical sensor-position meters. These are read-only graphics, not hardware controls.
- Flip Board: mechanical split-flap surfaces with prominent numerical readings.
- Data Spine: alternating reading groups around a central data line, without a large enclosing card surface.
- Line Dashboard: a separate compact actual history for each numeric sensor. Starts empty, uses the shared bounded history and retains gaps.
- Pixel Ledger: dense aligned readings with small pixel-level meters and an open outer surface.

Select Settings > Appearance & text > Card layout. All existing designs remain selectable (84 total). Selected metrics, IP fields, hardware names, text/icon controls, whole graphic size where applicable, theme/chart colors, background-only opacity, warnings and shared HW Sensors grouping remain supported. The new sensor-field compositions do not use a separate main graphic followed by a repeated reading list; ordinary reading text is controlled by Readings. Previews use demo hardware and simulated data only. No new polling, timers or sensor-control operations.

Visual review used the frontend-design guidance from anthropics/skills: deliberate structures, less repeated card chrome and more legible split-flap numerals. The skill is installed separately in the user's Codex profile and is not an application dependency or included in the source archive.

Validation: 114,345 native UI checks / 4,704 render combinations, including 2,368 targeted checks for the new layouts. Prior unchanged configuration/sensor fixture suites passed 181/126 checks. Portable version 2.22.3.0 with .NET 8.0.33. Unsigned Windows x64 EXE. No new physical hardware, reboot or soak test performed for this presentation-only update.

Close the previous Sidebar and retain aurora-settings.json beside the new EXE. The running installation, configuration and startup task were not changed. Nothing was published to GitHub.
