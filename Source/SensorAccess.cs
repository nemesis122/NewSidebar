using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace Aurora;
public static class SensorAccess
{
    public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    public static string Describe()
    {
        var version = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO", "DisplayVersion", null)?.ToString();
        if (version == null) return "PawnIO ist nicht installiert. CPU-Temperaturen und weitere Hardware-Sensoren benötigen diesen Sensortreiber.";
        using var handle = CreateFile(@"\\?\GLOBALROOT\Device\PawnIO", 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
        int error = Marshal.GetLastWin32Error();
        if (handle.IsInvalid) return $"PawnIO {version}: Zugriff fehlgeschlagen (Windows {error}: {new Win32Exception(error).Message})." + (!IsAdministrator ? " Sidebar mit Administratorrechten neu starten." : " Treiber-/Modulkompatibilität prüfen.");
        return $"PawnIO {version}: Treiberzugriff verfügbar. " + (IsAdministrator ? "Administratorrechte aktiv." : "Standardbenutzer.");
    }
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    private static extern SafeFileHandle CreateFile(string name,uint access,uint share,IntPtr security,uint mode,uint attributes,IntPtr template);
}
