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
  check(ReleaseIdentity.Version=="3.01.3.0"&&typeof(Aurora.App).Assembly.GetName().Version!.ToString()=="3.1.3.0","Published display identity preserves3.00.0.0 while CLR version is numeric");
  var (devices,hist)=Demo();Directory.CreateDirectory(folder);
  foreach(string layout in new[]{"VioletMonitor","VioletTwin","FluentGlass","LiquidGlass","FrostGlass","AquaGlass","PrismGlass","SmokeGlass","GlassTubes"})foreach(int columns in new[]{1,2})
  {
   double width=columns==1?320:640,height=columns==1?(layout=="VioletTwin"||GlassStyle.All.Any(g=>g.Key==layout)?1800:1120):(GlassStyle.All.Any(g=>g.Key==layout)?1100:900);var p=new Preferences{Layout=layout,Theme="OLED",ColumnWidth=320,Columns=columns,FontScale=1.05};var viewport=new CardViewport();Panel grid=columns==1?new StackPanel():new HardwareCardGrid{Columns=columns};viewport.Cards.Children.Add(grid);foreach(var d in devices){var c=CardLayouts.Create(d,p,hist);c.Update(d,p);grid.Children.Add(c.View);}viewport.Configure(true,width,height,columns>1);
   var root=new Grid{Width=width,Height=height+54,Background=Brushes.Black};root.RowDefinitions.Add(new(){Height=new GridLength(54)});root.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});var header=new StackPanel{Margin=new Thickness(9,7,9,0)};header.Children.Add(new TextBlock{Text="Sidebar by NemeSIS "+ReleaseIdentity.Version,FontSize=14,Foreground=Brushes.White});header.Children.Add(new TextBlock{Text=$"{CardLayouts.Get(layout).Name} · {columns} column(s) · DEMO DATA",FontSize=10,Foreground=Ui.Brush("#B3B3B3"),Margin=new Thickness(0,4,0,0)});root.Children.Add(header);Grid.SetRow(viewport,1);root.Children.Add(viewport);root.Measure(new Size(width,height+54));root.Arrange(new Rect(0,0,width,height+54));root.UpdateLayout();
   check(viewport.ActualWidth==width&&double.IsFinite(viewport.Cards.ActualHeight),"Release preview measures actual viewport: "+layout+"/"+columns);
   var bounds=viewport.Cards.TransformToAncestor(viewport).TransformBounds(new Rect(new Point(),viewport.Cards.RenderSize));check(bounds.Right<=width+.7&&bounds.Bottom<=height+.7,"One/two column preview fits through actual viewport: "+layout+"/"+columns);
   var image=new RenderTargetBitmap((int)width,(int)height+54,96,96,PixelFormats.Pbgra32);image.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var f=File.Create(Path.Combine(folder,$"{layout}-{columns}Column.png"));encoder.Save(f);
  }
 }
 // Demo hardware with a simulated 60-second history, shared by every preview image.
 static (List<Device> Devices,MetricHistory History) Demo()
 {
  var devices=new List<Device>{
   new("demo/cpu","CPU","AMD Ryzen 7 9800X3D · Demo",new(){new("load","Auslastung",24,"%"),new("temp","Temperatur",48,"°C"),new Metric("clock","Takt Ø",4800,"MHz",6000){IsCpuClock=true},new("power","Leistung",78,"W",200),new("volt","Spannung",.96,"V",2)}),
   new("demo/ram","RAM","32 GB · Physical memory · Demo",new(){new("load","Auslastung",43,"%"),new("used","Belegt",13,"GB",32),new("free","Verfügbar",18,"GB",32),new("total","Gesamt",32,"GB",32)}),
   new("demo/gpu","GPU","NVIDIA GeForce RTX · Demo",new(){new("load","Auslastung",36,"%"),new("temp","Temperatur",46,"°C"),new("clock","Takt",1320,"MHz",3000),new("power","Leistung",74,"W",300),new("fan","Lüfter",1100,"RPM",4000)}),
   new("demo/ssd1","Drives","NVMe SSD 2 TB · Demo",new(){new("temp","Temperatur",39,"°C"),new("used","Belegt",31,"%"),new("read","Lesen",22,"MB/s",1000),new("write","Schreiben",8,"MB/s",1000)}),
   new("demo/ssd2","Drives","SATA SSD 4 TB · Demo",new(){new("temp","Temperatur",33,"°C"),new("used","Belegt",57,"%"),new("read","Lesen",8,"MB/s",550),new("write","Schreiben",0,"MB/s",550)}),
   new("demo/net","Network","Ethernet · Demo hardware",new(){new("rx","Download",12,"MB/s",125),new("tx","Upload",2,"MB/s",125),new("link","Adapter-Link",10,"Gbit/s",10),new Metric("ip/internal-ip","Interne IP",null,"IP"){TextValue="192.0.2.10"},new Metric("ip/external-ip","Externe IP",null,"IP"){TextValue="198.51.100.24"}})
  };
  var hist=new MetricHistory();var now=new DateTime(2026,10,31,12,0,0);
  for(int i=0;i<60;i++)hist.Record(devices.Select(d=>d with{Metrics=d.Metrics.Select(m=>m with{Value=m.Value.HasValue?Math.Max(0,m.Value.Value*(.72+.28*Math.Sin(i*.27)*Math.Sin(i*.27))):null}).ToList()}),now.AddSeconds(i));
  return (devices,hist);
 }
 // One thumbnail per selectable layout (CPU, GPU, network) and one Liquid Glass CPU card per
 // color theme, for the README gallery. Same production renderer and demo data as the previews.
 public static void Gallery(Action<bool,string> check,string folder)
 {
  var (all,hist)=Demo();var devices=all.Where(d=>d.Category is "CPU" or "GPU" or "Network").ToList();
  string layouts=Path.Combine(folder,"Gallery"),themes=Path.Combine(folder,"Themes");Directory.CreateDirectory(layouts);Directory.CreateDirectory(themes);
  void Save(FrameworkElement root,double width,string file)
  {
   root.Measure(new Size(width,double.PositiveInfinity));root.Arrange(new Rect(0,0,width,root.DesiredSize.Height));root.UpdateLayout();
   int height=(int)Math.Ceiling(root.ActualHeight);check(height>0&&height<4000,"Gallery image has a finite height: "+file);
   var image=new RenderTargetBitmap((int)width,height,96,96,PixelFormats.Pbgra32);image.Render(root);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var f=File.Create(file);png.Save(f);
  }
  foreach(var layout in CardLayouts.All)
  {
   var p=new Preferences{Layout=layout.Key,Theme="OLED",ColumnWidth=300,Columns=1};var stack=new StackPanel{Width=300,Background=Brushes.Black};
   stack.Children.Add(new TextBlock{Text=layout.Name,FontSize=13,FontWeight=FontWeights.SemiBold,Foreground=Brushes.White,Margin=new Thickness(9,8,9,6)});
   foreach(var d in devices){var c=CardLayouts.Create(d,p,hist);c.Update(d,p);stack.Children.Add(c.View);}
   Save(stack,300,Path.Combine(layouts,layout.Key+".png"));
  }
  var cpu=all.First(d=>d.Category=="CPU");
  foreach(var design in DesignCatalog.All)
  {
   var p=new Preferences{Layout="LiquidGlass",Theme=design.Key,ColumnWidth=260,Columns=1};var c=CardLayouts.Create(cpu,p,hist);c.Update(cpu,p);
   var stack=new StackPanel{Width=260,Background=Ui.Brush(design.Back)};stack.Children.Add(new TextBlock{Text=design.Name,FontSize=12,FontWeight=FontWeights.SemiBold,Foreground=Ui.Brush(design.Text),Margin=new Thickness(9,8,9,6)});stack.Children.Add(c.View);
   Save(stack,260,Path.Combine(themes,design.Key+".png"));
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
