using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

public sealed class CyberHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly TextBlock number,unit;
 private readonly SensorDial dial;
 private readonly string? primaryId;
 private readonly Brush normalEdge;
 public CyberHardwareCard(Device device,Preferences p)
 {
  bool neon=CardLayouts.BaseLayout(p.Layout)=="CyberPulse";
  double scale=p.FontScale*CardLayouts.SpaceMultiplier(p);
  string accent=p.GraphColors.TryGetValue(device.Category,out var custom)?DesignCatalog.ColorOr(custom,"#EA36EC"):neon?"#EC32F0":"#B65CF7";
  string secondary=DesignCatalog.ColorOr(p.SecondaryGraphColor,"#35DFF4"),track=DesignCatalog.ColorOr(p.GraphTrackColor,"#252D3A");
  Resources["Text"]=Ui.Brush(DesignCatalog.ColorOr(p.TextColor,"#F1F4FA"));Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,"#A6B2C8"));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,"#E0E7F3"));
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(neon?11:13);Margin=new Thickness(3,0,3,8);CornerRadius=new CornerRadius(neon?8:3);BorderThickness=new Thickness(neon?2:1);
  normalEdge=neon?Gradient(accent,secondary):Ui.Brush("#525861");BorderBrush=normalEdge;
  var back=new LinearGradientBrush((Color)ColorConverter.ConvertFromString(neon?"#080A18":"#202226"),(Color)ColorConverter.ConvertFromString("#06070B"),new Point(0,0),new Point(1,1)){Opacity=p.Opacity};Background=back;
  var surface=new Grid();Child=surface;surface.Children.Add(new FrameDecoration(neon,accent,secondary){IsHitTestVisible=false});
  var body=new ExpandedReadingsPanel(p,2);surface.Children.Add(body);
  var heading=new StackPanel{Orientation=Orientation.Horizontal};var icon=Ui.HardwareIcon(device.Category,(neon?23:16)*scale*Preferences.TextFactor(p.TitleTextScale));if(neon&&icon is System.Windows.Shapes.Path path){path.Stroke=Ui.Brush(accent);if(device.Category=="GPU")path.Data=Geometry.Parse("M1,5 L19,5 19,16 1,16 Z M0,3 L0,18 M5,7 A3,3 0 1 1 5,13 A3,3 0 1 1 5,7 M14,7 A3,3 0 1 1 14,13 A3,3 0 1 1 14,7");}heading.Children.Add(icon);var title=Ui.SidebarText(device.Category,(neon?15:13)*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.Bold;heading.Children.Add(title);
  var name=Ui.HardwareTitle(device.Name,(neon?12:10)*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.ToolTip=name.Text;
  if(neon){var top=new Grid{Margin=new Thickness(0,0,0,10)};top.ColumnDefinitions.Add(new(){Width=GridLength.Auto});top.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var badge=new Border{Child=heading,BorderBrush=Ui.Brush(accent),BorderThickness=new Thickness(1,0,0,1),CornerRadius=new CornerRadius(4),Padding=new Thickness(4,2,5,4),Margin=new Thickness(0,0,8,0)};top.Children.Add(badge);Grid.SetColumn(name,1);name.VerticalAlignment=VerticalAlignment.Center;top.Children.Add(name);body.Children.Add(top);body.Children.Add(new Border{Height=0});}
  else{body.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});name.Margin=new Thickness(0,4,0,10);body.Children.Add(name);}
  var content=new Grid();double dialSize=neon?Math.Clamp((Math.Max(200,p.ColumnWidth)-46)*.6,100,184):132;content.ColumnDefinitions.Add(new(){Width=new GridLength(dialSize+12)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var primary=neon&&device.Category=="RAM"?device.Metrics.FirstOrDefault(m=>m.Label=="Belegt"&&m.Unit=="GB")??CardLayouts.MainMetric(device):CardLayouts.MainMetric(device);primaryId=primary?.Id;
  var instrument=new Grid{Width=dialSize,Height=dialSize,Margin=new Thickness(0,0,12,0),VerticalAlignment=VerticalAlignment.Top};dial=new SensorDial(neon,accent,secondary,track,DesignCatalog.GraphSize(p));var dialFace=new Grid{Width=132,Height=132};dialFace.Children.Add(dial);instrument.Children.Add(new Viewbox{Child=dialFace,Stretch=Stretch.Uniform});
  var center=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",27*p.FontScale*Preferences.TextFactor(p.InstrumentTextScale));number.FontWeight=FontWeights.Bold;number.Foreground=Ui.ValueBrush(p,"#F4F6FF");number.HorizontalAlignment=HorizontalAlignment.Center;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");center.Children.Add(number);unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",11*p.FontScale*Preferences.TextFactor(p.UnitTextScale),"Muted");unit.HorizontalAlignment=HorizontalAlignment.Center;center.Children.Add(unit);dialFace.Children.Add(new Viewbox{Child=center,Width=86,Height=72,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly});if(neon){foreach(var tick in new[]{("0",19d),("100",105d)}){var text=Ui.RoleText(SidebarTextRole.InstrumentUnits,tick.Item1,6*p.FontScale*Preferences.TextFactor(p.UnitTextScale),"Muted");text.HorizontalAlignment=HorizontalAlignment.Left;text.VerticalAlignment=VerticalAlignment.Top;text.Margin=new Thickness(tick.Item2,111,0,0);dialFace.Children.Add(text);}}content.Children.Add(instrument);
  var readings=new ExpandedReadingsPanel(p);Grid.SetColumn(readings,1);content.Children.Add(readings);
  var ordered=neon?device.Metrics.Where(m=>m.Id!=primaryId||device.Category=="Network").OrderBy(m=>m.Label=="Auslastung"?0:m.Label.StartsWith("Takt",StringComparison.Ordinal)?1:m.Label=="Leistung"?2:3).ToList():device.Metrics;int readingIndex=0;
  foreach(var metric in ordered){bool feature=neon&&readingIndex++<3&&metric.Unit!="IP";double labelSize=(feature?9:10)*scale*Preferences.TextFactor(p.LabelTextScale),valueSize=(feature?21:12)*scale*Preferences.TextFactor(p.ValueTextScale);var label=Ui.SidebarText(metric.Label,labelSize,"Muted");label.ToolTip=Ui.SidebarFullLabel(metric.Label);var value=Ui.Text("—",valueSize);value.Foreground=Ui.ValueBrush(p,metric.Unit=="%"?accent:"#E9EDF6");value.FontFamily=Ui.Font(p,feature?"Segoe UI":"Cascadia Mono, Consolas");value.FontWeight=FontWeights.Bold;value.TextTrimming=TextTrimming.None;if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   if(feature){label.TextWrapping=TextWrapping.Wrap;value.TextWrapping=TextWrapping.Wrap;var block=new StackPanel{Margin=new Thickness(8,3,0,4),VerticalAlignment=VerticalAlignment.Center};block.Children.Add(label);block.Children.Add(value);readings.Children.Add(new Border{Child=block,BorderBrush=Gradient(accent,secondary),BorderThickness=new Thickness(1,0,0,1),Padding=new Thickness(0,0,0,3),Margin=new Thickness(0,0,0,4)});}
   else readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,valueSize){Margin=new Thickness(0,2,0,2)});
  }
  CardLayouts.PositionInstrument(content,instrument,readings,p);
  if(content.RowDefinitions.Count==0)content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});
  Update(device,p);
 }
 private static LinearGradientBrush Gradient(string first,string second)=>new((Color)ColorConverter.ConvertFromString(first),(Color)ColorConverter.ConvertFromString(second),new Point(0,1),new Point(1,0));
 public void Update(Device device,Preferences p)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var text)){text.Text=metric.Display(p.Fahrenheit);text.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(p.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split<0?display:display[..split];unit.Text=split<0?primary?.Unit??"":display[(split+1)..];
  dial.Ratio=primary?.Value is double v&&double.IsFinite(v)?Math.Clamp(v/Math.Max(1,primary.Unit=="%"?100:primary.Unit=="°C"?100:primary.Maximum),0,1):null;dial.InvalidateVisual();
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):normalEdge;
 }
 private sealed class FrameDecoration:FrameworkElement
 {
  private readonly bool neon;private readonly Brush edge,accent;public FrameDecoration(bool neon,string a,string b){this.neon=neon;edge=Ui.Brush(neon?"#1C3851":"#41444A");accent=Gradient(a,b);}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<24||h<24)return;dc.DrawLine(new Pen(accent,neon?1.3:2),new Point(8,-5),new Point(w-8,-5));dc.DrawLine(new Pen(edge,1),new Point(0,28),new Point(w,28));if(neon){for(double y=35;y<h;y+=12)dc.DrawLine(new Pen(Ui.Brush("#0A1824"),.35),new Point(0,y),new Point(w,y));for(double x=4;x<w;x+=12)dc.DrawLine(new Pen(Ui.Brush("#11232C"),.35),new Point(x,35),new Point(x,h));}else{foreach(var pt in new[]{new Point(-7,-7),new Point(w+7,-7),new Point(-7,h+7),new Point(w+7,h+7)}){dc.DrawEllipse(Ui.Brush("#08090B"),new Pen(edge,1),pt,2.5,2.5);dc.DrawLine(new Pen(edge,1),new Point(pt.X-1.5,pt.Y),new Point(pt.X+1.5,pt.Y));}dc.DrawLine(new Pen(accent,2),new Point(w*.3,h+5),new Point(w*.7,h+5));}}
 }
 private sealed class SensorDial:FrameworkElement
 {
  public double? Ratio;private readonly bool neon;private readonly Brush ink,track,secondary;private readonly double thickness;
  public SensorDial(bool neon,string a,string b,string track,double thickness){this.neon=neon;ink=Gradient(a,b);secondary=Ui.Brush(b);this.track=Ui.Brush(track);this.thickness=thickness;}
  private static Point At(double angle,double r)=>new(66+r*Math.Cos(angle*Math.PI/180),66+r*Math.Sin(angle*Math.PI/180));
  private static Geometry Arc(double start,double sweep,double radius){var f=new PathFigure{StartPoint=At(start,radius)};f.Segments.Add(new ArcSegment(At(start+sweep,radius),new Size(radius,radius),0,sweep>180,SweepDirection.Clockwise,true));return new PathGeometry(new[]{f});}
  protected override void OnRender(DrawingContext dc)
  {
   if(neon){var bowl=new LinearGradientBrush(new GradientStopCollection{new((Color)ColorConverter.ConvertFromString("#040710"),0),new((Color)ColorConverter.ConvertFromString("#101024"),.45),new((Color)ColorConverter.ConvertFromString("#6122B8"),.65),new((Color)ColorConverter.ConvertFromString("#1645E8"),.86),new((Color)ColorConverter.ConvertFromString("#00BDE1"),1)},new Point(0,0),new Point(1,1));dc.DrawEllipse(bowl,new Pen(track,1),new Point(66,66),41,41);}else dc.DrawEllipse(new RadialGradientBrush((Color)ColorConverter.ConvertFromString(neon?"#12294D":"#1C1F25"),(Color)ColorConverter.ConvertFromString("#05060A")),new Pen(track,1),new Point(66,66),46,46);
   for(int i=0;i<60;i++){double a=135+i*4.5;dc.DrawLine(new Pen(neon?ink:track,i%5==0?1.5:.7),At(a,61),At(a,i%5==0?65:63));}
   if(neon){dc.DrawGeometry(null,new Pen(track,7*thickness),Arc(135,270,53));if(Ratio is double ratio&&ratio>0){dc.DrawGeometry(null,new Pen(ink,7*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},Arc(135,Math.Max(.01,270*ratio),53));dc.DrawEllipse(secondary,null,At(135+270*ratio,53),3.2,3.2);}dc.DrawEllipse(null,new Pen(track,.7),new Point(66,66),40,40);}
   else for(int i=0;i<48;i++){var geometry=Arc(135+i*5.625,3.9,54);dc.DrawGeometry(null,new Pen(Ratio is double ratio&&i/48d<ratio?ink:track,4*thickness),geometry);}
  }
 }
}
