using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;
static class ReleasePreviewQA
{
 public static void Verify(Action<bool,string> check,string folder)
 {
  check(ReleaseIdentity.Version=="3.01.1.0"&&typeof(Aurora.App).Assembly.GetName().Version!.ToString()=="3.1.1.0","Published display identity preserves3.00.0.0 while CLR version is numeric");
  var devices=new List<Device>{
   new("demo/cpu","CPU","AMD Ryzen 7 9800X3D · Demo",new(){new("load","Auslastung",24,"%"),new("temp","Temperatur",48,"°C"),new Metric("clock","Takt Ø",4800,"MHz",6000){IsCpuClock=true},new("power","Leistung",78,"W",200),new("volt","Spannung",.96,"V",2)}),
   new("demo/ram","RAM","32 GB · Physical memory · Demo",new(){new("load","Auslastung",43,"%"),new("used","Belegt",13,"GB",32),new("free","Verfügbar",18,"GB",32),new("total","Gesamt",32,"GB",32)}),
   new("demo/gpu","GPU","NVIDIA GeForce RTX · Demo",new(){new("load","Auslastung",36,"%"),new("temp","Temperatur",46,"°C"),new("clock","Takt",1320,"MHz",3000),new("power","Leistung",74,"W",300),new("fan","Lüfter",1100,"RPM",4000)}),
   new("demo/ssd1","Drives","NVMe SSD 2 TB · Demo",new(){new("temp","Temperatur",39,"°C"),new("used","Belegt",31,"%"),new("read","Lesen",22,"MB/s",1000),new("write","Schreiben",8,"MB/s",1000)}),
   new("demo/ssd2","Drives","SATA SSD 4 TB · Demo",new(){new("temp","Temperatur",33,"°C"),new("used","Belegt",57,"%"),new("read","Lesen",8,"MB/s",550),new("write","Schreiben",0,"MB/s",550)}),
   new("demo/net","Network","Ethernet · Demo hardware",new(){new("rx","Download",12,"MB/s",125),new("tx","Upload",2,"MB/s",125),new("link","Adapter-Link",10,"Gbit/s",10),new Metric("ip/internal-ip","Interne IP",null,"IP"){TextValue="192.0.2.10"},new Metric("ip/external-ip","Externe IP",null,"IP"){TextValue="198.51.100.24"}})
  };
  Directory.CreateDirectory(folder);var hist=new MetricHistory();var now=new DateTime(2026,10,31,12,0,0);
  for(int i=0;i<60;i++)hist.Record(devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=m.Value.HasValue?Math.Max(0,m.Value.Value*(.72+.28*Math.Sin(i*.27)*Math.Sin(i*.27))):null}).ToList()}),now.AddSeconds(i));
  foreach(string layout in new[]{"VioletMonitor","VioletTwin","FluentGlass"})foreach(int columns in new[]{1,2})
  {
   double width=columns==1?320:640,height=columns==1?(layout=="VioletTwin"?1800:1120):900;var p=new Preferences{Layout=layout,Theme="OLED",ColumnWidth=320,Columns=columns,FontScale=1.05};var viewport=new CardViewport();Panel grid=columns==1?new StackPanel():new HardwareCardGrid{Columns=columns};viewport.Cards.Children.Add(grid);foreach(var d in devices){var c=CardLayouts.Create(d,p,hist);c.Update(d,p);grid.Children.Add(c.View);}viewport.Configure(true,width,height,columns>1);
   var root=new Grid{Width=width,Height=height+54,Background=Brushes.Black};root.RowDefinitions.Add(new(){Height=new GridLength(54)});root.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});var header=new StackPanel{Margin=new Thickness(9,7,9,0)};header.Children.Add(new TextBlock{Text="Sidebar by NemeSIS 3.00.0.0",FontSize=14,Foreground=Brushes.White});header.Children.Add(new TextBlock{Text=$"{CardLayouts.Get(layout).Name} · {columns} column(s) · DEMO DATA",FontSize=10,Foreground=Ui.Brush("#B3B3B3"),Margin=new Thickness(0,4,0,0)});root.Children.Add(header);Grid.SetRow(viewport,1);root.Children.Add(viewport);root.Measure(new Size(width,height+54));root.Arrange(new Rect(0,0,width,height+54));root.UpdateLayout();
   check(viewport.ActualWidth==width&&double.IsFinite(viewport.Cards.ActualHeight),"Release preview measures actual viewport: "+layout+"/"+columns);
   var bounds=viewport.Cards.TransformToAncestor(viewport).TransformBounds(new Rect(new Point(),viewport.Cards.RenderSize));check(bounds.Right<=width+.7&&bounds.Bottom<=height+.7,"One/two column preview fits through actual viewport: "+layout+"/"+columns);
   var image=new RenderTargetBitmap((int)width,(int)height+54,96,96,PixelFormats.Pbgra32);image.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var f=File.Create(Path.Combine(folder,$"{layout}-{columns}Column.png"));encoder.Save(f);
  }
 }
 public static void Settings(Action<bool,string> check,string folder)
 {
  var sidebar=new SidebarWindow();sidebar.Prefs.SettingsLanguage="en";sidebar.Prefs.Layout="VioletMonitor";sidebar.Prefs.Theme="OLED";sidebar.Prefs.HiddenCategories.Clear();sidebar.Prefs.HiddenDevices.Clear();sidebar.Prefs.Pins.Clear();sidebar.Prefs.HiddenMetrics.Clear();sidebar.Prefs.Columns=2;
  var device=new Device("release/demo/cpu","CPU","AMD Ryzen 7 9800X3D · Demo hardware",new(){new("temp","Temperatur",48,"°C"),new("load","Auslastung",24,"%"),new Metric("clock","Takt Ø",4800,"MHz",6000){IsCpuClock=true},new("power","Leistung",78,"W",200)});
  typeof(SidebarWindow).GetProperty("Latest")!.SetValue(sidebar,new Snapshot(new(){device},new(),new(),DateTime.Now));sidebar.Apply();
  var window=new SettingsWindow(sidebar){Opacity=0,Width=1020,Height=760};
  try
  {
   window.Show();void Pump(){var f=new System.Windows.Threading.DispatcherFrame();System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,new Action(()=>f.Continue=false));System.Windows.Threading.Dispatcher.PushFrame(f);}Pump();
   foreach(var page in new[]{("Darstellung","Settings-Appearance.png"),("Textgrößen","Settings-TextSizes.png")})
   {
    typeof(SettingsWindow).GetMethod("SelectPage",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(window,new object[]{page.Item1});Pump();var root=(FrameworkElement)window.Content;root.Measure(new Size(1020,760));root.Arrange(new Rect(0,0,1020,760));root.UpdateLayout();Pump();
    check(!window.ShowInTaskbar,"Release settings stay out of the taskbar");var image=new RenderTargetBitmap(1020,760,96,96,PixelFormats.Pbgra32);image.Render(root);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(Path.Combine(folder,page.Item2));png.Save(file);
   }
  }
  finally{window.Close();sidebar.Close();SettingsText.CurrentLanguage="de";}
 }

}
