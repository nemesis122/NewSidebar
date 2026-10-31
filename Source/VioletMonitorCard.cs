using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Black reference-style surfaces, open violet/magenta ring frames and square sensor bars.
// The decorative ring is a frame, not an invented percentage/progress measurement.
public sealed class VioletMonitorCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly List<(string Id,VioletBar Bar)> bars=new();
 private readonly List<(string Id,HistoryAreaGraph Plot)> plots=new();
 private readonly TextBlock? number,unit;
 private readonly string? mainId;
 private readonly MetricHistory history;
 private readonly string deviceId;
 public VioletMonitorCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;double s=p.FontScale;
  string ink=DesignCatalog.ColorOr(p.TextColor,"#F0F0F0"),muted=DesignCatalog.ColorOr(p.LabelColor,"#A4A4A4"),track=DesignCatalog.ColorOr(p.GraphTrackColor,"#303030"),accent=p.GraphColors.TryGetValue(device.Category,out var chosen)?DesignCatalog.ColorOr(chosen,"#9300DC"):"#9300DC",end=DesignCatalog.ColorOr(p.SecondaryGraphColor,"#E000CB");
  Resources["Text"]=Ui.Brush(ink);Resources["Muted"]=Ui.Brush(muted);Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,ink));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(10,10,10,9);Margin=new Thickness(3,0,3,7);CornerRadius=new CornerRadius(7);BorderThickness=new Thickness(1);BorderBrush=Ui.Brush("#4A4A4A");var back=Ui.Brush("#000000");back.Opacity=p.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(p,1);Child=body;
  var iconTitle=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};iconTitle.Children.Add(Ui.HardwareIcon(device.Category,18*s*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,13*s*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.Bold;iconTitle.Children.Add(title);
  var hardware=Ui.HardwareTitle(device.Name,11*s*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));hardware.TextWrapping=TextWrapping.Wrap;hardware.TextTrimming=TextTrimming.None;
  body.Children.Add(new ReferenceHeader(iconTitle,hardware){Margin=new Thickness(0,0,0,10)});
  var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(126)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var readings=new ExpandedReadingsPanel(p);Grid.SetColumn(readings,1);content.Children.Add(readings);
  bool circular=device.Category is "CPU" or "GPU";
  if(circular&&CardLayouts.MainMetric(device) is Metric main)
  {
   mainId=main.Id;var instrument=new Grid{Width=116,Height=122,Margin=new Thickness(0,0,10,0),HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center};instrument.Children.Add(new VioletRing(accent,end,DesignCatalog.GraphSize(p)));
   number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",28*s*Preferences.TextFactor(p.InstrumentTextScale));number.Foreground=Ui.ValueBrush(p,ink);number.FontWeight=FontWeights.Bold;number.HorizontalAlignment=HorizontalAlignment.Center;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
   unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",11*s*Preferences.TextFactor(p.UnitTextScale),"Muted");unit.HorizontalAlignment=HorizontalAlignment.Center;
   var digits=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,0,10)};digits.Children.Add(number);digits.Children.Add(unit);instrument.Children.Add(new Viewbox{Child=digits,MaxWidth=70,MaxHeight=66,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center});
   var caption=Ui.SidebarText(main.Label,11*s*Preferences.TextFactor(p.LabelTextScale),"Muted");caption.HorizontalAlignment=HorizontalAlignment.Center;caption.VerticalAlignment=VerticalAlignment.Bottom;caption.Margin=new Thickness(0,0,0,1);instrument.Children.Add(new Viewbox{Child=caption,MaxWidth=84,MaxHeight=22,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,VerticalAlignment=VerticalAlignment.Bottom,HorizontalAlignment=HorizontalAlignment.Center});content.Children.Add(instrument);
   CardLayouts.PositionInstrument(content,instrument,readings,p);
  }
  else
  {
   content.ColumnDefinitions.Clear();content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});Grid.SetColumn(readings,0);
  }
  Panel target=readings;
  if(device.Category=="Network")
  {
   // Preserve one common value edge for all text, including IPs, next to the trace column.
   var split=new Grid();split.ColumnDefinitions.Add(new(){Width=new GridLength(1.6,GridUnitType.Star)});split.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var lines=new ExpandedReadingsPanel(p);split.Children.Add(lines);target=lines;
   var trace=new StackPanel{Margin=new Thickness(9,2,0,0),VerticalAlignment=VerticalAlignment.Top};Grid.SetColumn(trace,1);split.Children.Add(trace);
   foreach(var metric in device.Metrics.Where(m=>m.Unit!="IP"&&(m.Label=="Download"||m.Label=="Upload")).Take(2))
   {
    var plot=new HistoryAreaGraph(accent,track,DesignCatalog.GraphSize(p));var plotFrame=new Grid{Height=27*Preferences.TextFactor(p.InstrumentScale),Margin=new Thickness(0,0,0,5)};plotFrame.Children.Add(new NetworkGrid(track));plotFrame.Children.Add(plot);plots.Add((metric.Id,plot));trace.Children.Add(plotFrame);
   }
   readings.Children.Add(split);
  }
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*s*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   double size=11*s*Preferences.TextFactor(p.ValueTextScale);var value=Ui.RoleText(SidebarTextRole.Readings,"—",size);value.FontFamily=Ui.Font(p,"Segoe UI");value.FontWeight=FontWeights.SemiBold;value.Foreground=Ui.ValueBrush(p,ink);value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;values[metric.Id]=value;
   if(metric.Unit=="IP"){target.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(0,2,0,2)});continue;}
   bool memory=device.Category=="RAM",plain=device.Category=="Network"||memory&&metric.Unit!="%";
   if(plain){target.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(0,2,0,2)});continue;}
   var bar=new VioletBar(accent,track){Height=7*Preferences.TextFactor(p.InstrumentScale)*DesignCatalog.GraphSize(p)};bars.Add((metric.Id,bar));
   if(memory)
   {
    target.Children.Add(new CompactBarRow(label,value,bar){Margin=new Thickness(0,1,0,5)});
   }
   else
   {
    var field=new StackPanel{Margin=new Thickness(0,3,0,5)};
    bool coreClock=metric.Unit=="MHz"&&(metric.Label.Contains("Core",StringComparison.OrdinalIgnoreCase)||metric.Label.Contains("Kern",StringComparison.OrdinalIgnoreCase));
    var row=new AddressReadoutPanel(label,value){Margin=new Thickness(0,coreClock?0:1,0,coreClock?2:0)};
    if(coreClock)field.Children.Add(row);field.Children.Add(bar);if(!coreClock)field.Children.Add(row);target.Children.Add(field);
   }
  }
  if(content.RowDefinitions.Count==0)content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});
  Update(device,p);
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values)pair.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key)?.Display(p.Fahrenheit)??"—";
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==mainId);string formatted=primary?.Display(p.Fahrenheit)??"—";int split=formatted.LastIndexOf(' ');if(number!=null)number.Text=split<0?formatted:formatted[..split];if(unit!=null)unit.Text=split<0?primary?.Unit??"":formatted[(split+1)..];
  foreach(var (id,bar) in bars){var metric=device.Metrics.FirstOrDefault(m=>m.Id==id);bar.Ratio=metric?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,metric.Unit is "%" or "°C"?100:double.IsFinite(metric.Maximum)?metric.Maximum:100),0,1):null;bar.InvalidateVisual();}
  foreach(var (id,plot) in plots)plot.SetSamples(history.Read(deviceId,id),history.LatestTime,0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  BorderBrush=Ui.Brush(device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?"#F47777":"#4A4A4A");
 }
 private sealed class ReferenceHeader:Panel
 {
  readonly FrameworkElement heading,name;bool inline;double first;
  public ReferenceHeader(FrameworkElement heading,FrameworkElement name){this.heading=heading;this.name=name;Children.Add(heading);Children.Add(name);}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):300;heading.Measure(new Size(w,double.PositiveInfinity));first=Math.Min(w,heading.DesiredSize.Width);inline=w-first-7>=115;name.Measure(new Size(inline?Math.Max(0,w-first-7):w,double.PositiveInfinity));return new Size(w,inline?Math.Max(heading.DesiredSize.Height,name.DesiredSize.Height):heading.DesiredSize.Height+name.DesiredSize.Height+3);}
  protected override Size ArrangeOverride(Size s){heading.Arrange(new Rect(0,0,first,heading.DesiredSize.Height));name.Arrange(new Rect(inline?first+7:0,inline?0:heading.DesiredSize.Height+3,inline?Math.Max(0,s.Width-first-7):s.Width,name.DesiredSize.Height));return s;}
 }
 private sealed class CompactBarRow:Panel
 {
  readonly TextBlock label,value;readonly VioletBar bar;double left,right,gap;
  public CompactBarRow(TextBlock label,TextBlock value,VioletBar bar){this.label=label;this.value=value;this.bar=bar;Children.Add(label);Children.Add(bar);Children.Add(value);value.TextAlignment=TextAlignment.Right;}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):300;label.Measure(new Size(w*.3,double.PositiveInfinity));value.Measure(new Size(w*.3,double.PositiveInfinity));left=Math.Min(w*.3,label.DesiredSize.Width);right=Math.Min(w*.3,value.DesiredSize.Width);gap=Math.Max(0,w-left-right-14);bar.Measure(new Size(gap,bar.Height));return new Size(w,Math.Max(Math.Max(label.DesiredSize.Height,value.DesiredSize.Height),bar.DesiredSize.Height));}
  protected override Size ArrangeOverride(Size s){label.Arrange(new Rect(0,Math.Max(0,(s.Height-label.DesiredSize.Height)/2),left,label.DesiredSize.Height));bar.Arrange(new Rect(left+7,Math.Max(0,(s.Height-bar.DesiredSize.Height)/2),gap,bar.DesiredSize.Height));value.Arrange(new Rect(Math.Max(0,s.Width-right),Math.Max(0,(s.Height-value.DesiredSize.Height)/2),right,value.DesiredSize.Height));return s;}
 }
 private sealed class NetworkGrid:FrameworkElement
 {
  readonly Pen pen;public NetworkGrid(string track){pen=new Pen(Ui.Brush(track),.4);ClipToBounds=true;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){for(double x=0;x<ActualWidth;x+=5)dc.DrawLine(pen,new Point(x,0),new Point(x,ActualHeight));for(double y=0;y<ActualHeight;y+=5)dc.DrawLine(pen,new Point(0,y),new Point(ActualWidth,y));}
 }
 public sealed class VioletBar:FrameworkElement
 {
  public double? Ratio;readonly Brush ink,track;
  public VioletBar(string accent,string track){ink=Ui.Brush(accent);this.track=Ui.Brush(track);ClipToBounds=true;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<=0||h<=0)return;dc.DrawRectangle(track,null,new Rect(0,0,w,h));if(Ratio is double ratio&&ratio>0)dc.DrawRectangle(ink,null,new Rect(0,0,w*Math.Clamp(ratio,0,1),h));}
 }
 public sealed class VioletRing:FrameworkElement
 {
  readonly Brush ink;readonly double thickness;
  public VioletRing(string start,string end,double thickness){ink=new LinearGradientBrush(Ui.Brush(start).Color,Ui.Brush(end).Color,new Point(0,0),new Point(1,0));this.thickness=thickness;IsHitTestVisible=false;ClipToBounds=true;}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<10||h<10)return;double radius=Math.Min(w*.5-7,h*.5-15),cx=w/2,cy=h/2-6;Point At(double a)=>new(cx+radius*Math.Cos(a*Math.PI/180),cy+radius*Math.Sin(a*Math.PI/180));var figure=new PathFigure{StartPoint=At(145)};figure.Segments.Add(new ArcSegment(At(395),new Size(radius,radius),0,true,SweepDirection.Clockwise,true));var pen=new Pen(ink,9*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};dc.DrawGeometry(null,pen,new PathGeometry(new[]{figure}));dc.DrawEllipse(ink,null,new Point(cx-radius*.69,cy+radius*.78),3.6*thickness,3.6*thickness);dc.DrawEllipse(ink,null,new Point(cx-radius*.51,cy+radius*.96),3.6*thickness,3.6*thickness);}
 }
}
