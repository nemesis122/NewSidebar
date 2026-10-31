# Sidebar by NemeSIS v3.01.3.0

Portable Windows x64 hardware-monitor Sidebar. **EXE and matching source ZIP are both attached.**

## Changed: every glass reading sits in a capsule

- **RAM total and drive capacity** now sit in the same glass capsule as every other reading, without a fill (a fill there would always be full). Rows line up with the rest of the card.
- **Link speed** gets a capsule that shows how much of the link is in use right now: download plus upload against the link speed. At 10 Gbit/s a large download fills it visibly; when idle it stays nearly empty. The value keeps showing the link speed.
- IP addresses stay plain rows so long IPv6 addresses can wrap.

Included from earlier v3.01 updates: temperature is the large number for CPU, GPU and drives; voltages, clocks, memory sizes and link speeds keep their decimals.

## Download and update

Download **Sidebar-by-NemeSIS-v3.01.3.0.exe** for the portable app. **Sidebar-v3.01.3.0-Source.zip** is the matching source. Close the previous instance and keep `aurora-settings.json` beside the chosen EXE; your settings carry over unchanged. The EXE is unsigned. See **VALIDATION.txt** and **SHA256SUMS.txt** for test scope and integrity.
