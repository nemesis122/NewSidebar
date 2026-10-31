using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;

static class AddressAlignmentQA
{
 public static void Verify(Action<bool,string> check,string output)
 {
  var basic=new Device("demo/network","Network","Ethernet · demo hardware",new(){new("rx","Download",2,"MB/s",125),new("tx","Upload",0,"MB/s",125),new("link","Adapter-Link",10,"Gbit/s",10)});
  var network=basic with{Metrics=basic.Metrics.Concat(new[]{new Metric("ip/internal-ip","Interne IP",null,"IP"){TextValue="192.0.2.10"},new Metric("ip/external-ip","Externe IP",null,"IP"){TextValue="198.51.100.24"}}).ToList()};
  string[] aligned={"Original","Modern","Compact","Text","Bars","Nano","EdgeCompact","FlowCompact","OrbitMini","PixelLedger","LineDashboard","RangeDeck","ColumnFilm","TicketRail","TypeStack","RulerTracks","BridgeRows","ValueRail"};
  foreach(string key in aligned.Where(key=>CardLayouts.All.Any(l=>l.Key==key)))foreach(double width in new[]{200d,300d,440d})foreach(double scale in new[]{.8,1d,1.4})foreach(double graphic in key=="PixelLedger"?new[]{.5,1d,2d}:new[]{1d})
  {
   var p=new Preferences{Layout=key,ColumnWidth=width,Theme="OLED",FontScale=scale,InstrumentScale=graphic};var card=CardLayouts.Create(network,p);card.Update(network,p);var host=new Border{Width=width,Child=card.View};Arrange(host,width);var texts=Walk(card.View).OfType<TextBlock>().ToArray();var dl=texts.Single(t=>t.Text=="DL");var right=texts.Where(t=>SidebarTextVisibility.GetRole(t)==SidebarTextRole.Readings&&t.Text=="0 MB/s").First();var dlBounds=Bounds(dl,host);var rightBounds=Bounds(right,host);
   foreach(var name in new[]{"Int IP","Ext IP"}){var label=texts.Single(t=>t.Text==name);check(Math.Abs(Bounds(label,host).Left-dlBounds.Left)<.7,"IP label aligns with DL label: "+key+" / "+width+" / "+scale+" / "+graphic);}
   foreach(var address in new[]{"192.0.2.10","198.51.100.24"}){var value=texts.Single(t=>t.Text==address);check(Math.Abs(Bounds(value,host).Right-rightBounds.Right)<.7,"IP value shares ordinary reading right edge: "+key);check(value.TextWrapping==TextWrapping.Wrap&&value.TextAlignment==TextAlignment.Right,"Addresses retain right-aligned wrapping: "+key);}
   check(Walk(card.View).OfType<AddressReadoutPanel>().All(row=>!Walk(row).OfType<HistoryAreaGraph>().Any()),"Address rows contain text only, without graphs: "+key);
  }
  foreach(var layout in CardLayouts.All)
  {
   var p=new Preferences{Layout=layout.Key,ColumnWidth=300};var plain=CardLayouts.Create(basic,p);var withIp=CardLayouts.Create(network,p);plain.Update(basic,p);withIp.Update(network,p);
   check(Walk(plain.View).OfType<HistoryAreaGraph>().Count()==Walk(withIp.View).OfType<HistoryAreaGraph>().Count(),"IP addresses do not add history graphs: "+layout.Key);
  }
  foreach(double width in new[]{70d,120d,250d})
  {
   var label=Ui.SidebarText("Interne IP",12);var value=Ui.RoleText(SidebarTextRole.Readings,"2001:db8:1234:5678:9abc:def0:1234:5678",13);var row=new AddressReadoutPanel(label,value);Arrange(row,width);
   check(value.FontSize==13&&value.Text=="2001:db8:1234:5678:9abc:def0:1234:5678","IPv6 is preserved without font shrinking: "+width);check(value.ActualHeight>13&&value.TextTrimming==TextTrimming.None&&Bounds(value,row).Right<=width+.6,"Long IPv6 wraps inside its row: "+width);
  }
  var gallery=new Grid{Width=900,Background=Ui.Brush("#0A0C10")};for(int i=0;i<3;i++)gallery.ColumnDefinitions.Add(new(){Width=new GridLength(300)});
  int column=0;foreach(string key in new[]{"LineDashboard","Nano","Original"}){var p=new Preferences{Layout=key,Theme="OLED",ColumnWidth=280,FontScale=1.15};var card=CardLayouts.Create(network,p);card.Update(network,p);var stack=new StackPanel{Margin=new Thickness(8,12,8,12)};Grid.SetColumn(stack,column++);gallery.Children.Add(stack);stack.Children.Add(new TextBlock{Text=CardLayouts.Get(key).Name+" · demo",Foreground=Brushes.White,FontSize=16,Margin=new Thickness(3,0,0,9)});stack.Children.Add(card.View);}
  Arrange(gallery,900);Directory.CreateDirectory(output);var bitmap=new RenderTargetBitmap(900,(int)Math.Ceiling(gallery.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(gallery);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(output,"IP-Alignment.png"));encoder.Save(file);
 }
 static Rect Bounds(FrameworkElement e,Visual parent)=>e.TransformToAncestor(parent).TransformBounds(new Rect(new Point(),e.RenderSize));
 static void Arrange(FrameworkElement view,double width){view.Measure(new Size(width,double.PositiveInfinity));view.Arrange(new Rect(0,0,width,view.DesiredSize.Height));view.UpdateLayout();}
 static IEnumerable<DependencyObject> Walk(DependencyObject root){var seen=new HashSet<DependencyObject>();IEnumerable<DependencyObject> Visit(DependencyObject n){if(!seen.Add(n))yield break;yield return n;foreach(var c in LogicalTreeHelper.GetChildren(n).OfType<DependencyObject>())foreach(var x in Visit(c))yield return x;if(n is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(n);i++)foreach(var x in Visit(VisualTreeHelper.GetChild(n,i)))yield return x;}return Visit(root);}
}
