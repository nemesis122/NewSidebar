# Release validation — v3.00.0.0

Date: 31 October 2026. Release based on the explicitly restored local v2.22.8.0 baseline. The six later concept designs are excluded. The 18 removed layouts stay absent; 91 layouts remain.

## Automated functional checks

- 127,903 native WPF UI/layout/scroll checks passed across 5,096 layout/color/width combinations.
- 181 data/configuration checks passed.
- 336 sensor mapping/safety checks passed, including 210 checks across 30 synthetic CPU/GPU/source profiles.
- Native lifecycle coverage includes starting one sensor worker, preventing duplicate workers, source recovery, hiding/restoring through the tray, reopening settings, persisting warning limits/order, empty-selection recovery, and closing/releasing the backend once.
- UI coverage includes text/icon sizes and visibility, custom hardware names, selected/custom sensors, CPU temperature selection, one through four columns, 1440p/4K work areas at multiple DPI factors, fit/scrolling, manual cached IP refresh, removed-layout fallback, taskbar visibility and portable/startup-path logic.
- Startup task behavior is exercised with test definitions; no real login/reboot or task replacement was performed on the user's installation.
- Six one/two-column production-WPF demo previews and two English settings images were rendered and inspected. The one-column Twin preview uses a taller canvas to keep its text readable. 13 preview checks and 2 settings capture checks passed.

## Fresh physical engine test on the available PC

Detected: AMD Ryzen 9 5900X, NVIDIA GeForce RTX 3080, ASUS ROG STRIX X570-E GAMING and embedded controller/memory devices.

| Sequence | Source | Devices | Sensors | Available temperature sensors | Warnings |
| --- | --- | ---: | ---: | ---: | --- |
| 1 | LibreHardwareMonitor | 6 | 145 | 4 | None |
| 2 | OpenHardwareMonitorPawnIO | 6 | 146 | 9 | None |
| 3 | LibreHardwareMonitor | 6 | 145 | 9 | None |

The three fresh captures switched Libre → hybrid OHM/PawnIO → Libre and returned finite values. The cold first snapshot had fewer temperatures than the later initialized snapshots. The hardware run passed 339 checks including three live finite-value checks. This is a short engine smoke test, not a long-duration soak or a real Windows sign-in test.

## Synthetic profile coverage and limitations

Five CPU-name/core-clock/temperature patterns (AMD Ryzen 5900X, 9800X3D, 9950X3D; Intel Core i7-14700K and Core Ultra 9 285K) × three GPU patterns (NVIDIA RTX, AMD Radeon, Intel Arc) × two source choices. Tests cover selected CPU temperature precedence, core-clock precision, vendor GPU load mapping, valid zero fan values, unique IDs and clearing unavailable temperatures. Other tests cover repeated SSD model identities, Windows RAM arithmetic, storage fallback and missing-source recovery.

These fixtures do not access or certify those physical models. No fresh physical Intel, Radeon, Arc or X3D machine was available for this release. Exact support depends on the bundled libraries, drivers, firmware and permissions. Inaccessible/nonexistent sensors are not fabricated.

## Build and identity

Portable Windows x64, .NET8.0.33 bundled, unsigned. Product display/release/tag/file naming is exactly **3.00.0.0**. CLR numeric assembly/file version is **3.0.0.0**, because numeric version fields normalize leading zeros. System information and the portable verification command use the exact product display version. Actual executable and configuration paths were verified. OHM has zero embedded driver resources; removed Ring0/InpOut types/imports are checked.

Two existing upstream CS0618 warnings about UnmanagedType.AsAny remain. No new production sensor algorithm is introduced in this release. The user's running installation, configuration and startup task were not modified by validation.
