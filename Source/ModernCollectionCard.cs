using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Structural alternatives using only selected metrics and the existing shared history.
public sealed class ModernCollectionCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly List<(string Id,SensorArt Art)> meters=new();
 private readonly List<(Metric Metric,HistoryAreaGraph Graph)> plots=new();
 private readonly MetricHistory history;
 private readonly string deviceId;
 private readonly Brush edge;
 private readonly RadarArt? radar;
 public ModernCollectionCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool honey=Composition=="Honeycomb",layers=Composition=="LayerStack",faders=Composition=="FaderBank",flaps=Composition=="FlipBoard",spine=Composition=="DataSpine",lines=Composition=="LineDashboard",pixels=Composition=="PixelLedger",radarMode=Composition=="RadarMesh";
  var d=DesignCatalog.Resolve(p);double scale=p.FontScale;
  string accent=DesignCatalog.CategoryColor(d,device.Category,p),text=DesignCatalog.ColorOr(p.TextColor,d.Text),track=DesignCatalog.ColorOr(p.GraphTrackColor,d.Track);
  Resources["Text"]=Ui.Brush(text);Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,d.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,text));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(9,8,9,8);Margin=new Thickness(3,0,3,6);CornerRadius=new CornerRadius(layers?2:radarMode?20:flaps?4:12);BorderThickness=new Thickness(1);edge=layers||spine||pixels?Brushes.Transparent:Ui.Brush(d.Edge);BorderBrush=edge;var back=Ui.Brush(d.Panel);back.Opacity=layers||spine||pixels?0:p.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(p,2);Child=body;var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,11*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(new Viewbox{Child=header,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,7);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);body.Children.Add(readings);
  if(radarMode){radar=new RadarArt(accent,track,DesignCatalog.GraphSize(p));var chart=new ScaledFigure(radar,new Size(146,104),Preferences.TextFactor(p.InstrumentScale)){HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,0,0,5)};readings.Children.Add(chart);}
  var fields=honey||faders||radarMode?new FieldPanel(faders?4:3,(faders?62:honey?78:110)*scale):null;
  if(fields!=null)readings.Children.Add(fields);
  int index=0;
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,(flaps?9:10)*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   var value=Ui.RoleText(SidebarTextRole.Readings,"—",(flaps?17:11)*scale*Preferences.TextFactor(p.ValueTextScale));value.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(p,text);value.FontWeight=FontWeights.SemiBold;value.TextTrimming=TextTrimming.None;value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   var field=new Grid{Margin=new Thickness(1,1,1,3)};
   if(metric.Unit=="IP")
   {
    FieldPanel.SetWide(field);
    if(pixels){field.ColumnDefinitions.Add(new(){Width=new GridLength(31*Preferences.TextFactor(p.InstrumentScale))});field.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var address=new AddressReadoutPanel(label,value){Margin=new Thickness(3,4,0,4)};Grid.SetColumn(address,1);field.Children.Add(address);}
    else if(layers){field.Margin=new Thickness(index%3*3,1,(2-index%3)*3,2);field.Children.Add(new SensorArt("layer",accent,track,DesignCatalog.GraphSize(p),p.Opacity));field.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(12,6,12,6)});}
    else field.Children.Add(new AddressReadoutPanel(label,value){Margin=lines?new Thickness(2,0,2,2):new Thickness(3,2,3,2)});
   }
   else if(honey)
   {
    var art=new SensorArt("hex",accent,track,DesignCatalog.GraphSize(p),p.Opacity);field.Children.Add(art);meters.Add((metric.Id,art));
    var texts=new StackPanel{VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(10,12,10,12)};label.TextAlignment=value.TextAlignment=TextAlignment.Center;texts.Children.Add(label);texts.Children.Add(value);field.Children.Add(texts);field.MinHeight=66*Preferences.TextFactor(p.InstrumentScale);
   }
   else if(faders)
   {
    var stack=new StackPanel{Margin=new Thickness(2,3,2,3),VerticalAlignment=VerticalAlignment.Center};label.TextAlignment=value.TextAlignment=TextAlignment.Center;stack.Children.Add(label);
    var art=new SensorArt("fader",accent,track,DesignCatalog.GraphSize(p),p.Opacity){Width=20,Height=52};meters.Add((metric.Id,art));stack.Children.Add(new Viewbox{Child=art,Height=44*Preferences.TextFactor(p.InstrumentScale),Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,3,0,3)});stack.Children.Add(value);field.Children.Add(stack);
   }
   else if(flaps)
   {
    field.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});field.ColumnDefinitions.Add(new(){Width=new GridLength(1.55,GridUnitType.Star)});label.VerticalAlignment=VerticalAlignment.Center;field.Children.Add(label);
    var slab=new Grid{Margin=new Thickness(7,0,0,0)};slab.Children.Add(new SensorArt("flap",accent,track,DesignCatalog.GraphSize(p),p.Opacity));value.TextAlignment=TextAlignment.Right;value.Margin=new Thickness(7,5,7,5);slab.Children.Add(value);Grid.SetColumn(slab,1);field.Children.Add(slab);
   }
   else if(spine)
   {
    field.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});field.ColumnDefinitions.Add(new(){Width=new GridLength(14)});field.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
    var spineArt=new SensorArt("spine",accent,track,DesignCatalog.GraphSize(p),p.Opacity);Grid.SetColumn(spineArt,1);field.Children.Add(spineArt);
    var data=new StackPanel{Margin=new Thickness(3,2,3,3)};label.TextAlignment=value.TextAlignment=TextAlignment.Right;data.Children.Add(label);data.Children.Add(value);Grid.SetColumn(data,index%2==0?0:2);field.Children.Add(data);
    var art=new SensorArt("line",accent,track,DesignCatalog.GraphSize(p),p.Opacity){Margin=new Thickness(5,9,5,9)};meters.Add((metric.Id,art));Grid.SetColumn(art,index%2==0?2:0);field.Children.Add(art);
   }
   else if(lines)
   {
    var stack=new StackPanel{Margin=new Thickness(2,0,2,2)};stack.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)));
    var chart=new HistoryAreaGraph(accent,track,DesignCatalog.GraphSize(p)){Height=15*Preferences.TextFactor(p.InstrumentScale),Margin=new Thickness(0,1,0,2)};plots.Add((metric,chart));stack.Children.Add(chart);field.Children.Add(stack);
   }
   else if(pixels)
   {
    field.ColumnDefinitions.Add(new(){Width=new GridLength(31*Preferences.TextFactor(p.InstrumentScale))});field.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
    var art=new SensorArt("pixels",accent,track,DesignCatalog.GraphSize(p),p.Opacity){Margin=new Thickness(0,4,5,4)};meters.Add((metric.Id,art));field.Children.Add(art);
    var row=new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(3,4,0,4)};Grid.SetColumn(row,1);field.Children.Add(row);
   }
   else if(layers)
   {
    field.Margin=new Thickness(index%3*3,1,(2-index%3)*3,2);field.Children.Add(new SensorArt("layer",accent,track,DesignCatalog.GraphSize(p),p.Opacity));
    field.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(12,6,12,6)});
   }
   else
   {
    var cell=new Border{Background=Tint(accent,.055*p.Opacity),Padding=new Thickness(6,3,6,3),CornerRadius=new CornerRadius(4)};var stack=new StackPanel();stack.Children.Add(label);value.TextAlignment=TextAlignment.Right;stack.Children.Add(value);cell.Child=stack;field.Children.Add(cell);
   }
   if(fields==null)readings.Children.Add(field);else fields.Children.Add(field);index++;
  }
  Update(device,p);
 }
 private static Brush Tint(string color,double opacity){var b=Ui.Brush(color);b.Opacity=opacity;return b;}
 private static double? Ratio(Metric? m)=>m?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,m.Unit is "%" or "°C"?100:double.IsFinite(m.Maximum)?m.Maximum:100),0,1):null;
 public void Update(Device device,Preferences p)
 {
  foreach(var value in values)value.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==value.Key)?.Display(p.Fahrenheit)??"—";
  foreach(var (id,art) in meters){art.Ratio=Ratio(device.Metrics.FirstOrDefault(m=>m.Id==id));art.InvalidateVisual();}
  foreach(var (metric,chart) in plots)chart.SetSamples(history.Read(deviceId,metric.Id),history.LatestTime,metric.Unit=="%"?100:metric.Unit=="°C"?Math.Max(100,metric.Maximum):0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  if(radar!=null){radar.Values=device.Metrics.Where(m=>m.Unit!="IP").Take(8).Select(Ratio).ToArray();radar.InvalidateVisual();}
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class ScaledFigure:Decorator
 {
  private readonly Size natural;private readonly double requested;private double scale;
  public ScaledFigure(UIElement child,Size natural,double requested){Child=child;this.natural=natural;this.requested=requested;ClipToBounds=true;}
  protected override Size MeasureOverride(Size available){scale=Math.Min(requested,double.IsFinite(available.Width)?Math.Max(.001,available.Width/natural.Width):requested);Child.Measure(natural);return new Size(natural.Width*scale,natural.Height*scale);}
  protected override Size ArrangeOverride(Size size){Child.Arrange(new Rect(new Point(),natural));double fit=Math.Min(scale,Math.Max(.001,size.Width/natural.Width));Child.RenderTransform=new ScaleTransform(fit,fit);return size;}
 }
 private sealed class FieldPanel:Panel
 {
  private readonly int maximum;private readonly double minimum;private int columns=1;private readonly List<(UIElement Child,int Row,int Column,int Span)> slots=new();private readonly List<double> heights=new();
  private static readonly DependencyProperty WideProperty=DependencyProperty.RegisterAttached("Wide",typeof(bool),typeof(FieldPanel),new PropertyMetadata(false));
  public static void SetWide(DependencyObject child)=>child.SetValue(WideProperty,true);
  public FieldPanel(int maximum,double minimum){this.maximum=maximum;this.minimum=minimum;}
  protected override Size MeasureOverride(Size available){double width=double.IsFinite(available.Width)?Math.Max(0,available.Width):300;columns=Math.Clamp((int)(width/Math.Max(1,minimum)),1,maximum);slots.Clear();heights.Clear();int row=0,col=0;foreach(UIElement child in InternalChildren){int span=(bool)child.GetValue(WideProperty)?columns:1;if(col+span>columns){row++;col=0;}while(heights.Count<=row)heights.Add(0);child.Measure(new Size(width*span/columns,double.PositiveInfinity));heights[row]=Math.Max(heights[row],child.DesiredSize.Height);slots.Add((child,row,col,span));col+=span;if(col==columns){row++;col=0;}}return new Size(width,heights.Sum());}
  protected override Size ArrangeOverride(Size final){double extra=heights.Count==0?0:Math.Max(0,final.Height-heights.Sum())/heights.Count;foreach(var s in slots)s.Child.Arrange(new Rect(final.Width*s.Column/columns,heights.Take(s.Row).Sum()+s.Row*extra,final.Width*s.Span/columns,heights[s.Row]+extra));return final;}
 }
 private static Geometry Polygon(IEnumerable<Point> points){var all=points.ToArray();var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(all[0],true,true);foreach(var point in all.Skip(1))c.LineTo(point,true,false);}path.Freeze();return path;}
 private sealed class RadarArt:FrameworkElement
 {
  public double?[] Values=Array.Empty<double?>();private readonly Brush ink,track,fill;private readonly double thickness;
  public RadarArt(string accent,string track,double thickness){ink=Ui.Brush(accent);this.track=Ui.Brush(track);fill=Tint(accent,.13);this.thickness=thickness;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){int count=Values.Length;if(count<3)return;double r=Math.Max(1,Math.Min(ActualWidth,ActualHeight)/2-6);var center=new Point(ActualWidth/2,ActualHeight/2);Point At(int i,double ratio)=>new(center.X+Math.Sin(i*2*Math.PI/count)*r*ratio,center.Y-Math.Cos(i*2*Math.PI/count)*r*ratio);for(int level=1;level<=3;level++)dc.DrawGeometry(null,new Pen(track,.7),Polygon(Enumerable.Range(0,count).Select(i=>At(i,level/3d))));for(int i=0;i<count;i++)dc.DrawLine(new Pen(track,.7),center,At(i,1));if(Values.All(v=>v.HasValue))dc.DrawGeometry(fill,new Pen(ink,1.5*thickness),Polygon(Enumerable.Range(0,count).Select(i=>At(i,Values[i]!.Value))));for(int i=0;i<count;i++)if(Values[i] is double value)dc.DrawEllipse(ink,null,At(i,value),2*thickness,2*thickness);}
 }
 private sealed class SensorArt:FrameworkElement
 {
  public double? Ratio;private readonly string mode;private readonly Brush ink,track,wash;private readonly double thickness;
  public SensorArt(string mode,string accent,string track,double thickness,double opacity){this.mode=mode;ink=Ui.Brush(accent);this.track=Ui.Brush(track);wash=Tint(accent,(mode=="layer"?.1:.055)*opacity);this.thickness=thickness;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<2||h<2)return;
   if(mode=="hex"){var path=Polygon(new[]{new Point(w*.18,1),new Point(w*.82,1),new Point(w-1,h/2),new Point(w*.82,h-1),new Point(w*.18,h-1),new Point(1,h/2)});dc.DrawGeometry(wash,new Pen(track,1),path);if(Ratio is double ratio)dc.DrawLine(new Pen(ink,2*thickness),new Point(w*.25,h-8),new Point(w*.25+w*.5*ratio,h-8));return;}
   if(mode=="layer"){dc.DrawGeometry(wash,new Pen(track,1),Polygon(new[]{new Point(8,1),new Point(w-1,1),new Point(w-8,h-1),new Point(1,h-1)}));dc.DrawLine(new Pen(ink,2*thickness),new Point(8,2),new Point(Math.Min(w-2,23),2));return;}
   if(mode=="flap"){dc.DrawRoundedRectangle(wash,new Pen(track,1),new Rect(.5,.5,w-1,h-1),3,3);dc.DrawLine(new Pen(track,1),new Point(1,h/2),new Point(w-1,h/2));dc.DrawRectangle(track,null,new Rect(0,h/2-2,3,4));dc.DrawRectangle(track,null,new Rect(w-3,h/2-2,3,4));return;}
   if(mode=="spine"){dc.DrawLine(new Pen(track,1),new Point(w/2,0),new Point(w/2,h));dc.DrawEllipse(ink,null,new Point(w/2,h/2),2.5,2.5);return;}
   if(mode=="fader"){double center=w/2;dc.DrawLine(new Pen(track,3),new Point(center,4),new Point(center,h-4));for(int i=0;i<6;i++)dc.DrawLine(new Pen(track,1),new Point(1,4+(h-8)*i/5),new Point(5,4+(h-8)*i/5));if(Ratio is double value){double y=h-4-(h-8)*value;dc.DrawLine(new Pen(ink,2*thickness),new Point(center,h-4),new Point(center,y));dc.DrawRoundedRectangle(ink,null,new Rect(center-6,y-2.5,12,5),1.5,1.5);}return;}
   if(mode=="pixels"){int count=18;double cellW=w/6,cellH=h/3;for(int i=0;i<count;i++)dc.DrawRoundedRectangle(Ratio.HasValue&&Ratio.Value>i/(double)count?ink:track,null,new Rect(i%6*cellW+1,(2-i/6)*cellH+1,Math.Max(.5,cellW-2),Math.Max(.5,cellH-2)),.7,.7);return;}
   dc.DrawLine(new Pen(track,2),new Point(0,h/2),new Point(w,h/2));if(Ratio is double fill)dc.DrawLine(new Pen(ink,2*thickness),new Point(0,h/2),new Point(w*fill,h/2));
  }
 }
}
