Sidebar by NemeSIS v2.18.0

A portable Windows hardware sidebar with 63 layouts, 14 color themes and a redesigned WPF UI 4.3.0 settings menu. This release includes all changes developed after v2.15.6.

## Redesigned settings menu

- Flat navigation with Fluent icons and a useful overview page. Position, appearance, text sizes, colors, devices, card order, warnings, setup and system information have their own clear pages.
- Native Fluent buttons and selectors, on/off switches for general preferences, and numeric entry beside every slider. Type an exact width, text size, graphic size or opacity; sliders and number fields stay synchronized.
- Devices, visible readings, custom sensor assignments, custom hardware names and CPU temperature-source selection remain together under Devices & readings.
- Action failures appear in an inline status panel. Sensor installation and Windows administrator approval remain explicit actions.
- Compact forms are checked at the minimum window size. Settings support English, German, French, Italian, Spanish and Portuguese; sidebar labels remain English.

## Twelve new designs

| Design | Widget presentation |
|---|---|
| Fluent Glass | Tinted surfaces with a fine circular sensor instrument. |
| Telemetry | A real 60-second sensor chart with aligned reading rows. |
| Minimal Pro | Compact typography and a slim current-value indicator. |
| Neon Panel | Cyan/violet panels, a large header and a dial to the right of the readings. |
| Signal Deck | Blue/violet instruments with real sensor-history bars. |
| Sensor Deck | A main instrument and individual meters beside the sensor readings. |
| Redline | Black panels with red, open sports-style instruments. |
| Pulse Tiles | Reading tiles beside a matrix-style main indicator. |
| Aurora Flow | A vertical gradient meter and a clear numerical reading. |
| Focus | A prominent main value with a compact real history chart. |
| Contour | Fine rings for available primary and secondary sensor readings. |
| Monochrome | Black/white instruments and simple sensor bars. |

Choose a design in Settings → Appearance & text → Card layout. Previous designs remain selectable. History charts use the existing bounded, memory-only sensor history and start empty; missing readings remain unavailable. Decorative frames do not invent FPS, RAM specifications or unsupported sensor values.

## Customization and existing features

- Independent sizes for widget titles, hardware names, reading labels, values, instrument numbers and units; installed font selection and a separate slider for the whole graphic.
- Custom text/graph colors, graph thickness and widget-background opacity that leaves text and readings visible.
- 1–4 columns, widths of 50–440 logical pixels, multiple monitors, fit-to-view and a scrollbar shown on hover. Text below the graph works with one or more columns; expanded multi-column cards spread readings across the available height.
- Per-device selection, individual readings, custom sensor pinning to the matching widget, hardware renaming and configurable CPU temperature sources.
- Network DL/UL in whole MB/s, Link in Mbit/s or Gbit/s, and optional saved internal/external IP values refreshed only by the button. External lookup uses api64.ipify.org through the default internet route.
- Whole displayed readings truncate after unit conversion; sub-unit precision is retained, for example 31.7% → 31%, 0.73 V → 0.73 V. IP text and raw sensor/history data are preserved.
- Notification-area icon only. Existing per-folder instance isolation, startup-path checks and portable settings behavior are retained.

## Download and upgrade

Download [Sidebar-by-NemeSIS-v2.18.0.exe](https://github.com/nemesis122/NewSidebar/releases/download/v2.18.0/Sidebar-by-NemeSIS-v2.18.0.exe). Windows 10/11 x64; .NET 8.0.32 is bundled. No application installer or separate .NET installation is needed.

1. Exit the old copy through its notification-area icon.
2. Place the EXE in a writable folder and keep aurora-settings.json beside it to preserve preferences. Different folders have separate settings.
3. Start the new copy. System information should report 2.18.0.0 and the launched EXE path.

Some CPU/motherboard sensors require PawnIO and administrator access; see Settings → Setup. An existing startup task pointing to another copy may require one Windows approval to move it to the selected EXE; its on/off choice and delay are preserved. No startup task is created automatically on a fresh PC. Actual sensor availability depends on hardware, firmware, drivers and permissions.


Validation: 72,716 UI checks; 3,528 render combinations; 110 data checks. Matching source and SHA256 checksums are included.
