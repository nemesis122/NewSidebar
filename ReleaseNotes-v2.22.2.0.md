Sidebar by NemeSIS v2.22.2.0 — local build, not published

Three additional layouts with different structures, rather than another main dial and reading list:

- Ring Matrix: each numeric sensor has its own mini ring containing its reading. Fields reflow to the available width; IP addresses use full-width fields.
- Capsule Dock: floating rounded reading capsules without the large enclosing card surface. Dense inline readings and width-aware wrapping.
- Equalizer Deck: each numeric sensor has its own vertical segmented meter, label and reading. No added history or fabricated samples.

Select Settings > Appearance & text > Card layout. All earlier designs remain available (76 total). Existing hardware names, selections, text/icon controls, graphic size, chart color/thickness, background-only opacity, warning limits and shared HW Sensors grouping remain available. These sensor-field compositions have no separate main graphic followed by a duplicate list; the existing Text below the graph option is relevant to layouts with that structure. Instrument number/unit visibility does not control ordinary reading text inside these fields; use Readings.

The values use the same snapshots and metric maxima as the existing Sidebar. Missing readings stay unavailable. There are no new hardware accesses or timers. Preview images use demo hardware and simulated readings only.

Validation: 102,993 native UI checks, 4,256 render combinations, including 888 targeted structural/reading checks for the new layouts. Previous unchanged data/sensor suites passed 181 configuration and 126 sensor fixture/safety checks. Portable identity verifies version 2.22.2.0 and bundled .NET 8.0.33. The EXE is unsigned. No new physical hardware, reboot or long-duration test was performed for this presentation change.

Close the previous Sidebar and retain aurora-settings.json beside the new EXE to preserve settings. The running installation and startup task were not changed. Nothing was published to GitHub.
