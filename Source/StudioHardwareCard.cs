using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Three distinct compositions. Graphics use actual metrics and the bounded shared history.
public sealed class StudioHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly string deviceId;
 private readonly string? primaryId;
 private readonly MetricHistory history;
 private readonly TextBlock number,unit;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly StudioMeter? meter;
 private readonly HistoryAreaGraph? plot;
 private readonly Brush edge;
 public StudioHardwareCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);bool glass=Composition=="FluentGlass",telemetry=Composition=="Telemetry";
  var d=DesignCatalog.Resolve(p);string accent=DesignCatalog.CategoryColor(d,device.Category,p),secondary=DesignCatalog.ColorOr(p.SecondaryGraphColor,accent),track=DesignCatalog.ColorOr(p.GraphTrackColor,d.Track);
  double scale=p.FontScale*CardLayouts.SpaceMultiplier(p);System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Resources["Text"]=Ui.Brush(d.Text);Resources["Muted"]=Ui.Brush(d.Muted);Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,d.Text));
  Padding=new Thickness(glass?15:12);Margin=new Thickness(3,0,3,7);CornerRadius=new CornerRadius(glass?19:telemetry?10:7);BorderThickness=new Thickness(1);edge=Ui.Brush(d.Edge);BorderBrush=edge;
  var surface=new LinearGradientBrush(new GradientStopCollection{new(Ui.Brush(d.Panel).Color,0),new(Mix(Ui.Brush(d.Panel).Color,Ui.Brush(accent).Color,glass?.13:.035),1)},new Point(0,0),new Point(1,1));surface.Opacity=p.Opacity;Background=surface;
  var body=new ExpandedReadingsPanel(p,2);Child=body;
  var header=new Grid();header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});header.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,15*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);header.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var menu=new Button{Content="⋯",FontSize=14,Padding=new Thickness(3,0,3,0),Background=Brushes.Transparent,BorderBrush=Brushes.Transparent,ToolTip="Widget settings"};System.Windows.Automation.AutomationProperties.SetAutomationId(menu,"BoardWidgetSettings");menu.Click+=(_,_)=>{Application.Current.Windows.OfType<SidebarWindow>().FirstOrDefault()?.OpenSettingsForDevice(deviceId);};Grid.SetColumn(menu,1);header.Children.Add(menu);body.Children.Add(header);
  var hardware=Ui.HardwareTitle(device.Name,11*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));hardware.TextWrapping=TextWrapping.Wrap;hardware.TextTrimming=TextTrimming.None;hardware.Margin=new Thickness(0,5,0,glass?12:9);body.Children.Add(hardware);
  var primary=CardLayouts.MainMetric(device);primaryId=primary?.Id;
  var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(132)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var instrument=new Grid{Width=120,Height=glass?126:telemetry?132:84,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,12,0)};content.Children.Add(instrument);
  number=Ui.Text("—",(glass?31:telemetry?29:32)*p.FontScale*Preferences.TextFactor(p.InstrumentTextScale));number.FontWeight=FontWeights.SemiBold;number.Foreground=Ui.ValueBrush(p,accent);number.TextTrimming=TextTrimming.None;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
  unit=Ui.Text("",11*p.FontScale*Preferences.TextFactor(p.UnitTextScale),"Muted");
  var digits=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};number.HorizontalAlignment=HorizontalAlignment.Center;unit.HorizontalAlignment=HorizontalAlignment.Center;digits.Children.Add(number);digits.Children.Add(unit);
  if(glass){meter=new StudioMeter(accent,secondary,track,DesignCatalog.GraphSize(p),true);instrument.Children.Add(meter);instrument.Children.Add(new Viewbox{Child=digits,Width=76,Height=76,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly});}
  else if(telemetry){instrument.RowDefinitions.Add(new(){Height=GridLength.Auto});instrument.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});instrument.RowDefinitions.Add(new(){Height=GridLength.Auto});digits.Orientation=Orientation.Horizontal;digits.HorizontalAlignment=HorizontalAlignment.Left;unit.VerticalAlignment=VerticalAlignment.Bottom;unit.Margin=new Thickness(5,0,0,4);instrument.Children.Add(new Viewbox{Child=digits,Height=42,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});var chart=new Grid{Margin=new Thickness(0,8,0,5)};chart.Children.Add(new ChartGrid(track));plot=new HistoryAreaGraph(accent,track,DesignCatalog.GraphSize(p));chart.Children.Add(plot);Grid.SetRow(chart,1);instrument.Children.Add(chart);var legend=Ui.Text("60s",9*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");legend.ToolTip="Last 60 seconds · live samples only";Grid.SetRow(legend,2);instrument.Children.Add(legend);}
  else{digits.HorizontalAlignment=HorizontalAlignment.Left;digits.Orientation=Orientation.Horizontal;unit.VerticalAlignment=VerticalAlignment.Bottom;unit.Margin=new Thickness(5,0,0,5);instrument.Children.Add(new Viewbox{Child=digits,Height=48,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top});meter=new StudioMeter(accent,secondary,track,DesignCatalog.GraphSize(p),false){Height=14,VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(0,0,0,10)};instrument.Children.Add(meter);}
  var readings=new ExpandedReadingsPanel(p);Grid.SetColumn(readings,1);content.Children.Add(readings);
  foreach(var metric in device.Metrics){var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.ToolTip=Ui.SidebarFullLabel(metric.Label);var value=Ui.Text("—",11*scale*Preferences.TextFactor(p.ValueTextScale));value.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");value.FontWeight=FontWeights.Medium;value.Foreground=Ui.ValueBrush(p,d.Text);value.TextTrimming=TextTrimming.None;if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   var row=new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(0,glass?4:telemetry?3:1,0,glass?4:telemetry?3:1)};
   if(telemetry)readings.Children.Add(new Border{Child=row,BorderBrush=Ui.Brush(d.Edge),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(0,2,0,4)});else readings.Children.Add(row);
  }
  CardLayouts.PositionInstrument(content,instrument,readings,p);if(content.RowDefinitions.Count==0)content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});Update(device,p);
 }
 private static Color Mix(Color a,Color b,double t)=>Color.FromRgb((byte)(a.R+(b.R-a.R)*t),(byte)(a.G+(b.G-a.G)*t),(byte)(a.B+(b.B-a.B)*t));
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values){var metric=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key);pair.Value.Text=metric?.Display(p.Fahrenheit)??"—";pair.Value.ToolTip=metric==null?"Unavailable":Ui.SidebarFullLabel(metric.Label)+" · "+pair.Value.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);var display=primary?.Display(p.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split<0?display:display[..split];unit.Text=split<0?primary?.Unit??"":display[(split+1)..];number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+display;
  if(meter!=null){meter.Available=primary?.Value is double raw&&double.IsFinite(raw);meter.Ratio=primary?.Value is double v&&double.IsFinite(v)?Math.Clamp(v/Math.Max(1,primary.Unit is "%" or "°C"?100:double.IsFinite(primary.Maximum)?primary.Maximum:100),0,1):0;meter.InvalidateVisual();}
  if(plot!=null)plot.SetSamples(primaryId==null?Array.Empty<HistorySample>():history.Read(deviceId,primaryId),history.LatestTime,primary?.Unit=="%"?100:primary?.Unit=="°C"?Math.Max(100,primary.Maximum):0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class ChartGrid:FrameworkElement{private readonly Pen pen;public ChartGrid(string color){pen=new Pen(Ui.Brush(color),.6);IsHitTestVisible=false;}protected override void OnRender(DrawingContext dc){for(int i=1;i<4;i++)dc.DrawLine(pen,new Point(2,ActualHeight*i/4),new Point(Math.Max(2,ActualWidth-2),ActualHeight*i/4));}}
 private sealed class StudioMeter:FrameworkElement
 {
  public double Ratio;public bool Available;private readonly Brush ink,track;private readonly bool circular;private readonly double thickness;
  public StudioMeter(string accent,string secondary,string track,double thickness,bool circular){ink=new LinearGradientBrush(Ui.Brush(accent).Color,Ui.Brush(secondary).Color,new Point(0,1),new Point(1,0));this.track=Ui.Brush(track);this.thickness=thickness;this.circular=circular;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<4||h<4)return;if(!circular){double y=h/2;dc.DrawRoundedRectangle(track,null,new Rect(0,y-2,w,4),2,2);if(Available){dc.DrawRoundedRectangle(ink,null,new Rect(0,y-2,w*Ratio,4),2,2);dc.DrawEllipse(ink,null,new Point(Math.Clamp(w*Ratio,3,Math.Max(3,w-3)),y),3,3);}return;}double r=Math.Max(2,Math.Min(w,h)/2-8),cx=w/2,cy=h/2;Point At(double a)=>new(cx+r*Math.Cos(a*Math.PI/180),cy+r*Math.Sin(a*Math.PI/180));var line=new Pen(track,3*thickness);dc.DrawEllipse(null,line,new Point(cx,cy),r,r);dc.DrawEllipse(null,new Pen(track,.7),new Point(cx,cy),Math.Max(1,r-7),Math.Max(1,r-7));if(Available&&Ratio>0){double sweep=Math.Min(359.9,360*Ratio);var f=new PathFigure{StartPoint=At(-90)};f.Segments.Add(new ArcSegment(At(-90+sweep),new Size(r,r),0,sweep>180,SweepDirection.Clockwise,true));dc.DrawGeometry(null,new Pen(ink,3*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},new PathGeometry(new[]{f}));dc.DrawEllipse(ink,null,At(-90+sweep),3.3,3.3);}}
 }
}
