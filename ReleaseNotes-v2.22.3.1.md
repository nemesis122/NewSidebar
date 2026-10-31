Sidebar by NemeSIS v2.22.3.1 — local build, not published

Aligned internal and external IP text rows

- Int IP / Ext IP use the same label edge and reading right edge as ordinary network rows in Original, Modern, Compact, Text, Bars, Nano, Edge Compact, Flow Compact, Orbit Mini, Pixel Ledger and Line Dashboard.
- Pixel Ledger reserves its ordinary graphic gutter as empty space for address rows; no graph, meter or pixel indicator is added for IP addresses.
- Short addresses stay beside their label where space permits. Long IPv6 addresses wrap below, retain their chosen font size and stay right-aligned without ellipsis.
- Layer Stack retains its own staggered strip geometry for address text. Existing wrapped readout text aligns right within its value block.

The fix changes presentation only. It does not fetch addresses, change saved IPs, add refreshes or alter sensor sources. All 84 designs remain available.

Validation: 909 new address-alignment checks, including graphic-gutter scaling, font sizes, narrow widths, long IPv6 and unchanged history-graph counts across all 84 layouts. Full native UI suite: 115,254 checks / 4,704 render combinations passed. Prior unchanged configuration/sensor fixture suites passed 181/126 checks. Portable diagnostic: version 2.22.3.1, bundled .NET 8.0.33 and zero OHM embedded resources. No physical hardware, Windows logon or soak test repeated for this presentation change.

Preview uses demo hardware and documentation-only IP addresses. Close the previous Sidebar and retain aurora-settings.json beside the chosen EXE. The EXE is unsigned. Running installation/configuration/autostart and GitHub were not modified.
