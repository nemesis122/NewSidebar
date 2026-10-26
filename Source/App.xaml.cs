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
            File.WriteAllText(e.Args[verify+1],System.Text.Json.JsonSerializer.Serialize(new{Version=typeof(App).Assembly.GetName().Version!.ToString(),Executable=Environment.ProcessPath,Settings=Preferences.Path,Runtime=Environment.Version.ToString(),SensorLibrary=typeof(LibreHardwareMonitor.Hardware.Computer).Assembly.GetName().Version!.ToString(),CpuTemperature=CpuTemperatureSelector.Select("/cpu/0",new[]{new SensorRow("/board/cpu","Board","CPU","Temperature",50,"°C","SuperIO")},null)?.Value}));
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
        if(!SensorAccess.IsAdministrator && ScheduledAutostart.Available)
        {
            try
            {
                using var existing=EventWaitHandle.OpenExisting("Local\\AuroraSidebar-Show");
                existing.Set(); Shutdown(); return;
            }
            catch(WaitHandleCannotBeOpenedException) { }
            catch(UnauthorizedAccessException) { }
            try { ScheduledAutostart.Run(); Shutdown(); return; }
            catch(Exception error) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"autostart-launch-error.txt"),error.ToString()); }
        }
        instance = new Mutex(true, "Local\\AuroraSidebar-2026", out bool created);
        if(!created && SensorAccess.IsAdministrator)
        {
            try { created=instance.WaitOne(5000); }
            catch(AbandonedMutexException) { created=true; }
        }
        showRequest = new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\AuroraSidebar-Show");
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
