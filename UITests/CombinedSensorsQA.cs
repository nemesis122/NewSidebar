using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Aurora;

static class CombinedSensorsQA
{
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Verify(Action<bool,string> check,string folder)
    {
        var snapshot=new Snapshot(new(){
            new("cpu","CPU","Demo CPU",new(){new("load","Auslastung",23,"%"),new("temp","Temperatur",52,"°C"),new("clock","Takt",4200,"MHz",6000),new("power","Leistung",58,"W",120)}),
            new("gpu","GPU","Demo GPU",new(){new("load","Auslastung",76,"%"),new("temp","Temperatur",56,"°C"),new("power","Leistung",142,"W",200),new("fan","Lüfter",1240,"RPM",4000)}),
            new("ram","RAM","Demo RAM",new(){new("load","Auslastung",43,"%"),new("used","Belegt",14,"GB",32),new("free","Verfügbar",18,"GB",32)}),
            new("net","Network","Demo Ethernet",new(){new("rx","Download",24,"MB/s",125),new("tx","Upload",2,"MB/s",125),new("link","Adapter-Link",1,"Gbit/s",1)})
        },new(),new(),DateTime.Now);
        var history=new MetricHistory();
        foreach(var layout in CardLayouts.All)
        foreach(string theme in new[]{"OLED","Light"})
        foreach(int columns in new[]{1,2})
        {
            var p=new Preferences{CombineSensorCards=true,Layout=layout.Key,Theme=theme,Columns=columns};
            var devices=SensorCardComposition.Visible(snapshot,p);history.Record(devices,snapshot.Time);var group=new HardwareSensorGroup(devices,p,history);
            var host=Host(group,p,330*columns);Arrange(host,330*columns);
            check(Walk(group).OfType<TextBlock>().Any(t=>t.Text=="HW Sensors"),"Every design has one shared HW Sensors header: "+layout.Key);
            check(group.Sections.Count==devices.Count,"Every original hardware section remains in the shared surface: "+layout.Key);
            foreach(var device in devices)
            {
                var original=CardLayouts.Create(device,p,history);original.Update(device,p);var originalHost=Host(original.View,p,330);Arrange(originalHost,330);
                var section=group.Sections[device.Id];
                check(section.GetType()==original.GetType(),"The original card implementation is preserved: "+layout.Key+" / "+device.Category);
                var originalText=Walk(original.View).OfType<TextBlock>().Select(t=>(t.Text,Role:SidebarTextVisibility.GetRole(t),t.FontSize,t.Visibility)).ToArray();
                var groupedText=Walk(section.View).OfType<TextBlock>().Select(t=>(t.Text,Role:SidebarTextVisibility.GetRole(t),t.FontSize,t.Visibility)).ToArray();
                check(originalText.SequenceEqual(groupedText),"Original labels, values, typography and visibility remain unchanged: "+layout.Key+" / "+device.Category);
                check(Walk(original.View).OfType<HistoryAreaGraph>().Count()==Walk(section.View).OfType<HistoryAreaGraph>().Count(),"Each hardware section retains its own history graphs: "+layout.Key+" / "+device.Category);
                check(Walk(original.View).OfType<HistoryAreaGraph>().Select(g=>g.SampleCount).SequenceEqual(Walk(section.View).OfType<HistoryAreaGraph>().Select(g=>g.SampleCount)),"History samples remain assigned to their original device and sensor IDs: "+layout.Key+" / "+device.Category);
                check(section.View is Border{BorderThickness:var thickness}&&thickness==new Thickness(0),"Individual section frames are removed: "+layout.Key);
                check(!Walk(section.View).OfType<Wpf.Ui.Controls.Card>().Any(),"No nested Fluent card frame remains: "+layout.Key);
            }
            check(double.IsFinite(host.DesiredSize.Height)&&host.DesiredSize.Height>0,"The shared surface measures in the existing column layout: "+layout.Key);
            var overloaded=devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=m.Unit=="°C"?99:m.Value}).ToList()}).ToArray();group.Update(overloaded,p);
            check(group.BorderBrush is SolidColorBrush red&&red.Color==Color.FromRgb(244,119,119),"The shared perimeter shows hardware warnings: "+layout.Key);
            var missing=devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=null}).ToList()}).ToArray();group.Update(missing,p);
            check(group.Sections.Values.All(c=>Walk(c.View).OfType<TextBlock>().Any(t=>t.Text=="—")),"Every original section clears unavailable readings: "+layout.Key);
            group.Update(devices,p);
            check(group.Sections.Values.All(c=>((Border)c.View).BorderThickness==new Thickness(0)),"Live updates never recreate separate outer frames: "+layout.Key);
        }
        var gallery=new System.Windows.Controls.Primitives.UniformGrid{Columns=3,Background=Ui.Brush("#10141C")};
        foreach(string layout in new[]{"Original","DashboardPro","CyberPulse"})
        {
            var p=new Preferences{CombineSensorCards=true,Layout=layout,Theme="Graphite"};
            var devices=SensorCardComposition.Visible(snapshot,p).Where(d=>d.Category is "CPU" or "GPU").ToList();var group=new HardwareSensorGroup(devices,p,history);
            var example=new StackPanel();example.Children.Add(new TextBlock{Text=CardLayouts.Get(layout).Name+" · Demo",Foreground=Ui.Brush("#ECF2FA"),Margin=new Thickness(6,4,6,10)});example.Children.Add(group);gallery.Children.Add(Host(example,p,330));
        }
        Arrange(gallery,990);Save(gallery,Path.Combine(folder,"HW-Sensors-Original-Designs.png"));
        VerifyToggle(snapshot,check,folder);
    }
    static void VerifyToggle(Snapshot snapshot,Action<bool,string> check,string folder)
    {
        var sidebar=new SidebarWindow();sidebar.Prefs.CombineSensorCards=false;sidebar.Prefs.SensorSource=SensorSources.Libre;sidebar.Prefs.Layout="Original";sidebar.Prefs.Columns=2;sidebar.Prefs.HiddenDevices.Clear();sidebar.Prefs.HiddenCategories.Clear();sidebar.Prefs.SettingsLanguage="de";sidebar.Prefs.Pins.Clear();sidebar.Prefs.HiddenMetrics.Clear();
        typeof(SidebarWindow).GetProperty("Latest")!.SetValue(sidebar,snapshot);sidebar.Apply();
        var settings=new SettingsWindow(sidebar){Opacity=0};settings.Show();Pump();
        typeof(SettingsWindow).GetMethod("SelectPage",Private)!.Invoke(settings,new object[]{"Karten"});
        var toggle=Walk(settings).OfType<Wpf.Ui.Controls.ToggleSwitch>().Single(t=>System.Windows.Automation.AutomationProperties.GetAutomationId(t)=="CombineSensorCards");
        var views=(System.Collections.IDictionary)typeof(SidebarWindow).GetField("views",Private)!.GetValue(sidebar)!;
        check(views.Count==4,"The original device cards render before grouping");
        toggle.IsChecked=true;toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var scroll=(CardViewport)typeof(SidebarWindow).GetField("scroll",Private)!.GetValue(sidebar)!;
        check(scroll.Cards.Children.Count==1&&scroll.Cards.Children[0] is HardwareSensorGroup,"The actual toggle creates exactly one shared outer tile");
        check(views.Count==4&&snapshot.Devices.All(d=>views.Contains(d.Id)),"The actual renderer preserves all original device IDs and live views");
        check(Preferences.Load().CombineSensorCards,"The shared-frame option persists");
        var root=(FrameworkElement)settings.Content;settings.Width=1000;settings.Height=820;root.Measure(new Size(1000,820));root.Arrange(new Rect(0,0,1000,820));root.UpdateLayout();Pump();Save(root,Path.Combine(folder,"HW-Sensors-Grouped-Settings.png"));
        toggle.IsChecked=false;toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));check(!Walk(scroll.Cards).OfType<HardwareSensorGroup>().Any()&&views.Count==4,"Disabling grouping restores separate original card shells");
        settings.Close();sidebar.Close();Pump();
    }
    static Border Host(FrameworkElement view,Preferences p,double width){var d=DesignCatalog.Resolve(p);return new Border{Child=view,Width=width,Padding=new(6),Background=Ui.Brush(d.Back),Resources=new ResourceDictionary{["Back"]=Ui.Brush(d.Back),["Panel"]=Ui.Brush(d.Panel),["Text"]=Ui.Brush(d.Text),["Muted"]=Ui.Brush(d.Muted),["Edge"]=Ui.Brush(d.Edge),["Accent"]=Ui.Brush(d.Accent)}};}
    static void Arrange(FrameworkElement view,double width){view.Measure(new Size(width,double.PositiveInfinity));view.Arrange(new Rect(0,0,width,view.DesiredSize.Height));view.UpdateLayout();}
    static IEnumerable<DependencyObject> Walk(DependencyObject root){var visited=new HashSet<DependencyObject>();IEnumerable<DependencyObject> Visit(DependencyObject node){if(!visited.Add(node))yield break;yield return node;foreach(var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>())foreach(var item in Visit(child))yield return item;if(node is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)foreach(var item in Visit(VisualTreeHelper.GetChild(node,i)))yield return item;}return Visit(root);}
    static void Pump(){var frame=new DispatcherFrame();Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,new Action(()=>frame.Continue=false));Dispatcher.PushFrame(frame);}
    static void Save(FrameworkElement view,string path){var image=new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth),(int)Math.Ceiling(view.ActualHeight),96,96,PixelFormats.Pbgra32);image.Render(view);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(path);png.Save(file);}
}
