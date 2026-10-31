using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Aurora;
public partial class App : Application
{
    private Mutex? instance;
    private EventWaitHandle? showRequest;
    private RegisteredWaitHandle? showWait;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try { Start(e); }
        catch(Exception error)
        {
            string report=$"{DateTimeOffset.Now:O}\nVersion: {ReleaseIdentity.Version}\nEXE: {Environment.ProcessPath}\n{error}\n";
            string? saved=null;
            foreach(string path in new[]{Path.Combine(AppContext.BaseDirectory,"startup-error.log"),Path.Combine(Path.GetTempPath(),"NemeSIS-startup-error.log")})
            {
                try { File.AppendAllText(path,report);saved=path;break; } catch { }
            }
            MessageBox.Show("Sidebar could not start.\n\n"+error.Message+(saved==null?"":"\n\nDiagnostic log: "+saved),"Sidebar by NemeSIS",MessageBoxButton.OK,MessageBoxImage.Error);
            Shutdown(1);
        }
    }
    private void Start(StartupEventArgs e)
    {
        if(e.Args.Contains("--configure-sensor-access"))
        {
            try{if(!SensorAccess.IsAdministrator)ScheduledAutostart.RequestAccess();else{ScheduledAutostart.ConfigureAccess();MessageBox.Show("Sensorzugriff eingerichtet. Bitte die Sidebar anschließend neu starten.","Sidebar by NemeSIS");}}
            catch(Exception error){MessageBox.Show(error.Message,"Sensorzugriff");}Shutdown();return;
        }
        if(e.Args.Contains("--configure-autostart") || e.Args.Contains("--disable-autostart"))
        {
            bool enabled=e.Args.Contains("--configure-autostart");
            try
            {
                if(!SensorAccess.IsAdministrator) ScheduledAutostart.Request(enabled);
                else
                {
                    ScheduledAutostart.Configure(enabled);
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"autostart-status.txt"),DateTime.Now+"\n"+ScheduledAutostart.Describe());
                    MessageBox.Show(enabled ? "Autostart eingerichtet. Sidebar by NemeSIS startet bei deiner Windows-Anmeldung automatisch mit Sensorzugriff. Eine erneute Administratorabfrage ist dann nicht erforderlich." : "Autostart deaktiviert.","Sidebar by NemeSIS");
                }
            }
            catch(Exception error)
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"autostart-status.txt"),"FEHLER: "+error);
                MessageBox.Show(error.Message,"Autostart konnte nicht eingerichtet werden");
            }
            Shutdown(); return;
        }
        int verify=Array.IndexOf(e.Args,"--verify-portable");
        if(verify>=0 && verify+1<e.Args.Length)
        {
            File.WriteAllText(e.Args[verify+1],System.Text.Json.JsonSerializer.Serialize(new{Version=ReleaseIdentity.Version,AssemblyVersion=typeof(App).Assembly.GetName().Version!.ToString(),Executable=Environment.ProcessPath,Settings=Preferences.Path,Runtime=Environment.Version.ToString(),SensorLibrary=typeof(LibreHardwareMonitor.Hardware.Computer).Assembly.GetName().Version!.ToString(),OpenSensorLibrary=typeof(OpenHardwareMonitor.Hardware.DriverlessComputer).Assembly.GetName().Version!.ToString(),OpenSensorResources=typeof(OpenHardwareMonitor.Hardware.DriverlessComputer).Assembly.GetManifestResourceNames(),SensorSources=SensorSources.All.Select(s=>s.Key).ToArray(),CpuTemperature=CpuTemperatureSelector.Select("/cpu/0",new[]{new SensorRow("/board/cpu","Board","CPU","Temperature",50,"°C","SuperIO")},null)?.Value}));
            Shutdown();return;
        }
        int waiting = Array.IndexOf(e.Args,"--wait-for-exit");
        if(waiting >= 0 && waiting+1 < e.Args.Length && int.TryParse(e.Args[waiting+1],out int previousId))
        { try { Process.GetProcessById(previousId).WaitForExit(10000); } catch(ArgumentException) { } }
        if (e.Args.Contains("--probe"))
        {
            var probeService = new SensorService(); int probeCount=0;
            probeService.Updated += snapshot => {
                if(++probeCount<3) return;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"probe.json"),System.Text.Json.JsonSerializer.Serialize(snapshot,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
                probeService.Stop(); Dispatcher.BeginInvoke(new Action(Shutdown));
            }; probeService.Start(); return;
        }
        if(e.Args.Contains("--repair-startup-location"))
        {
            try { ScheduledAutostart.RelocateToCurrentExecutable(); }
            catch(Exception error)
            {
                MessageBox.Show("The startup location could not be updated. The selected copy will still open.\n\n"+error.Message,"Sidebar by NemeSIS",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }
        else if(ScheduledAutostart.NeedsRelocation)
        {
            try
            {
                bool elevated=SensorAccess.IsAdministrator;
                ScheduledAutostart.RequestRelocation();
                if(!elevated) { Shutdown();return; }
            }
            catch(System.ComponentModel.Win32Exception error) when(error.NativeErrorCode==1223)
            {
                // A declined UAC request must not prevent the manually selected portable copy from opening.
            }
            catch(Exception error)
            {
                MessageBox.Show("The startup location could not be updated. This copy will start without changing the existing task.\n\n"+error.Message,"Sidebar by NemeSIS",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }
        if(!SensorAccess.IsAdministrator && ScheduledAutostart.Available)
        {
            try
            {
                using var existing=EventWaitHandle.OpenExisting(StartupIdentity.ShowEventName(Environment.ProcessPath!));
                existing.Set(); Shutdown(); return;
            }
            catch(WaitHandleCannotBeOpenedException) { }
            catch(UnauthorizedAccessException) { }
            try { ScheduledAutostart.Run(); Shutdown(); return; }
            catch(Exception error) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"autostart-launch-error.txt"),error.ToString()); }
        }
        instance = new Mutex(true, StartupIdentity.MutexName(Environment.ProcessPath!), out bool created);
        if(!created && SensorAccess.IsAdministrator)
        {
            try { created=instance.WaitOne(5000); }
            catch(AbandonedMutexException) { created=true; }
        }
        showRequest = new EventWaitHandle(false,EventResetMode.AutoReset,StartupIdentity.ShowEventName(Environment.ProcessPath!));
        if (!created) { showRequest.Set(); Shutdown(); return; }
        DispatcherUnhandledException += (_, args) => {
            try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "aurora-errors.log"), DateTime.Now + " " + args.Exception + Environment.NewLine); } catch { }
            MessageBox.Show(args.Exception.Message, "Sidebar by NemeSIS"); args.Handled = true;
        };
        if (e.Args.Contains("--probe"))
        {
            var service = new SensorService(); int count = 0;
            service.Updated += snapshot => {
                if (++count < 3) return;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "probe.json"), System.Text.Json.JsonSerializer.Serialize(snapshot, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                service.Stop(); Dispatcher.BeginInvoke(new Action(Shutdown));
            };
            service.Start(); return;
        }
        var window = new SidebarWindow(); MainWindow = window; window.Show();
        showWait = ThreadPool.RegisterWaitForSingleObject(showRequest,(_,_)=>Dispatcher.BeginInvoke(new Action(()=> { window.Restore(); window.OpenSettings(); })),null,Timeout.Infinite,false);
        if (e.Args.Contains("--settings")) window.OpenSettings();
    }
    protected override void OnExit(ExitEventArgs e) { showWait?.Unregister(null); showRequest?.Dispose(); instance?.Dispose(); base.OnExit(e); }
}
