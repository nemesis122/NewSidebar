Sidebar by NemeSIS v2.22.5.0 — local build, not published

Eight new native WPF compositions, bringing the selectable catalog to 100 layouts:

- Margin Notes: readings form a left-hand typographic column, with annotations on the right. An open surface avoids repeating an enclosing box.
- Bridge Rows: a compact signal bridge sits between each numeric label and its right-aligned value.
- Folded Sheet: cut-corner data sheets separate the label above-left from its reading below-right.
- Corner Tiles: responsive value/label fields with triangular corner indicators. The filled triangle area, rather than its side length, is proportional to the normalized reading.
- Chrono Dots: timestamped dot histories sit beside each numeric reading. These use actual shared samples and each visible series peak; empty or missing samples are not invented or interpolated.
- Label Dock: reading compartments with attached label tabs.
- Ladder Grid: offset fields with stepped, segmented signal bands.
- Value Rail: a continuous tinted reading column spans the rows.

Choose Settings > Appearance & text > Card layout. All previous 92 designs remain selectable. This work uses Refactoring UI and the project-specific Sidebar Design Lab guidance for hierarchy, native layout and comparison of different compositions. The skills are authoring guidance, not runtime dependencies and are not included in the application/source archive.

All selected metrics remain visible. New compositions retain hardware names, text/icon visibility and scaling, font/color overrides, whole-graphic size where applicable, background-only opacity, shared HW Sensors and warning frames. These per-field layouts do not use a separate primary hero number; their text uses the ordinary Readings control. Their per-field structures stay intact rather than moving one main instrument above a list.

Internal and external IPs remain text-only, with no meter, history graph or extra network requests. The new line-oriented Bridge Rows and Value Rail styles are covered by shared IP-edge alignment checks. Address wrapping preserves the complete string. Existing clock formatting, sensor-source choices, manual IP refresh, hardware access and startup behavior are unchanged.

Validation: 138,703 native UI checks and 5,600 layout/color/width combinations passed, including 2,592 focused new-design checks. Large text, graphic sizes, missing readings, actual/empty history, text-only IPs and expanded heights are checked. 181 data/configuration and 126 sensor fixture/safety checks passed. The exhaustive render harness periodically collects transient native bitmap buffers so the larger catalog does not exhaust WPF bitmap resources. Existing renderer source files were preserved. Two pre-existing upstream CS0618 warnings remain.

Portable identity verified version2.22.5.0, its own EXE/config paths, bundled .NET8.0.33, and zero embedded OHM driver resources. This is an unsigned Windows x64 single-file portable EXE. No new physical-hardware, restart/logon or long-duration test was performed. Preview uses clearly labeled demo hardware and simulated readings/history rendered through the production WPF controls.

No installed running app, user settings, startup task or GitHub release was changed. Close the previous instance before trying this EXE, and keep your configuration beside your chosen EXE.
