Sidebar by NemeSIS v2.22.1.0 — local preview, not published

Four modern compact layouts based on the official v2.22.0.0 source:

- Edge Compact: dense, aligned reading rows and a fine category-colored edge. No duplicated hero value or reserved chart space.
- Micro Grid: small tinted fields arranged in two columns; address fields span the full width.
- Flow Compact: a small main reading beside a short actual sensor-history chart, followed by compact reading rows.
- Orbit Mini: a small circular instrument beside dense readings. The existing Text below the graph option moves the instrument above the list.

Choose Settings > Appearance & text > Card layout. All 69 existing layouts remain available (73 total). Existing device/reading selections, custom sensors, hardware names, text and icon visibility/size, whole-graphic size, chart colors, background-only opacity, warning limits and optional HW Sensors grouping remain supported. No new polling, timers or driver changes.

Flow Compact uses the shared bounded history; it starts empty and retains gaps for missing samples. Preview images explicitly use demo hardware and simulated history. All selected readings remain in the widget, including network DL/UL/Link and enabled IP addresses. CPU clock formatting and the v2.22.0.0 hardware fixes are retained.

Portable Windows x64 EXE with .NET 8.0.33 bundled. The EXE is unsigned. Close the previous Sidebar before trying this local build, and retain aurora-settings.json beside the chosen EXE. The running user installation, configuration and startup task were not changed. Nothing was published to GitHub.

Validation: 98,799 native UI checks, 4,088 layout/color/width render combinations, 181 data/configuration checks and 126 sensor fixture/safety checks passed. The compact-layout checks retain every selected reading, verify narrow-card boundaries, missing readings and empty/real histories. Existing suites exercise shared HW Sensors grouping, languages, typography, visibility, graphic scaling and lifecycle. Portable identity reports version 2.22.1.0 and bundled runtime 8.0.33. This presentation-only update did not repeat physical hardware or Windows logon tests.
