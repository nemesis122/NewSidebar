using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Aurora;
public static class ScheduledAutostart
{
    public const string TaskName="Sidebar by NemeSIS";
    public static bool Enabled
    {
        get
        {
            try { dynamic scheduler=Connect(); dynamic task=scheduler.GetFolder("\\").GetTask(TaskName); return (bool)task.Enabled && (bool)task.Definition.Triggers.Item(1).Enabled; }
            catch { return false; }
        }
    }
    public static bool Available
    {
        get { try { dynamic scheduler=Connect(); dynamic task=scheduler.GetFolder("\\").GetTask(TaskName); return (bool)task.Enabled; } catch { return false; } }
    }
    private static dynamic Connect()
    {
        var type=Type.GetTypeFromProgID("Schedule.Service") ?? throw new InvalidOperationException("Windows-Aufgabenplanung ist nicht verfügbar.");
        dynamic scheduler=Activator.CreateInstance(type)!; scheduler.Connect(); return scheduler;
    }
    public static void Configure(bool enabled)
    {
        if(!SensorAccess.IsAdministrator) throw new InvalidOperationException("Die einmalige Einrichtung benötigt Administratorrechte.");
        dynamic scheduler=Connect(); dynamic folder=scheduler.GetFolder("\\");
        if(!enabled)
        {
            try
            {
                dynamic task=folder.GetTask(TaskName); dynamic current=task.Definition;
                foreach(dynamic existingTrigger in current.Triggers) existingTrigger.Enabled=false;
                current.Settings.Enabled=true;
                folder.RegisterTaskDefinition(TaskName,current,6,WindowsIdentity.GetCurrent().Name,null,3,null);
            } catch(COMException e) when((uint)e.HResult==0x80070002) { }
            return;
        }
        string user=WindowsIdentity.GetCurrent().Name;
        dynamic definition=scheduler.NewTask(0);
        definition.RegistrationInfo.Description="Startet Sidebar by NemeSIS bei der Benutzeranmeldung mit Sensorzugriff, ohne erneute UAC-Abfrage.";
        definition.RegistrationInfo.Author=user;
        definition.Principal.UserId=user;
        definition.Principal.LogonType=3; // InteractiveToken: no password is stored.
        definition.Principal.RunLevel=1; // HighestAvailable; approved once during setup.
        definition.Settings.Enabled=true;
        definition.Settings.AllowDemandStart=true;
        definition.Settings.StartWhenAvailable=true;
        definition.Settings.DisallowStartIfOnBatteries=false;
        definition.Settings.StopIfGoingOnBatteries=false;
        definition.Settings.ExecutionTimeLimit="PT0S";
        definition.Settings.MultipleInstances=2;
        dynamic trigger=definition.Triggers.Create(9);
        trigger.UserId=user; trigger.Delay="PT5S"; trigger.Enabled=true;
        dynamic action=definition.Actions.Create(0);
        action.Path=Environment.ProcessPath!;
        action.WorkingDirectory=Preferences.Directory;
        folder.RegisterTaskDefinition(TaskName,definition,6,user,null,3,null);
        // Replace this app's legacy unelevated Run entry after successful registration.
        using var run=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true);
        var legacy=run?.GetValue("AuroraSidebar") as string;
        if(legacy!=null && (legacy.Contains(@"\Aurora-Sidebar\App\AuroraSidebar.exe",StringComparison.OrdinalIgnoreCase) || legacy.Contains(Environment.ProcessPath!,StringComparison.OrdinalIgnoreCase)))
            run!.DeleteValue("AuroraSidebar",false);
    }
    public static void Request(bool enabled)
    {
        if(SensorAccess.IsAdministrator) { Configure(enabled); return; }
        Process.Start(new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute=true,Verb="runas",Arguments=enabled ? "--configure-autostart" : "--disable-autostart" });
    }
    public static void ConfigureAccess()
    {
        bool enabled=Enabled;Configure(true);if(!enabled)Configure(false);
    }
    public static void RequestAccess()
    {
        if(SensorAccess.IsAdministrator){ConfigureAccess();return;}
        Process.Start(new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=true,Verb="runas",Arguments="--configure-sensor-access"});
    }
    public static void Run()
    {
        dynamic scheduler=Connect(); dynamic task=scheduler.GetFolder("\\").GetTask(TaskName);
        task.Run(null);
    }
    public static string Describe()
    {
        try
        {
            dynamic scheduler=Connect(); dynamic task=scheduler.GetFolder("\\").GetTask(TaskName);
            return $"Autostart: {(Enabled ? "aktiv" : "deaktiviert")} · Aufgabenplanung · erhöhte Sensorrechte\nDatei: {task.Definition.Actions.Item(1).Path}";
        }
        catch { return "Autostart mit Sensorrechten ist noch nicht eingerichtet."; }
    }
}
