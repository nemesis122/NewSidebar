using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;

static class DesignReadabilityQA
{
 public static void Verify(Action<bool,string> check,string output,string suffix)
 {
  Directory.CreateDirectory(output);
  var overflow=new List<string>();
  var device=new Device("readability/cpu","CPU","Demo CPU · simulated readings",new(){new("load","Auslastung",17,"%"),new("temp","Temperatur (Package)",48,"°C"),new Metric("clock","Takt Ø",4111,"MHz",6000){IsCpuClock=true},new("power","CPU Package Power",27,"W"),new("volt","Core Voltage",.96,"V"),new("fan","CPU Cooling Fan",1234,"RPM")});
  foreach(double width in new[]{220d,300d})
  {
   var gallery=new Grid{Width=width*4,Background=Ui.Brush("#101218")};for(int i=0;i<4;i++)gallery.ColumnDefinitions.Add(new(){Width=new GridLength(width)});
   int column=0;
   foreach(var key in new[]{"Original","Nano","TwinRings","PerformanceHUD"})
   {
    var p=new Preferences{Layout=key,Theme="Graphite",ColumnWidth=width};var card=CardLayouts.Create(device,p);var panel=new StackPanel{Margin=new Thickness(5,12,5,12)};
    panel.Children.Add(new TextBlock{Text=CardLayouts.Get(key).Name,Foreground=Brushes.White,FontSize=15,Margin=new Thickness(3,0,0,8)});panel.Children.Add(card.View);
    Grid.SetColumn(panel,column++);gallery.Children.Add(panel);
    card.Update(device,p);
   }
   Arrange(gallery,width*4);
   Save(gallery,Path.Combine(output,$"Designs-{(int)width}-{suffix}.png"));
  }
  foreach(var layout in CardLayouts.All)foreach(double width in new[]{200d,300d})
  {
   var p=new Preferences{Layout=layout.Key,Theme="Graphite",ColumnWidth=width};var card=CardLayouts.Create(device,p);card.Update(device,p);var host=new Border{Width=width,Child=card.View};Arrange(host,width);
   check(double.IsFinite(host.DesiredSize.Height)&&host.DesiredSize.Height>0,"Readability layout retains finite dimensions: "+layout.Key+" / "+width);
   foreach(var text in Walk(card.View).OfType<TextBlock>().Where(t=>t.Visibility==Visibility.Visible&&SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Text!="—"))
   {
    var bounds=text.TransformToAncestor(host).TransformBounds(new Rect(new Point(),text.RenderSize));
    if(bounds.Left<-.6||bounds.Right>width+.6)overflow.Add(layout.Key+" / "+width+" / "+text.Text+" / "+bounds);
   }
  }
  File.WriteAllLines(Path.Combine(output,"Layout-Grenzen-"+suffix+".txt"),overflow);
  if(suffix!="Vorher")check(overflow.Count==0,"Readings remain within their cards across all layouts: "+string.Join("; ",overflow.Take(5)));
 }
 static IEnumerable<DependencyObject> Walk(DependencyObject root){var seen=new HashSet<DependencyObject>();IEnumerable<DependencyObject> Visit(DependencyObject node){if(!seen.Add(node))yield break;yield return node;foreach(var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>())foreach(var item in Visit(child))yield return item;if(node is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)foreach(var item in Visit(VisualTreeHelper.GetChild(node,i)))yield return item;}return Visit(root);}
 static void Arrange(FrameworkElement view,double width){view.Measure(new Size(width,double.PositiveInfinity));view.Arrange(new Rect(0,0,width,view.DesiredSize.Height));view.UpdateLayout();}
 static void Save(FrameworkElement view,string path){var bitmap=new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth),(int)Math.Ceiling(view.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(view);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var file=File.Create(path);encoder.Save(file);}
}
