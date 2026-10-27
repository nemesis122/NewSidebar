using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Aurora;
public static class Prerequisites
{
    public const string PawnIoUrl="https://github.com/namazso/PawnIO.Setup/releases/latest/download/PawnIO_setup.exe";
    public static async Task InstallPawnIO(Window owner)
    {
        if(MessageBox.Show(owner,"Offiziellen PawnIO-Installer herunterladen und nach Prüfung seiner Windows-Signatur öffnen? Die Installation führst du im Installer selbst aus.","PawnIO installieren",MessageBoxButton.OKCancel)!=MessageBoxResult.OK)return;
        string folder=Path.Combine(Preferences.Directory,"Setup");Directory.CreateDirectory(folder);
        string file=Path.Combine(folder,"PawnIO_setup.exe"),partial=file+".download";
        try
        {
            using var client=new HttpClient {Timeout=TimeSpan.FromMinutes(2)};
            using var response=await client.GetAsync(PawnIoUrl,HttpCompletionOption.ResponseHeadersRead);response.EnsureSuccessStatusCode();
            var uri=response.RequestMessage!.RequestUri!;
            if(uri.Scheme!="https" || !(uri.Host=="github.com" || uri.Host=="release-assets.githubusercontent.com" || uri.Host=="objects.githubusercontent.com"))throw new IOException("Unerwartete Downloadquelle. Bitte die offizielle PawnIO-Webseite verwenden.");
            using(var input=await response.Content.ReadAsStreamAsync())
            using(var output=File.Create(partial))
            {var buffer=new byte[81920];long total=0;int read;while((read=await input.ReadAsync(buffer))>0){total+=read;if(total>32*1024*1024)throw new IOException("Installer überschreitet die zulässige Dateigröße.");await output.WriteAsync(buffer.AsMemory(0,read));}}
            if(!await Task.Run(()=>HasTrustedSignature(partial)))throw new IOException("Die Windows-Signatur des Downloads konnte nicht bestätigt werden. Die Datei wird nicht ausgeführt.");
            File.Move(partial,file,true);
            Process.Start(new ProcessStartInfo(file){UseShellExecute=true,Verb="runas"});
            MessageBox.Show(owner,"Nach Abschluss der Installation die Sidebar neu starten. Falls Sensorrechte fehlen, unter Einrichtung den Sensorzugriff einmalig einrichten.","PawnIO-Einrichtung");
        }
        finally{if(File.Exists(partial))File.Delete(partial);}
    }
    public static bool HasTrustedSignature(string path)
    {
        IntPtr name=Marshal.StringToCoTaskMemUni(path),info=IntPtr.Zero;
        var action=new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
        var data=new TrustData{Size=(uint)Marshal.SizeOf<TrustData>(),Ui=2,Choice=1,StateAction=1};
        try
        {
            var file=new TrustFile{Size=(uint)Marshal.SizeOf<TrustFile>(),Path=name};info=Marshal.AllocHGlobal(Marshal.SizeOf<TrustFile>());Marshal.StructureToPtr(file,info,false);data.File=info;
            return WinVerifyTrust(new IntPtr(-1),ref action,ref data)==0;
        }
        finally{data.StateAction=2;WinVerifyTrust(new IntPtr(-1),ref action,ref data);if(info!=IntPtr.Zero)Marshal.FreeHGlobal(info);Marshal.FreeCoTaskMem(name);}
    }
    [StructLayout(LayoutKind.Sequential)]private struct TrustFile{public uint Size;public IntPtr Path,Handle,Subject;}
    [StructLayout(LayoutKind.Sequential)]private struct TrustData{public uint Size;public IntPtr Policy,Sip;public uint Ui,Revocation,Choice;public IntPtr File;public uint StateAction;public IntPtr State,Url;public uint Flags,Context;public IntPtr Signature;}
    [DllImport("wintrust.dll",ExactSpelling=true)]private static extern int WinVerifyTrust(IntPtr window,ref Guid action,ref TrustData data);
}
