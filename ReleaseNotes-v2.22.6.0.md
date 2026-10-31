Sidebar by NemeSIS v2.22.6.0 — local build, not published

Violet Monitor: a new narrow-sidebar composition adapted closely from the supplied black/violet reference. The existing 100 designs remain available (101 total).

- Black surfaces with fine gray rounded outlines.
- Open violet-to-magenta ring frames, rounded ends and two small lower-left dots on CPU/GPU cards.
- White primary readings, compact gray captions and hardware labels in the header.
- Square violet sensor bars on dark gray tracks, with labels and readings immediately beneath. Selected per-core clock captions appear above their bars.
- A compact inline load bar and ordinary capacity/usage rows on memory cards.
- Compact bar fields for storage and available motherboard sensors.
- Separate small DL/UL histories with a fine grid on network cards. They start empty, consume the shared bounded actual history and leave missing samples as gaps.

Select Settings > Appearance & text > Card layout > Violet Monitor. This is the reference's visual language adapted to the user's explicitly requested narrow Sidebar, rather than its wide dashboard arrangement. Runtime hardware names and values remain those of the real selected devices. CPU/GPU rings use the available primary temperature; no FPS, BIOS fields, RAM specifications or logo are fabricated to match a sample. The colored open ring is a decorative frame as in the supplied reference, not a percentage scale; the sensor bars encode actual normalized values.

The design intentionally defaults to black, violet, magenta and gray for fidelity. Existing explicit graph/text/hardware-name/font overrides remain available. Text/icon sizes and visibility, whole-instrument size, graph thickness, readings below the main instrument (including one column), temperature units, background-only opacity, warning borders and shared HW Sensors remain supported. Every selected metric remains visible. Internal/external IPs stay text-only and use the same label/value edges as the ordinary network rows, with complete wrapping where needed. No automatic IP lookups or additional sensor acquisition were added.

Validation: 141,102 native UI checks /5,656 layout-color-width combinations passed, including 1,158 focused reference checks for actual/missing readings, empty/shared network histories, black surfaces, IPs, narrow/large text, graph thickness and background transparency. 181 data/configuration and126 sensor fixture/safety checks passed. Two inherited upstream CS0618 warnings remain. Portable identity reports version2.22.6.0, its actual EXE/config paths, bundled runtime8.0.33 and zero embedded OHM driver resources.

Unsigned Windows x64 single-file portable EXE, with matching source archive. No fresh physical-hardware, logon or long-duration test was performed. The preview is rendered by the production WPF controls and explicitly uses demo hardware and simulated readings/history. Close the previous instance before trying the new EXE; keep your configuration beside the EXE you choose.

The running user installation, configuration, startup task and GitHub were not modified. This local build is not published.
