using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;

static class VioletReferenceQA
{
 public static void Verify(Action<bool,string> check,string folder)
 {
  var cpu=new Device("demo/cpu","CPU","AMD Ryzen 7 9800X3D · Demo",new(){new("load","Auslastung",8,"%"),new("temp","Temperatur",38,"°C"),new Metric("clock","Takt Ø",5425,"MHz",6000){IsCpuClock=true},new("fan","Lüfter",1427,"RPM",3000)});
  var gpu=new Device("demo/gpu","GPU","NVIDIA GeForce · Demo hardware",new(){new("load","Auslastung",26,"%"),new("temp","Temperatur",46,"°C"),new("clock","Takt",1320,"MHz",3000),new("power","Leistung",74,"W",300)});
  var ram=new Device("demo/ram","RAM","64 GB · Physical memory · Demo",new(){new("load","Auslastung",24,"%"),new("used","Belegt",15.6,"GB",64),new("free","Verfügbar",48,"GB",64)});
  var disk=new Device("demo/disk","Drives","NVMe SSD · Demo hardware",new(){new("temp","Temperatur",32,"°C"),new("used","Belegt",31,"%"),new("read","Lesen",22,"MB/s",1000),new("write","Schreiben",8,"MB/s",1000)});
  var network=new Device("demo/net","Network","Ethernet · Demo hardware",new(){new("rx","Download",4,"MB/s",125),new("tx","Upload",2,"MB/s",125),new("link","Adapter-Link",10,"Gbit/s",10),new Metric("ip/internal-ip","Interne IP",null,"IP"){TextValue="192.0.2.10"},new Metric("ip/external-ip","Externe IP",null,"IP"){TextValue="198.51.100.24"}});
  var board=new Device("demo/board","Mainboard","Mainboard · Demo hardware",new(){new("temp","Temperatur",29,"°C"),new("fan","Lüfter",800,"RPM",3000),new("volt","Spannung",1.21,"V",2)});
  var devices=new[]{cpu,ram,gpu,disk,network,board};
  foreach(var d in devices)foreach(double width in new[]{200d,290d,440d})foreach(double scale in new[]{.8,1.4})foreach(bool below in new[]{false,true})
  {
   var prefs=new Preferences{Layout="VioletMonitor",ColumnWidth=width,FontScale=scale,ExpandMultiColumnReadings=below,Columns=2};var history=new MetricHistory();var card=CardLayouts.Create(d,prefs,history);card.Update(d,prefs);var host=new Border{Width=width,Child=card.View};Arrange(host,width);
   check(card is VioletMonitorCard,"Reference design is available through the existing layout selector");
   var text=Walk(card.View).OfType<TextBlock>().ToArray();foreach(var m in d.Metrics)check(text.Any(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Text==m.Display(false)),"Reference preserves each actual selected sensor: "+d.Category+"/"+m.Id);
   foreach(var value in text.Where(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Visibility==Visibility.Visible)){var bounds=value.TransformToAncestor(host).TransformBounds(new Rect(new Point(),value.RenderSize));check(bounds.Left>=-.6&&bounds.Right<=width+.6,"Reference value fits its sidebar: "+value.Text);}
   check(Walk(card.View).OfType<VioletMonitorCard.VioletRing>().Count()==(d.Category is "CPU" or "GPU"?1:0),"Reference rings are reserved for CPU and GPU");
   check(((Border)card.View).Background is SolidColorBrush surface&&surface.Color==Colors.Black,"Reference uses the requested black surface");
   var plots=Walk(card.View).OfType<HistoryAreaGraph>().ToArray();check(plots.Length==(d.Category=="Network"?2:0)&&plots.All(g=>g.SampleCount==0),"Reference traces start empty and are only DL/UL");
   var bars=Walk(card.View).OfType<VioletMonitorCard.VioletBar>().ToArray();check(bars.Length==(d.Category=="Network"?0:d.Category=="RAM"?1:d.Metrics.Count(m=>m.Unit!="IP")),"No invented meters for text/IP fields");
   var now=new DateTime(2026,10,31,12,0,0);history.Record(new[]{d},now);history.Record(new[]{d},now.AddSeconds(1));card.Update(d,prefs);check(plots.All(g=>g.SampleCount==2),"Network traces consume real shared samples");
   card.Update(d with{Metrics=d.Metrics.Select(m=>m with{Value=null,TextValue=null}).ToList()},prefs);check(text.Where(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings).All(t=>t.Text=="—")&&bars.All(b=>b.Ratio==null),"Missing data clears reference values and colored bars");
   if(d.Category is "CPU" or "GPU")check(text.Single(t=>System.Windows.Automation.AutomationProperties.GetAutomationId(t)=="PrimaryMetric").Text=="—","Unavailable primary value is not shown as zero");
   var transparent=CardLayouts.Create(d,new(){Layout="VioletMonitor",Opacity=0});check(transparent.View.Opacity==1&&transparent.View is Border {Background.Opacity:0},"Reference opacity only changes the black surface");
  }
  foreach(double width in new[]{200d,440d})
  {
   double previous=0;foreach(double size in new[]{.5,1d,1.6}){var p=new Preferences{Layout="VioletMonitor",ColumnWidth=width,GraphScale=size};var c=CardLayouts.Create(cpu,p);var h=new Border{Width=width,Child=c.View};Arrange(h,width);var bars=Walk(c.View).OfType<VioletMonitorCard.VioletBar>().ToArray();check(bars.Length==cpu.Metrics.Count&&bars.All(b=>b.ActualHeight>previous),"Reference square bars respond to the graph thickness slider");previous=bars[0].ActualHeight;}
  }
  var pref=new Preferences{Layout="VioletMonitor",ColumnWidth=320,Theme="OLED"};var shared=new MetricHistory();var time=new DateTime(2026,10,31,12,0,0);
  for(int i=0;i<60;i++)shared.Record(devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=m.Value.HasValue?Math.Max(0,m.Value.Value*(d.Category=="Network"?(i%19<3?.3+(i%19)*.3:.015):.7+.3*Math.Sin(i*.23)*Math.Sin(i*.23))):null}).ToList()}),time.AddSeconds(i));
  var gallery=new StackPanel{Width=340,Background=Brushes.Black,Margin=new Thickness(0)};gallery.Children.Add(new TextBlock{Text="VIOLET MONITOR · SIDEBAR",Foreground=Brushes.White,FontSize=14,Margin=new Thickness(10,12,0,3)});gallery.Children.Add(new TextBlock{Text="DEMO HARDWARE · SIMULATED READINGS",Foreground=Ui.Brush("#999999"),FontSize=8,Margin=new Thickness(10,0,0,10)});
  foreach(var d in devices){var card=CardLayouts.Create(d,pref,shared);card.Update(d,pref);gallery.Children.Add(card.View);}
  Arrange(gallery,340);Directory.CreateDirectory(folder);Save(gallery,Path.Combine(folder,"Violet-Monitor-Sidebar.png"));
 }
 static void Arrange(FrameworkElement view,double width){view.Measure(new Size(width,double.PositiveInfinity));view.Arrange(new Rect(0,0,width,view.DesiredSize.Height));view.UpdateLayout();}
 static IEnumerable<DependencyObject> Walk(DependencyObject root){var seen=new HashSet<DependencyObject>();IEnumerable<DependencyObject> Visit(DependencyObject n){if(!seen.Add(n))yield break;yield return n;foreach(var c in LogicalTreeHelper.GetChildren(n).OfType<DependencyObject>())foreach(var x in Visit(c))yield return x;if(n is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(n);i++)foreach(var x in Visit(VisualTreeHelper.GetChild(n,i)))yield return x;}return Visit(root);}
 static void Save(FrameworkElement view,string path){var b=new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth),(int)Math.Ceiling(view.ActualHeight),96,96,PixelFormats.Pbgra32);b.Render(view);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(b));using var file=File.Create(path);encoder.Save(file);}
}
