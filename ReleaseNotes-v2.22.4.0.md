Sidebar by NemeSIS v2.22.4.0 — local build, not published

Eight additional layouts developed using the installed frontend-design and UI UX Pro Max guidance:

- Signal Fan: nested semicircular signals for the first eight selected numeric sensors, ordered outside to inside; all readings remain listed.
- Petal Matrix: radial sensor petals for the first eight numeric sensors, clockwise from the top, plus all selected readings.
- Range Deck: actual recent-history minimum/maximum bands and the current position, normalized to the metric's configured maximum. Starts without a fabricated range.
- Column Film: timestamped history columns per numeric sensor. Empty/missing samples do not receive fabricated bars; each strip uses its actual visible peak.
- Ticket Rail: compact notched data slips and an open outer surface.
- Type Stack: a clear larger typographic reading hierarchy without a large graphic. Bahnschrift is its default reading face; the user's font selection still overrides it.
- Ruler Tracks: fine static scales with a current reading pointer, on an open surface.
- Aperture Grid: responsive per-sensor geometric aperture fields with their actual readings.

Choose Settings > Appearance & text > Card layout. All earlier layouts remain available (92 total). Existing device/metric selection, pins, hardware names, text/icon sizes and visibility, chart colors/thickness, whole-graphic size where applicable, background-only opacity, warning frames and shared HW Sensors remain supported. Ordinary reading text is controlled by Readings, rather than the separate primary-instrument number/unit switches. No additional hardware access or polling timers.

Internal and external IPs remain text-only, aligned with ordinary row edges in line-oriented layouts. Short addresses stay inline when possible; long addresses wrap without shrinking or ellipsis. No IP graphs or meters are created. Existing v2.22.0.0 hardware/lifecycle fixes and v2.22.3.1 address fixes are retained.

Validation: 127,001 native UI checks / 5,152 layout-color-width render combinations passed, including 2,368 focused checks for the new layouts, actual/empty history, missing values and selected metrics. The address suite also covers the new line-oriented layouts. 181 data/configuration and 126 sensor fixture/safety checks passed. Portable identity reports version 2.22.4.0 and bundled runtime8.0.33, with no OHM embedded driver resources. Two pre-existing upstream CS0618 warnings remain. No new physical-hardware, logon or long-duration test was performed.

Windows x64 single-file portable EXE; unsigned. Close the previous Sidebar and retain aurora-settings.json beside the chosen EXE. The running user installation, settings, startup task and GitHub were not modified. Preview uses demo hardware and simulated readings/history, rendered by the production WPF controls. Design skills are authoring guidance, not application dependencies and are not bundled in the source archive.
