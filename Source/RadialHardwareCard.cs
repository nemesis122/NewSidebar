using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Aurora;

public sealed class RadialHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly SidebarDesign design;
 private readonly Dictionary<string,Readout> readings=new();
 private Dial? dial;
 public RadialHardwareCard(Device device,Preferences prefs)
 {
  design=DesignCatalog.Resolve(prefs);string accent=DesignCatalog.CategoryColor(design,device.Category,prefs);string layout=CardLayouts.BaseLayout(prefs.Layout);
  CornerRadius=new CornerRadius(design.Radius);BorderThickness=new Thickness(1);Padding=new Thickness(11,9,11,9);Margin=new Thickness(3,0,3,design.Gap);
  SetResourceReference(BackgroundProperty,"Panel");SetResourceReference(BorderBrushProperty,"Edge");
  var body=new StackPanel();Child=body;var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,14*prefs.FontScale));var title=Ui.SidebarText(device.Category,12*prefs.FontScale);title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(header);
  var name=Ui.HardwareTitle(device.Name,10*prefs.FontScale);name.TextTrimming=TextTrimming.CharacterEllipsis;name.ToolTip=Ui.HardwareName(device.Name);name.Margin=new Thickness(0,3,0,7);body.Children.Add(name);
  var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(112)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var primary=CardLayouts.MainMetric(device);
  if(primary!=null)
  {
   var secondary=layout!="TwinRings"?null:device.Category=="Network"?device.Metrics.FirstOrDefault(m=>m.Label=="Upload"):device.Metrics.FirstOrDefault(m=>m.Id!=primary.Id&&m.Label=="Auslastung")??device.Metrics.FirstOrDefault(m=>m.Id!=primary.Id&&m.Unit=="%")??device.Metrics.FirstOrDefault(m=>m.Id!=primary.Id);
   double size=layout=="Gauges"?96:layout=="ArcGauge"?108:104;
   dial=new Dial(primary.Id,size,layout=="ArcGauge",prefs.FontScale,accent,design.Track,design.Edge,layout=="OrbitGauge",secondary?.Id,layout,prefs);
   System.Windows.Automation.AutomationProperties.SetAutomationId(dial.Number,"PrimaryMetric");
   var instrument=new StackPanel{HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,0,8,0)};instrument.Children.Add(dial.View);
   content.Children.Add(instrument);
  }
  if(primary==null)content.ColumnDefinitions[0].Width=new GridLength(0);
  var values=new StackPanel{VerticalAlignment=VerticalAlignment.Top};System.Windows.Automation.AutomationProperties.SetAutomationId(values,"GaugeReadings");Grid.SetColumn(values,1);content.Children.Add(values);
  foreach(var metric in device.Metrics){var row=new Readout(metric,prefs.FontScale*CardLayouts.ReadingMultiplier(prefs.Layout),prefs,design.Text);readings[metric.Id]=row;values.Children.Add(row.View);}
 }
 public void Update(Device device,Preferences prefs)
 {
  foreach(var metric in device.Metrics)if(readings.TryGetValue(metric.Id,out var row))row.Update(metric,prefs);
  if(dial!=null){var primary=device.Metrics.FirstOrDefault(m=>m.Id==dial.Id);if(primary!=null)dial.Update(primary,device.Metrics.FirstOrDefault(m=>m.Id==dial.SecondaryId),prefs);}
  bool alert=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)));BorderBrush=Ui.Brush(alert?"#F47777":design.Edge);
 }
 private sealed class Readout
 {
  public Panel View{get;}
  private readonly TextBlock value;
  public Readout(Metric metric,double scale,Preferences prefs,string textColor)
  {
   var label=Ui.SidebarText(metric.Label,10*scale,"Muted");label.ToolTip=Ui.SidebarFullLabel(metric.Label);value=Ui.Text("—",11*scale);value.Foreground=Ui.ValueBrush(prefs,textColor);value.FontFamily=new FontFamily("Cascadia Mono, Consolas");value.TextTrimming=TextTrimming.CharacterEllipsis;if(metric.Unit=="IP"){value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;}
   View=new ReadoutPanel(label,value,11*scale){Margin=new Thickness(0,1,0,2)};
  }
  public void Update(Metric metric,Preferences prefs){value.Text=metric.Display(prefs.Fahrenheit);value.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+value.Text+(metric.Source==null?"":"\n"+metric.Source);}
 }
 // Choose line wrapping during Measure, never in SizeChanged/Arrange callbacks.
 // This prevents a text reflow from starting another layout pass indefinitely.
 internal sealed class ReadoutPanel:Panel
 {
  private readonly TextBlock label,value;private readonly double font;private bool inline;
  public ReadoutPanel(TextBlock label,TextBlock value,double font){this.label=label;this.value=value;this.font=font;Children.Add(label);Children.Add(value);}
  private static double TextWidth(TextBlock text,double? size=null)=>new FormattedText(text.Text,CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface(text.FontFamily,text.FontStyle,text.FontWeight,text.FontStretch),size??text.FontSize,Brushes.White,VisualTreeHelper.GetDpi(text).PixelsPerDip).WidthIncludingTrailingWhitespace;
  protected override Size MeasureOverride(Size available)
  {
   double width=double.IsFinite(available.Width)?Math.Max(0,available.Width):400;double textWidth=TextWidth(value,font);double nextFont=textWidth>width?Math.Max(Math.Min(8,font),font*width/Math.Max(1,textWidth)):font;if(Math.Abs(value.FontSize-nextFont)>.01)value.FontSize=nextFont;
   inline=TextWidth(label)+TextWidth(value)+7<=width;label.TextWrapping=inline?TextWrapping.NoWrap:TextWrapping.Wrap;
   value.Measure(new Size(width,double.PositiveInfinity));label.Measure(new Size(inline?Math.Max(0,width-value.DesiredSize.Width-7):width,double.PositiveInfinity));
   return new Size(width,inline?Math.Max(label.DesiredSize.Height,value.DesiredSize.Height):label.DesiredSize.Height+value.DesiredSize.Height);
  }
  protected override Size ArrangeOverride(Size size)
  {
   double valueWidth=Math.Min(size.Width,value.DesiredSize.Width);label.Arrange(new Rect(0,0,inline?Math.Max(0,size.Width-valueWidth-7):size.Width,label.DesiredSize.Height));value.Arrange(new Rect(inline?size.Width-valueWidth:0,inline?0:label.DesiredSize.Height,valueWidth,value.DesiredSize.Height));return size;
  }
 }
 private sealed class Dial
 {
  public string Id{get;}
  public string? SecondaryId{get;}
  public Grid View{get;}
  public TextBlock Number{get;}
  private readonly TextBlock unit;
  private readonly System.Windows.Shapes.Path fill;
  private readonly System.Windows.Shapes.Path? secondaryFill;
  private readonly double size,radius,start,sweep,centerY;
  private readonly string style;
  private readonly List<System.Windows.Shapes.Path> segments=new();
  private Border? columnFill;
  private Ellipse? marker;
  public Dial(string id,double size,bool half,double scale,string accent,string track,string edge,bool ticks,string? secondaryId,string style,Preferences prefs)
  {
   this.style=style;Id=id;SecondaryId=secondaryId;this.size=size;radius=size/2-7;centerY=size/2;start=half?180:style=="HorseshoeGauge"?120:style=="SpeedGauge"?135:270;sweep=half?180:style=="HorseshoeGauge"?300:style=="SpeedGauge"?270:360;
   View=new Grid{Width=size,Height=half?size*.65:size,HorizontalAlignment=HorizontalAlignment.Center};
   double graphScale=DesignCatalog.GraphSize(prefs);double stroke=(half?6:5)*graphScale;View.Children.Add(new System.Windows.Shapes.Path{Data=Arc(.999999),Stroke=Ui.Brush(track),StrokeThickness=stroke,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round});
   fill=new(){Stroke=Ui.Brush(accent),StrokeThickness=stroke,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round};View.Children.Add(fill);
   if(style=="HorseshoeGauge"){marker=new Ellipse{Width=8*graphScale,Height=8*graphScale,Fill=Ui.Brush(accent),Stroke=Ui.Brush(track),StrokeThickness=1,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top};View.Children.Add(marker);}
   if(style=="SegmentGauge")
   {
    View.Children.Clear();for(int i=0;i<24;i++){var segment=new System.Windows.Shapes.Path{Data=ArcSlice(i/24d,(i+.72)/24d),Stroke=Ui.Brush(accent),StrokeThickness=6*graphScale};View.Children.Add(new System.Windows.Shapes.Path{Data=segment.Data,Stroke=Ui.Brush(track),StrokeThickness=segment.StrokeThickness});segments.Add(segment);View.Children.Add(segment);}
   }
   if(style=="ColumnGauge")
   {
    View.Children.Clear();var trackColumn=new Border{Width=12*graphScale,Height=88,CornerRadius=new(6),Background=Ui.Brush(track),HorizontalAlignment=HorizontalAlignment.Left,Margin=new(4,0,0,0),VerticalAlignment=VerticalAlignment.Center};
    columnFill=new Border{Width=12*graphScale,Height=0,CornerRadius=new(6),Background=Ui.Brush(accent),VerticalAlignment=VerticalAlignment.Bottom};trackColumn.Child=columnFill;View.Children.Add(trackColumn);
   }
   if(secondaryId!=null){View.Children.Add(new System.Windows.Shapes.Path{Data=Arc(.999999,radius-10),Stroke=Ui.Brush(track),StrokeThickness=4*graphScale});secondaryFill=new(){Stroke=Ui.Brush(DesignCatalog.ColorOr(prefs.SecondaryGraphColor,"#78B9FC")),StrokeThickness=4*graphScale,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round};View.Children.Add(secondaryFill);}
   if(ticks)for(int i=0;i<12;i++){double angle=i*Math.PI/6;View.Children.Add(new Line{X1=size/2+(radius-10)*Math.Cos(angle),Y1=centerY+(radius-10)*Math.Sin(angle),X2=size/2+(radius-6)*Math.Cos(angle),Y2=centerY+(radius-6)*Math.Sin(angle),Stroke=Ui.Brush(edge),StrokeThickness=1});}
   var center=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Width=size-22,Margin=half?new Thickness(0,19,0,0):new Thickness(0)};
   if(style=="ColumnGauge"){center.Width=size-30;center.Margin=new Thickness(24,0,0,0);}
   Number=Ui.Text("—",(secondaryId!=null||size<100?18:20)*scale);Number.Foreground=Ui.ValueBrush(prefs,accent);Number.FontFamily=new FontFamily("Cascadia Mono, Consolas");Number.FontWeight=FontWeights.SemiBold;Number.HorizontalAlignment=HorizontalAlignment.Center;Number.TextTrimming=TextTrimming.CharacterEllipsis;Number.MaxWidth=size-22;
   unit=Ui.Text("",9*scale,"Muted");unit.HorizontalAlignment=HorizontalAlignment.Center;unit.Margin=new Thickness(0,2,0,0);center.Children.Add(Number);center.Children.Add(unit);View.Children.Add(center);
  }
  private double Ratio(Metric? metric)=>metric?.Value is double value&&double.IsFinite(value)?Math.Clamp(value/Math.Max(1,metric.Maximum),0,.999999):0;
  public void Update(Metric metric,Metric? secondary,Preferences prefs)
  {
   string text=metric.Display(prefs.Fahrenheit);int split=text.LastIndexOf(' ');Number.Text=split>=0?text.Substring(0,split):text;unit.Text=split>=0?text.Substring(split+1):prefs.Fahrenheit&&metric.Unit=="°C"?"°F":metric.Unit;Number.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text;
   double ratio=Ratio(metric);if(marker!=null){double angle=(start+sweep*ratio)*Math.PI/180;marker.RenderTransform=new TranslateTransform(size/2+radius*Math.Cos(angle)-marker.Width/2,centerY+radius*Math.Sin(angle)-marker.Height/2);marker.Visibility=metric.Value.HasValue?Visibility.Visible:Visibility.Hidden;}for(int i=0;i<segments.Count;i++)segments[i].Opacity=ratio>i/24d?1:0;if(columnFill!=null)columnFill.Height=88*ratio;fill.Data=ratio>0?Arc(ratio):null;if(secondaryFill!=null){double other=Ratio(secondary);secondaryFill.Data=other>0?Arc(other,radius-10):null;}
  }
  private Geometry ArcSlice(double from,double to)
  {
   Point At(double fraction){double angle=(start+sweep*fraction)*Math.PI/180;return new Point(size/2+radius*Math.Cos(angle),centerY+radius*Math.Sin(angle));}
   var figure=new PathFigure{StartPoint=At(from),IsClosed=false};figure.Segments.Add(new ArcSegment(At(to),new Size(radius,radius),0,false,SweepDirection.Clockwise,true));return new PathGeometry(new[]{figure});
  }
  private Geometry Arc(double ratio,double? overrideRadius=null)
  {
   double r=overrideRadius??radius;
   if(style=="HexCore"){
    Point Vertex(int index){double a=(270+index*60)*Math.PI/180;return new Point(size/2+r*Math.Cos(a),centerY+r*Math.Sin(a));}
    var path=new PathFigure{StartPoint=Vertex(0),IsClosed=false};double progress=Math.Clamp(ratio,0,1)*6;int completed=(int)Math.Floor(progress);for(int i=1;i<=completed;i++)path.Segments.Add(new LineSegment(Vertex(i),true));if(completed<6){var from=Vertex(completed);var to=Vertex(completed+1);double fraction=progress-completed;path.Segments.Add(new LineSegment(new Point(from.X+(to.X-from.X)*fraction,from.Y+(to.Y-from.Y)*fraction),true));}return new PathGeometry(new[]{path});
   }
   Point PointAt(double degrees){double radians=degrees*Math.PI/180;return new Point(size/2+r*Math.Cos(radians),centerY+r*Math.Sin(radians));}
   double degrees=sweep*ratio;var figure=new PathFigure{StartPoint=PointAt(start),IsClosed=false};figure.Segments.Add(new ArcSegment(PointAt(start+degrees),new Size(r,r),0,degrees>180,SweepDirection.Clockwise,true));return new PathGeometry(new[]{figure});
  }
 }
}
