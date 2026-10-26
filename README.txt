SIDEBAR BY NEMESIS v2.8.0 - SOURCE

Windows 10/11 x64, .NET 8 SDK, WPF, LibreHardwareMonitorLib 0.9.6.
Build: powershell -File Build-Portable.ps1
Checks: dotnet run --project Tests/Checks.csproj -c Release --configfile Source/NuGet.Config
UI checks: dotnet run --project UITests/LayoutQA.csproj -c Release --configfile UITests/NuGet.Config
UI tests require Windows desktop capabilities. Demo frames are simulated readings.

No personal settings, machine logs, credentials, compiled binaries or build caches
are included in this source archive. Third-party notices are included separately.
