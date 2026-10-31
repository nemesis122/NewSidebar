Sidebar by NemeSIS v2.22.8.0 — local build, not published

Removed 18 requested designs from the selectable catalog and factory routing:
Pixel Ledger, Bento, Bento Live, Micro Grid, Ring Matrix, Equalizer Deck, Radar Mesh, Layer Stack, Fader Bank, Data Spine, Range Deck, Column Film, Ruler Tracks, Aperture Grid, Margin Notes, Bridge Rows, Corner Tiles and Chrono Dots.

91 designs remain, including the original Violet Monitor and all eight new Violet variants. A saved removed key, including its legacy .Large suffix, resolves to Original. Device/sensor selections and other settings are retained. Shared rendering implementations remain where needed by surviving designs; removed choices cannot be selected or instantiated through the layout factory.

Validation: 127,890 native UI checks /5,096 render combinations passed. Regression checks verify all18 removed keys are absent, resolve safely and render Original; surviving design suites use the active catalog. Existing data/configuration and sensor behavior are unchanged; the immediately preceding build passed181 data checks and126 sensor fixture checks. No new physical-hardware, logon or long-duration test. Two pre-existing upstream CS0618 warnings remain.

Portable single-file Windows x64 EXE, unsigned, version2.22.8.0 and bundled .NET8.0.33 verified with actual EXE/config paths and zero embedded OHM driver resources. Running user installation, settings, startup task and GitHub were not changed. Close the previous instance before using this build.
