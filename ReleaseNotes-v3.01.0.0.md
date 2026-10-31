# Sidebar by NemeSIS v3.01.0.0

Portable Windows x64 hardware-monitor Sidebar. **EXE and matching source ZIP are both attached.** This release adds the glass design family, five color themes and a reset button on every settings page. All v3.00 layouts are unchanged.

## Glass design family

Six new layouts built on one construction: a translucent glass slab, a rim that catches light at the top edge, a sheen across the upper third, and every reading as a glass capsule.

| Layout | Material |
| --- | --- |
| Liquid Glass | Clear glass, capsules filled like liquid to their level |
| Frost Glass | Frosted white glass, brighter rim, rounder corners |
| Aqua Glass | Glass tinted in each hardware's accent color |
| Prism Glass | Rim refracted from the accent into a neighbouring hue |
| Smoke Glass | Dark smoked glass with glowing capsules |
| Glass Tubes | Slim glass tubes beneath each reading row |

- Network download/upload and drive read/write show a live 60-second trace inside their capsule, scaled to the last minute's peak.
- Link speed, RAM total and drive capacity are plain rows, because a bar there would always be full.
- Bars keep exactly the line/bar thickness you set in every card. In stretched multi-column cards, spare height becomes even spacing between rows; only traces grow.
- Every appearance setting applies: line/bar thickness, graphic size, widget background (the sheen fades with it), text below graph, all text sizes and visibility switches, all color overrides, °C/°F and warning limits. Light themes use the theme edge for the rim. In the shared HW Sensors frame the sheen sits once on the common perimeter.

## Color themes

Catppuccin Mocha, Tokyo Night, Rosé Pine, Kanagawa and Everforest, each with its own colors per hardware category. 19 themes in total.

## Settings

- Every settings page has one reset button at its bottom that resets exactly the values on that page. The first click asks for confirmation, a second click within four seconds resets.
- Overview and System offer a full reset. Settings language, Windows startup and sensor access (PawnIO) are never changed by a reset.
- Text sizes now resets sizes and visibility together, like every other page. The small reset button next to font size was removed; the Design & font reset covers it.
- All new texts are translated into German, English, French, Italian, Spanish and Portuguese.

## Under the hood

- Layouts are registered in one table instead of a long if-chain; existing layouts behave as before.

## Download and update

Download **Sidebar-by-NemeSIS-v3.01.0.0.exe** for the portable app. **Sidebar-v3.01.0.0-Source.zip** is the matching source. Close the previous instance, keep `aurora-settings.json` beside the chosen EXE, and review the startup target after moving or renaming the app. Advanced CPU/motherboard access requires PawnIO and the appropriate rights. The EXE is unsigned. See **SHA256SUMS.txt** for integrity.
