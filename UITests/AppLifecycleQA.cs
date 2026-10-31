using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Aurora;
using LibreHardwareMonitor.Hardware;

static class AppLifecycleQA
{
 const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 public static void Verify(Action<bool,string> check)
 {
  var backend=new LifecycleBackend();var service=new SensorService(new SensorBackendSession(_=>backend)){Interval=30};
  var sidebar=new SidebarWindow(service){ShowActivated=false};
  sidebar.Prefs.ReserveSpace=false;sidebar.Prefs.Topmost=false;sidebar.Prefs.SensorSource=SensorSources.Libre;sidebar.Prefs.Interval=500;sidebar.Prefs.Monitor="missing-monitor-test";sidebar.Prefs.HiddenCategories.Clear();sidebar.Prefs.HiddenDevices.Clear();
  sidebar.Prefs.Pins.Clear();sidebar.Prefs.PinTargets.Clear();sidebar.Prefs.HiddenMetrics.Clear();
  try
  {
   sidebar.Show();PumpUntil(()=>sidebar.Latest!=null);
   check(sidebar.IsVisible&&sidebar.Latest!=null&&service.Worker!=null,"Actual sidebar starts its worker and renders a snapshot");
   var worker=service.Worker;service.Start();service.Start();
   check(ReferenceEquals(worker,service.Worker),"Repeated start requests retain exactly one sensor worker");
   check(double.IsFinite(sidebar.Left)&&double.IsFinite(sidebar.Top)&&sidebar.ActualWidth>0,"A removed monitor safely falls back to an available screen");
   var tray=typeof(SidebarWindow).GetField("tray",Private)!.GetValue(sidebar)!;
   var hide=typeof(SidebarWindow).GetMethod("HideToTray",Private)!;
   for(int i=0;i<3;i++){hide.Invoke(sidebar,null);Pump();check(!sidebar.IsVisible,"Hide-to-tray hides the sidebar: "+i);sidebar.Restore();Pump();check(sidebar.IsVisible&&ReferenceEquals(worker,service.Worker),"Restore retains the original worker: "+i);}
   sidebar.OpenSettings();Pump();var first=(SettingsWindow)typeof(SidebarWindow).GetField("settings",Private)!.GetValue(sidebar)!;
   sidebar.OpenSettings();Pump();check(ReferenceEquals(first,typeof(SidebarWindow).GetField("settings",Private)!.GetValue(sidebar)),"Opening settings twice reuses the existing settings window");
   first.Close();sidebar.OpenSettings();Pump();var second=(SettingsWindow)typeof(SidebarWindow).GetField("settings",Private)!.GetValue(sidebar)!;
   check(!ReferenceEquals(first,second)&&second.IsVisible,"Settings can be reopened after closing");
   var navigate=typeof(SettingsWindow).GetMethod("Navigate",Private)!;
   navigate.Invoke(second,new object[]{"Grenzen"});Pump();
   var page=(Panel)typeof(SettingsWindow).GetField("page",Private)!.GetValue(second)!;
   var numbers=Logical(page).OfType<Wpf.Ui.Controls.NumberBox>().ToArray();numbers[0].Value=90;numbers[1].Value=95;
   check(sidebar.Prefs.TempLimit==90&&sidebar.Prefs.LoadLimit==95&&Preferences.Load().TempLimit==90,"Actual warning limit controls apply and persist");
   var move=typeof(SettingsWindow).GetMethod("Move",Private)!;sidebar.Prefs.Order=new(){"CPU","GPU","RAM","Drives","Network"};move.Invoke(second,new object[]{"GPU",-1});
   check(sidebar.Prefs.Order[0]=="GPU"&&Preferences.Load().Order[0]=="GPU","Card order buttons apply and persist their new order");
   move.Invoke(second,new object[]{"GPU",-1});check(sidebar.Prefs.Order[0]=="GPU","Moving the first card upwards preserves the order");
   sidebar.Prefs.HiddenCategories=sidebar.Latest!.Devices.Select(d=>d.Category).Distinct().ToList();sidebar.Apply();
   var viewport=(CardViewport)typeof(SidebarWindow).GetField("scroll",Private)!.GetValue(sidebar)!;
   check(Logical(viewport.Cards).OfType<TextBlock>().Any(t=>t.Text.StartsWith("No cards enabled.")),"Disabling every card displays the empty-selection hint");
   sidebar.Prefs.HiddenCategories.Clear();sidebar.Apply();
   check(!Logical(viewport.Cards).OfType<TextBlock>().Any(t=>t.Text.StartsWith("No cards enabled.")),"Re-enabling cards removes the empty-selection hint");
  }
  finally{sidebar.Close();Pump();}
  check(service.Worker!.Wait(3000)&&service.Worker.IsCompletedSuccessfully&&backend.Closed==1,"Closing the sidebar stops the worker and releases its backend once");
  check(backend.Opened==1,"Hide, restore and settings do not create duplicate sensor sessions");
 }
 static IEnumerable<DependencyObject> Logical(DependencyObject root){yield return root;foreach(var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())foreach(var next in Logical(child))yield return next;}
 static void PumpUntil(Func<bool> condition){var timer=System.Diagnostics.Stopwatch.StartNew();while(!condition()&&timer.ElapsedMilliseconds<5000){Pump();Thread.Sleep(10);}}
 static void Pump(){var frame=new DispatcherFrame();Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,new Action(()=>frame.Continue=false));Dispatcher.PushFrame(frame);}
 sealed class LifecycleBackend:ISensorBackend
 {
  public int Opened,Closed;
  public void Open()=>Interlocked.Increment(ref Opened);
  public void Dispose()=>Interlocked.Increment(ref Closed);
  public HardwareSample[] Capture(List<string> warnings)=>new[]{new HardwareSample("/cpu/fixture","Lifecycle CPU","Cpu",new[]{new SensorSample("/cpu/fixture/load","CPU Total",SensorType.Load,10),new SensorSample("/cpu/fixture/temp","CPU Package",SensorType.Temperature,50)},"Lifecycle fixture")};
 }
}
