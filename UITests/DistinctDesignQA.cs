using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;

static class DistinctDesignQA
{
 public static readonly string[] Keys={"RingMatrix","CapsuleDock","EqualizerDeck"};
 public static void Verify(Action<bool,string> check,string output)
 {
  Directory.CreateDirectory(output);
  var cpu=new Device("demo/cpu","CPU","AMD Ryzen 9 · Demo hardware",new(){new("load","Auslastung",24,"%"),new("temp","Temperatur",52,"°C"),new Metric("clock","Takt Ø",4111,"MHz",6000){IsCpuClock=true},new("power","Leistung",78,"W",200),new("volt","Spannung",.96,"V",2)});
  var gpu=new Device("demo/gpu","GPU","NVIDIA GeForce · Demo hardware",new(){new("load","Auslastung",36,"%"),new("temp","Temperatur",46,"°C"),new("clock","Takt",420,"MHz",3000),new("power","Leistung",41,"W",300),new("fan","Lüfter",1050,"RPM",4000)});
  var ram=new Device("demo/ram","RAM","32 GB · Physical memory",new(){new("load","Auslastung",43,"%"),new("used","Belegt",13.8,"GB",32),new("free","Verfügbar",18.2,"GB",32),new("total","Gesamt",32,"GB",32)});
  var disk=new Device("demo/disk","Drives","NVMe SSD · Demo hardware",new(){new("temp","Temperatur",44,"°C"),new("used","Belegt",31,"%"),new("read","Lesen",22,"MB/s",1000),new("write","Schreiben",8,"MB/s",1000)});
  var network=new Device("demo/net","Network","Ethernet · Demo hardware",new(){new("rx","Download",12,"MB/s",125),new("tx","Upload",2,"MB/s",125),new("link","Adapter-Link",1,"Gbit/s",1),new Metric("ip/internal-ip","Interne IP",null,"IP"){TextValue="192.0.2.10"}});
  var devices=new[]{cpu,ram,gpu,disk,network};
  foreach(string key in Keys.Where(key=>CardLayouts.All.Any(l=>l.Key==key)))foreach(var device in devices)foreach(double width in new[]{200d,300d})foreach(bool below in new[]{false,true})
  {
   var p=new Preferences{Layout=key,Theme="OLED",ColumnWidth=width,ExpandMultiColumnReadings=below};var history=new MetricHistory();var card=CardLayouts.Create(device,p,history);card.Update(device,p);var host=Host(card.View,width);Arrange(host,width);
   check(card is DistinctHardwareCard modern&&modern.Composition==key,"Distinct layout is selectable: "+key);
   var text=Walk(card.View).OfType<TextBlock>().ToArray();foreach(var metric in device.Metrics)check(text.Any(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Text==metric.Display(false)),"Every selected reading is retained: "+key+" / "+metric.Id);
   check(text.All(t=>t.Visibility!=Visibility.Visible||!ToolTipService.GetIsEnabled(t)),"Card tooltips remain disabled: "+key);
   foreach(var value in text.Where(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Visibility==Visibility.Visible)){var rect=value.TransformToAncestor(host).TransformBounds(new Rect(new Point(),value.RenderSize));check(rect.Left>=-.6&&rect.Right<=width+.6,"Distinct reading remains within the widget: "+key+" / "+value.Text);}
   check(double.IsFinite(host.DesiredSize.Height)&&host.DesiredSize.Height<400,"Compact widgets have finite dense natural heights: "+key);
   var graphs=Walk(card.View).OfType<HistoryAreaGraph>().ToArray();check(graphs.Length==(key=="FlowCompact"?1:0)&&graphs.All(g=>g.SampleCount==0),"Only Flow Compact has history, starting empty: "+key);
   var time=new DateTime(2026,10,30,12,0,0);history.Record(new[]{device},time);history.Record(new[]{device},time.AddSeconds(1));card.Update(device,p);check(graphs.All(g=>g.SampleCount==2),"Real shared samples feed the compact history: "+key);
   var missing=device with{Metrics=device.Metrics.Select(m=>m with{Value=null,TextValue=null}).ToList()};card.Update(missing,p);check(text.Where(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings).All(t=>t.Text=="—"),"Missing compact readings clear instead of freezing: "+key);
   card.Update(device,p);
  }
  var gallery=new Grid{Width=840,Background=Ui.Brush("#0B0E14")};for(int i=0;i<3;i++)gallery.ColumnDefinitions.Add(new(){Width=new GridLength(280)});
  var activeKeys=Keys.Where(key=>CardLayouts.All.Any(l=>l.Key==key)).ToArray();
  for(int column=0;column<activeKeys.Length;column++)
  {
   string key=activeKeys[column];var p=new Preferences{Layout=key,Theme="OLED",ColumnWidth=260};var history=new MetricHistory();var now=new DateTime(2026,10,30,12,0,0);
   for(int i=0;i<60;i++)history.Record(devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=m.Value.HasValue?Math.Max(0,m.Value.Value*(.8+.2*Math.Sin(i*.37))):null}).ToList()}),now.AddSeconds(i));
   var stack=new StackPanel{Margin=new Thickness(8,14,8,14)};Grid.SetColumn(stack,column);gallery.Children.Add(stack);stack.Children.Add(new TextBlock{Text=CardLayouts.Get(key).Name,FontSize=17,FontWeight=FontWeights.SemiBold,Foreground=Brushes.White,Margin=new Thickness(3,0,0,4)});stack.Children.Add(new TextBlock{Text="DEMO HARDWARE · SIMULATED READINGS",FontSize=8,Foreground=Ui.Brush("#8993A7"),Margin=new Thickness(3,0,0,12)});
   foreach(var device in devices){var card=CardLayouts.Create(device,p,history);card.Update(device,p);stack.Children.Add(card.View);}
  }
  Arrange(gallery,840);Save(gallery,Path.Combine(output,"Distinct-Designs.png"));
 }
 static FrameworkElement Host(FrameworkElement view,double width)=>new Border{Width=width,Child=view};
 static void Arrange(FrameworkElement view,double width){view.Measure(new Size(width,double.PositiveInfinity));view.Arrange(new Rect(0,0,width,view.DesiredSize.Height));view.UpdateLayout();}
 static IEnumerable<DependencyObject> Walk(DependencyObject root){var seen=new HashSet<DependencyObject>();IEnumerable<DependencyObject> Visit(DependencyObject n){if(!seen.Add(n))yield break;yield return n;foreach(var c in LogicalTreeHelper.GetChildren(n).OfType<DependencyObject>())foreach(var x in Visit(c))yield return x;if(n is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(n);i++)foreach(var x in Visit(VisualTreeHelper.GetChild(n,i)))yield return x;}return Visit(root);}
 static void Save(FrameworkElement view,string path){var b=new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth),(int)Math.Ceiling(view.ActualHeight),96,96,PixelFormats.Pbgra32);b.Render(view);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(b));using var file=File.Create(path);encoder.Save(file);}
}
