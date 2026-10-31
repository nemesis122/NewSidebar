Sidebar by NemeSIS v2.22.7.0 — local build, not published

Eight additional layouts in the requested Violet Monitor visual family. The original Violet Monitor renderer is unchanged and all 101 earlier layouts remain available (109 total).

- Violet Stack: a centered dial above open two-column reading fields.
- Violet Mirror: the dial on the right and sensor bars/readings on the left.
- Violet Twin: two dials above the readings, using the primary sensor and an actual secondary load sensor or another available numeric sensor. A second dial is omitted if no second numeric sensor is available.
- Violet Tiles: compact individual bordered reading tiles and violet bars, without a duplicated primary instrument.
- Violet Console: a rectangular console display beside compact sensor bars.
- Violet Crown: a wide semicircular frame above the sensor rows.
- Violet Ribbon: a horizontal primary-reading band above full-width sensor bars.
- Violet Compact: a smaller dial beside dense sensor rows.

Select Settings > Appearance & text > Card layout. Black surfaces, gray outlines/tracks, white readings and violet-to-pink instrument frames carry forward the supplied reference appearance. Explicit text/graph/font overrides remain supported. The open rings and semicircle are decorative frames; the bars encode actual normalized metric values. No fabricated FPS, specifications or unavailable hardware values are introduced.

All selected/custom readings remain visible. Internal and external IPs are text-only, spanning the full reading-field width where applicable, and never add history or meters. Network DL/UL charts use real bounded shared samples, start empty and retain gaps for missing values. No new sensor polling, hardware access, background timers or automatic IP lookups are added.

Existing text and icon sizes/visibility, instrument number/unit sizes, whole-graphic size, graph thickness, custom hardware names, temperature units, background-only opacity, warnings and shared HW Sensors are retained. The readings-below option stacks the main instrument above its list for the inline styles, including one column. Stack, Twin, Crown and Ribbon inherently place their main display above the readings. Tiles has no separate primary instrument.

Validation: 153,677 native UI checks /6,104 layout-color-width combinations passed, including 2,592 focused checks for the eight additions. Narrow/large text, graphic scaling, stretched heights, selected/missing data, text-only IPs and empty/shared network histories are covered. 181 data/configuration and126 sensor fixture/safety checks passed. Two inherited upstream CS0618 warnings remain. Portable identity verifies version2.22.7.0, its actual EXE/config paths, .NET8.0.33 and zero embedded OHM driver resources.

Unsigned Windows x64 single-file portable EXE, with matching source archive. No new physical-hardware, restart/logon or long-duration test was performed. The production-WPF gallery uses explicitly labeled demo hardware and simulated readings/history. Close the previous Sidebar before trying the new EXE and keep your configuration beside your chosen EXE.

The running user installation, configuration, startup task and GitHub were not modified. This build is not published.
