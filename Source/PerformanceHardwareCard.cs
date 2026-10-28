using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// A vertical instrument panel. Every dial is tied to an available metric.
public sealed class PerformanceHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,List<TextBlock>> values=new();
 private readonly List<(string Id,TextBlock Number,TextBlock Unit,SegmentMeter Meter)> dials=new();
 private readonly List<(string Id,Border Fill,Border Track)> tracks=new();
 private readonly Preferences prefs;
 private readonly Brush edge;
 public PerformanceHardwareCard(Device device,Preferences p)
 {
  prefs=p;double scale=p.FontScale*CardLayouts.SpaceMultiplier(p);string accent=p.GraphColors.TryGetValue(device.Category,out var custom)?DesignCatalog.ColorOr(custom,"#68D9A1"):"#68D9A1",secondary=DesignCatalog.ColorOr(p.SecondaryGraphColor,"#E16467"),track=DesignCatalog.ColorOr(p.GraphTrackColor,"#242A31");bool customPalette=p.GraphColors.ContainsKey(device.Category)||!string.IsNullOrWhiteSpace(p.SecondaryGraphColor);
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));Resources["Text"]=Ui.Brush(DesignCatalog.ColorOr(p.TextColor,"#F0F2F5"));Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,"#A7B0BC"));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,"#D7DFE8"));
  BorderThickness=new Thickness(1);edge=Ui.Brush("#343B40");BorderBrush=edge;CornerRadius=new CornerRadius(2);Padding=new Thickness(12);Margin=new Thickness(3,0,3,7);var back=Ui.Brush("#030505");back.Opacity=p.Opacity;Background=back;
  var surface=new Grid();Child=surface;surface.Children.Add(new PanelFrame(accent,secondary){IsHitTestVisible=false});var body=new ExpandedReadingsPanel(p,2);surface.Children.Add(body);
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,15*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.Bold;heading.Children.Add(title);body.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,5,0,12);body.Children.Add(name);
  var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(154)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var primary=device.Category=="Network"?device.Metrics.FirstOrDefault(m=>m.Label=="Download"):device.Metrics.FirstOrDefault(m=>m.Unit=="%"&&(m.Label=="Auslastung"||m.Label=="Belegt"));primary??=CardLayouts.MainMetric(device);
  var instrument=new StackPanel{Width=142,Margin=new Thickness(0,0,12,0),HorizontalAlignment=HorizontalAlignment.Left};content.Children.Add(instrument);
  var main=Dial(primary,142,30,accent,secondary,track,customPalette,true);instrument.Children.Add(main);
  var temperature=device.Metrics.FirstOrDefault(m=>m.Unit=="°C"&&m.Id!=primary?.Id);if(temperature!=null){var text=Value(temperature,12*p.FontScale*Preferences.TextFactor(p.ValueTextScale));text.HorizontalAlignment=HorizontalAlignment.Center;text.Margin=new Thickness(0,5,0,0);main.Children.Add(new Border{Child=text,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,83,0,0)});}
  var miniMetrics=device.Category is "CPU" or "GPU"?device.Metrics.Where(m=>m.Label=="Leistung"||m.Unit=="RPM"||m.Label=="Spannung").OrderBy(m=>m.Label=="Leistung"?0:m.Unit=="RPM"?1:2).Take(2).ToArray():device.Category=="RAM"?device.Metrics.Where(m=>m.Unit=="GB"&&(m.Label=="Belegt"||m.Label=="Verfügbar")).Take(2).ToArray():Array.Empty<Metric>();
  if(miniMetrics.Length>0){var minis=new System.Windows.Controls.Primitives.UniformGrid{Columns=2,Margin=new Thickness(0,4,0,0)};foreach(var metric in miniMetrics){var panel=new StackPanel();var label=Ui.SidebarText(metric.Label,8*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.HorizontalAlignment=HorizontalAlignment.Center;label.TextWrapping=TextWrapping.Wrap;panel.Children.Add(label);panel.Children.Add(Dial(metric,65,15,accent,secondary,track,customPalette,false));minis.Children.Add(panel);}instrument.Children.Add(minis);}
  var readings=new ExpandedReadingsPanel(p);Grid.SetColumn(readings,1);content.Children.Add(readings);
  foreach(var metric in device.Metrics){var label=Ui.SidebarText(metric.Label,9*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.ToolTip=Ui.SidebarFullLabel(metric.Label);var value=Value(metric,11*scale*Preferences.TextFactor(p.ValueTextScale));var row=new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale));var cell=new StackPanel();cell.Children.Add(row);
   if(metric.Unit!="IP"&&metric.TextValue==null){var fill=new Border{HorizontalAlignment=HorizontalAlignment.Left,Width=0,Background=PaletteBrush(accent,secondary,customPalette)};var bar=new Border{Height=3*DesignCatalog.GraphSize(p),Background=Ui.Brush(track),Child=fill,Margin=new Thickness(0,5,0,0)};tracks.Add((metric.Id,fill,bar));bar.SizeChanged+=(_,_)=>{if(fill.Tag is double ratio)fill.Width=Math.Max(0,bar.ActualWidth*ratio);};cell.Children.Add(bar);}
   readings.Children.Add(new Border{Child=cell,BorderBrush=Ui.Brush("#1C2328"),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(0,3,0,6),Margin=new Thickness(0,0,0,4)});
  }
  CardLayouts.PositionInstrument(content,instrument,readings,p);if(content.RowDefinitions.Count==0)content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});Update(device,p);
 }
 private TextBlock Value(Metric m,double size){var t=Ui.Text("—",size);t.Foreground=Ui.ValueBrush(prefs,"#F1F4F6");t.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");t.FontWeight=FontWeights.SemiBold;t.TextTrimming=TextTrimming.None;if(m.Unit=="IP")t.TextWrapping=TextWrapping.Wrap;if(!values.TryGetValue(m.Id,out var list))values[m.Id]=list=new();list.Add(t);return t;}
 private Grid Dial(Metric? metric,double size,double font,string a,string b,string track,bool custom,bool primary)
 {
  var g=new Grid{Width=size,Height=size};var meter=new SegmentMeter(a,b,track,custom,DesignCatalog.GraphSize(prefs));g.Children.Add(meter);var digits=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=primary?new Thickness(0,-17,0,0):new Thickness(0)};var n=Ui.Text("—",font*prefs.FontScale*Preferences.TextFactor(prefs.InstrumentTextScale));n.FontWeight=FontWeights.Bold;n.Foreground=Ui.ValueBrush(prefs,"#FFFFFF");n.HorizontalAlignment=HorizontalAlignment.Center;var u=Ui.Text("",(primary?10:8)*prefs.FontScale*Preferences.TextFactor(prefs.UnitTextScale),"Muted");u.HorizontalAlignment=HorizontalAlignment.Center;digits.Children.Add(n);digits.Children.Add(u);g.Children.Add(new Viewbox{Child=digits,Width=size*.62,Height=size*.55,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly});if(primary)System.Windows.Automation.AutomationProperties.SetAutomationId(n,"PrimaryMetric");if(metric!=null)dials.Add((metric.Id,n,u,meter));return g;
 }
 private static Brush PaletteBrush(string a,string b,bool custom)=>new LinearGradientBrush(new GradientStopCollection{new(Ui.Brush(custom?a:"#5DD69A").Color,0),new(Ui.Brush(custom?a:"#D8BD64").Color,.52),new(Ui.Brush(custom?b:"#F05260").Color,1)},new Point(0,0),new Point(1,0));
 private static double Ratio(Metric? metric)=>metric?.Value is double v&&double.IsFinite(v)?Math.Clamp(v/Math.Max(1,metric.Unit=="%"||metric.Unit=="°C"?100:double.IsFinite(metric.Maximum)?metric.Maximum:1),0,1):0;
 public void Update(Device device,Preferences p)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var list))foreach(var text in list){text.Text=metric.Display(p.Fahrenheit);text.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text.Text+(metric.Source==null?"":"\n"+metric.Source);}
  foreach(var dial in dials){var metric=device.Metrics.FirstOrDefault(m=>m.Id==dial.Id);string text=metric?.Display(p.Fahrenheit)??"—";int split=text.LastIndexOf(' ');dial.Number.Text=split<0?text:text[..split];dial.Unit.Text=split<0?metric?.Unit??"":text[(split+1)..];dial.Meter.Ratio=Ratio(metric);dial.Meter.InvalidateVisual();}
  foreach(var bar in tracks){double ratio=Ratio(device.Metrics.FirstOrDefault(m=>m.Id==bar.Id));bar.Fill.Tag=ratio;bar.Fill.Width=Math.Max(0,bar.Track.ActualWidth*ratio);}
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class PanelFrame:FrameworkElement
 {
  private readonly Pen left,right,line;public PanelFrame(string a,string b){left=new Pen(Ui.Brush(a),1);right=new Pen(Ui.Brush(b),1);line=new Pen(Ui.Brush("#252C31"),.8);}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<30||h<30)return;dc.DrawLine(line,new Point(0,25),new Point(w,25));foreach(var corner in new[]{(0d,0d,1d,1d),(w,0d,-1d,1d),(0d,h,1d,-1d),(w,h,-1d,-1d)}){var (x,y,sx,sy)=corner;var pen=sx>0?left:right;dc.DrawLine(pen,new Point(x-7*sx,y+10*sy),new Point(x-7*sx,y-2*sy));dc.DrawLine(pen,new Point(x-7*sx,y-2*sy),new Point(x-2*sx,y-7*sy));dc.DrawLine(pen,new Point(x-2*sx,y-7*sy),new Point(x+13*sx,y-7*sy));}for(int i=0;i<3;i++){dc.DrawRectangle(left.Brush,null,new Rect(i*5,h+7,2,2));dc.DrawRectangle(right.Brush,null,new Rect(w-i*5-2,h+7,2,2));}}
 }
 private sealed class SegmentMeter:FrameworkElement
 {
  public double Ratio;private readonly Color first,last;private readonly bool custom;private readonly Brush track;private readonly double thickness;
  public SegmentMeter(string a,string b,string track,bool custom,double thickness){first=Ui.Brush(a).Color;last=Ui.Brush(b).Color;this.track=Ui.Brush(track);this.custom=custom;this.thickness=thickness;}
  private Color ColorAt(double t){Color Lerp(Color a,Color b,double amount)=>Color.FromRgb((byte)(a.R+(b.R-a.R)*amount),(byte)(a.G+(b.G-a.G)*amount),(byte)(a.B+(b.B-a.B)*amount));return custom?Lerp(first,last,t):t<.5?Lerp(Ui.Brush("#50D491").Color,Ui.Brush("#DAC164").Color,t*2):Lerp(Ui.Brush("#DAC164").Color,Ui.Brush("#ED4B59").Color,(t-.5)*2);}
  protected override void OnRender(DrawingContext dc){double cx=ActualWidth/2,cy=ActualHeight/2,r=Math.Max(1,Math.Min(cx,cy)-8),stroke=Math.Max(2,r*.12)*thickness;Point At(double angle)=>new(cx+r*Math.Cos(angle*Math.PI/180),cy+r*Math.Sin(angle*Math.PI/180));dc.DrawEllipse(null,new Pen(track,.7),new Point(cx,cy),Math.Max(1,r-stroke),Math.Max(1,r-stroke));for(int i=0;i<44;i++){double angle=135+i*270d/44;var f=new PathFigure{StartPoint=At(angle)};f.Segments.Add(new ArcSegment(At(angle+4.7),new Size(r,r),0,false,SweepDirection.Clockwise,true));var geometry=new PathGeometry(new[]{f});dc.DrawGeometry(null,new Pen(i/44d<Ratio?new SolidColorBrush(ColorAt(Math.Clamp(i/43d/Math.Max(.001,Ratio),0,1))):track,stroke),geometry);}}
 }
}
