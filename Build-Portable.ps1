$ErrorActionPreference = "Stop"
dotnet publish Source/SidebarByNemeSIS.csproj -c Release -r win-x64 --self-contained true -p:RuntimeFrameworkVersion=8.0.33 -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true --configfile Source/NuGet.Config -o publish
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
Copy-Item -LiteralPath publish/SidebarByNemeSIS.exe -Destination publish/Sidebar-by-NemeSIS.exe -Force
