# Sidebar by NemeSIS v3.01.1.0

Portable Windows x64 hardware-monitor Sidebar. **EXE and matching source ZIP are both attached.** This is a fix release for v3.01.0.0: readings whose meaning lives in their decimals are no longer cut to whole numbers.

## Fixed: reading precision

Until now every reading of 1 or more was shown as a whole number, so a CPU voltage of 1.35 V appeared as "1 V" and a GPU clock of 1905 MHz as "1 GHz". Each unit now has its own precision:

| Unit | Before | Now |
| --- | --- | --- |
| Voltage | 1 V | 1.256 V (always three decimals) |
| GPU and other clocks above 1 GHz | 1 GHz | 1.91 GHz (two decimals, like the CPU clock) |
| Memory and drive sizes | 13 GB | 13.9 GB; from 100 GB whole numbers (1863 GB) |
| Link speed | 2 Gbit/s on 2.5 GbE | 2.5 Gbit/s |
| Drive read/write below 10 | 1 GB/s | 1.5 GB/s |

Temperatures, percentages, watts, fan speeds, VRAM in MB and network MB/s keep their whole-number display. Decimals always use a point, independent of the Windows language, matching the CPU clock. The RAM card title shows the installed size with one decimal as well.

## Download and update

Download **Sidebar-by-NemeSIS-v3.01.1.0.exe** for the portable app. **Sidebar-v3.01.1.0-Source.zip** is the matching source. Close the previous instance and keep `aurora-settings.json` beside the chosen EXE; your settings carry over unchanged. The EXE is unsigned. See **VALIDATION.txt** and **SHA256SUMS.txt** for test scope and integrity.
