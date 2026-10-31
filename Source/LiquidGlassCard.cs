using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// The glass family. Every variant shares one construction: a translucent slab, a rim
// that catches light at the top edge, a sheen across the upper third, and each reading
// as a glass capsule. Variants differ only in material (frost, tint, smoke, prism) and
// in capsule form (liquid fill or slim tube). Download and upload get a live 60-second
// trace inside their capsule; text readings such as link speed stay plain rows.
public sealed record GlassStyle(string Key,double Lift=.10,byte Top=235,byte Bottom=200,double Radius=16,byte Sheen=34,double SheenHeight=46,
 byte RimTop=110,byte RimBottom=18,double Tint=0,double Smoke=0,bool Prism=false,bool Tubes=false,byte LiquidA=120,byte LiquidB=150,double TrackLift=0,double PillHeight=24)
{
 public static readonly GlassStyle[] All={
  new("LiquidGlass"),
  new("FrostGlass",Lift:.22,Top:225,Bottom:190,Radius:20,Sheen:52,SheenHeight:62,RimTop:160,RimBottom:44,LiquidA:150,LiquidB:175,TrackLift:.14,PillHeight:26),
  new("AquaGlass",Tint:.17,RimTop:120,RimBottom:24,LiquidA:135,LiquidB:165),
  new("PrismGlass",Radius:18,Sheen:28,Prism:true,RimTop:190,RimBottom:70,LiquidA:130,LiquidB:170),
  new("SmokeGlass",Lift:.04,Smoke:.45,Top:242,Bottom:225,Radius:14,Sheen:16,SheenHeight:38,RimTop:70,RimBottom:10,LiquidA:175,LiquidB:215),
  new("GlassTubes",Radius:16,Tubes:true,PillHeight:7)
 };
 public static GlassStyle Get(string key)=>All.FirstOrDefault(s=>s.Key==key)??All[0];
}

public sealed class LiquidGlassCard:ModernCardBase
{
 private readonly GlassStyle style;
 private readonly MetricHistory history;
 private readonly TextBlock? number,unit;
 private readonly string? heroId;
 private readonly string second;
 private readonly bool light;
 private readonly Color rimTop,rimBottom;
 private readonly List<(string Id,Capsule Pill,bool Level)> pills=new();
 private readonly HashSet<string> links=new();
 private readonly List<(string Id,TraceCapsule Trace)> traces=new();
 public LiquidGlassCard(Device device,Preferences p,MetricHistory history):this(device,p,history,GlassStyle.All[0]){}
 public LiquidGlassCard(Device device,Preferences p,MetricHistory history,GlassStyle style):base(device,p)
 {
  this.style=style;this.history=history;
  // Prism splits light into the accent and a neighbouring hue unless the user chose a secondary colour.
  second=style.Prism&&Secondary==Accent?HueShift(Accent,42):Secondary;
  var panel=C(D.Panel);var accent=C(Accent);double alpha=Math.Clamp(p.Opacity,0,1);
  // Light themes (Light, Arctic): a white rim vanishes on white glass and dark smoke would
  // hide dark text, so the rim takes the theme edge and smoke becomes a light grey tint.
  light=Luminance(panel)>.55;
  if(style.Tint>0)panel=Mix(panel,accent,style.Tint);if(style.Smoke>0)panel=Mix(panel,Colors.Black,light?.10:style.Smoke);
  var edge=C(Edge);var tintRim=style.Tint>0||style.Prism?accent:light?edge:Colors.White;
  rimTop=Color.FromArgb(light?(byte)230:style.RimTop,tintRim.R,tintRim.G,tintRim.B);rimBottom=Color.FromArgb(light?(byte)120:style.RimBottom,(light?edge:tintRim).R,(light?edge:tintRim).G,(light?edge:tintRim).B);
  var top=Mix(panel,Colors.White,style.Lift);
  CornerRadius=new CornerRadius(style.Radius);Margin=new Thickness(3,0,3,8);BorderThickness=new Thickness(style.Prism?1.4:1.2);
  Background=new LinearGradientBrush(Color.FromArgb((byte)(style.Top*alpha),top.R,top.G,top.B),Color.FromArgb((byte)(style.Bottom*alpha),panel.R,panel.G,panel.B),new Point(0,0),new Point(0,1));
  var root=new Grid();Child=root;
  // The sheen belongs to the glass surface, so it fades with "Widget-Hintergrund". Its type name
  // lets the shared HW Sensors frame lift one sheen onto its perimeter instead of one per section.
  byte sheen=(byte)(style.Sheen*alpha);
  root.Children.Add(new FrameDecoration{CornerRadius=new CornerRadius(Math.Max(0,style.Radius-1),Math.Max(0,style.Radius-1),40,40),VerticalAlignment=VerticalAlignment.Top,Height=style.SheenHeight,IsHitTestVisible=false,
   Background=new LinearGradientBrush(Color.FromArgb(sheen,255,255,255),Color.FromArgb(0,255,255,255),new Point(0,0),style.Prism?new Point(.7,1):new Point(0,1))});
  var body=new ExpandedReadingsPanel(p,1){Margin=new Thickness(14,12,14,13)};root.Children.Add(body);
  body.Children.Add(Header(device,p));
  var content=new GrowPanel();body.Children.Add(content);
  bool network=device.Category=="Network";
  var hero=network?null:HeroMetric(device);
  if(hero!=null&&Plottable(hero))
  {
   heroId=hero.Id;(number,unit)=HeroNumber(p,30);
   var line=new StackPanel{Orientation=Orientation.Horizontal};line.Children.Add(number);line.Children.Add(unit);
   // The big number leads; its own capsule right below names it and shows its level.
   content.Children.Add(new Viewbox{Child=line,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,9)});
  }
  // Grafikgröße scales capsule and trace size; Linien-/Balkenstärke scales bar thickness and
  // line widths. "Text unter dem Graphen" moves each reading row beneath its capsule.
  double scale=S*Preferences.TextFactor(p.InstrumentScale),stroke=DesignCatalog.GraphSize(p);
  bool below=CardLayouts.ReadingsBelow(p);
  double textRow=Math.Max(10.5*F(p.LabelTextScale),12*F(p.ValueTextScale))*S*1.45;
  foreach(var m in device.Metrics)
  {
   // Throughput (network DL/UL, drive read/write) has no fixed maximum: it gets a live trace.
   // Totals and capacities would always be a full bar, and network text values have no level.
   bool traced=m.Unit is "MB/s" or "B/s";
   // Totals, capacities and link speed sit in the same glass capsule but without a level:
   // a fill there would always be full. Only text values such as IP addresses stay plain rows.
   bool level=!(network&&!traced)&&m.Label is not ("Gesamt" or "Kapazität");
   // Link speed: the capsule shows how much of the link download + upload use right now.
   if(network&&m.Unit is "Gbit/s" or "Mbit/s")links.Add(m.Id);
   if(!Plottable(m)){content.Children.Add(new AddressReadoutPanel(Label(m.Label,p),Value(m,p,11.5)){Margin=new Thickness(4,3,4,3)});continue;}
   FrameworkElement glass;
   bool separate=style.Tubes||below;
   if(traced){var trace=new TraceCapsule(D.Track,Accent,second,style,stroke,separate,light);traces.Add((m.Id,trace));glass=trace;}
   else{var pill=new Capsule(D.Track,Accent,second,style,stroke,light);pills.Add((m.Id,pill,level));glass=pill;}
   if(separate)
   {
    // Reading row and glass on separate lines: tubes put the row above, the text-below option beneath.
    glass.MinHeight=traced?Math.Max(10,22*scale):Math.Max(3,(style.Tubes?style.PillHeight:style.PillHeight*.5)*S*stroke);
    // Bars keep exactly the set thickness in every card; only traces take spare height.
    var cell=traced?GrowPanel.Grow(new Grid{Margin=new Thickness(0,1,0,7)},1,2.2):new Grid{Margin=new Thickness(0,1,0,7)};
    var auto=new RowDefinition{Height=GridLength.Auto};var star=new RowDefinition{Height=new GridLength(1,GridUnitType.Star)};
    var row=new ReadingRow(Label(m.Label,p,10.5),Value(m,p,12)){Margin=new Thickness(2,below?4:0,2,below?0:4)};
    if(below){cell.RowDefinitions.Add(star);cell.RowDefinitions.Add(auto);Grid.SetRow(row,1);}else{cell.RowDefinitions.Add(auto);cell.RowDefinitions.Add(star);Grid.SetRow(glass,1);}
    cell.Children.Add(row);cell.Children.Add(glass);content.Children.Add(cell);
   }
   else
   {
    // Text sits inside the capsule, so it never gets thinner than one line of text.
    glass.MinHeight=traced?Math.Max(textRow*1.9,style.PillHeight*1.6*scale):Math.Max(textRow,style.PillHeight*scale*stroke);
    var cell=traced?GrowPanel.Grow(new Grid{Margin=new Thickness(0,0,0,5)},1,2.4):new Grid{Margin=new Thickness(0,0,0,5)};cell.Children.Add(glass);
    var row=new ReadingRow(Label(m.Label,p,10.5),Value(m,p,12)){Margin=new Thickness(11,0,11,0),VerticalAlignment=traced?VerticalAlignment.Top:VerticalAlignment.Center};
    if(traced)row.Margin=new Thickness(12,5,12,0);
    cell.Children.Add(row);content.Children.Add(cell);
   }
  }
  Update(device,p);
 }
 // Share of the link in use: (download + upload) against the link speed, both in MB/s.
 private static double? LinkUse(Device device,Metric? link)
 {
  if(link?.Value is not double speed||!double.IsFinite(speed)||speed<=0)return null;
  double capacity=speed*(link.Unit=="Gbit/s"?125:.125);
  double used=device.Metrics.Where(m=>m.Unit=="MB/s"&&m.Value is double v&&double.IsFinite(v)).Sum(m=>Math.Max(0,m.Value!.Value))
   +device.Metrics.Where(m=>m.Unit=="B/s"&&m.Value is double v&&double.IsFinite(v)).Sum(m=>Math.Max(0,m.Value!.Value)/1048576d);
  return Math.Clamp(used/capacity,0,1);
 }
 private static double Luminance(Color c)=>(.2126*c.R+.7152*c.G+.0722*c.B)/255;
 public sealed class FrameDecoration:Border{}
 private static string HueShift(string hex,double degrees)
 {
  var c=C(hex);double r=c.R/255d,g=c.G/255d,b=c.B/255d,max=Math.Max(r,Math.Max(g,b)),min=Math.Min(r,Math.Min(g,b)),l=(max+min)/2,s=0,h=0,d=max-min;
  if(d>0){s=l>.5?d/(2-max-min):d/(max+min);h=max==r?(g-b)/d+(g<b?6:0):max==g?(b-r)/d+2:(r-g)/d+4;h*=60;}
  h=(h+degrees)%360;double q=l<.5?l*(1+s):l+s-l*s,pp=2*l-q;
  double Ch(double t){t=(t%1+1)%1;return t<1/6d?pp+(q-pp)*6*t:t<.5?q:t<2/3d?pp+(q-pp)*(2/3d-t)*6:pp;}
  byte B(double v)=>(byte)Math.Round(Math.Clamp(v,0,1)*255);
  return s==0?hex:$"#{B(Ch(h/360+1/3d)):X2}{B(Ch(h/360)):X2}{B(Ch(h/360-1/3d)):X2}";
 }
 public override void Update(Device device,Preferences p)
 {
  UpdateValues(device,p);
  bool hot=device.Metrics.Any(m=>Warn(m,p));
  var a=C(Accent);var b=C(second);
  var white=light?C(Edge):Colors.White;
  BorderBrush=hot?Ui.Brush(WarningColor):style.Prism
   ?new LinearGradientBrush(new GradientStopCollection{new GradientStop(Color.FromArgb(style.RimTop,a.R,a.G,a.B),0),new GradientStop(Color.FromArgb(style.RimTop,white.R,white.G,white.B),.35),new GradientStop(Color.FromArgb(style.RimBottom,b.R,b.G,b.B),1)},new Point(0,0),new Point(1,1))
   :new LinearGradientBrush(rimTop,rimBottom,new Point(0,0),new Point(0,1));
  SetHero(number,unit,device.Metrics.FirstOrDefault(m=>m.Id==heroId),p);
  foreach(var (id,pill,level) in pills){var m=device.Metrics.FirstOrDefault(x=>x.Id==id);pill.Exact=links.Contains(id);pill.Set(links.Contains(id)?LinkUse(device,m):level?Ratio(m):null,m!=null&&Warn(m,p));}
  foreach(var (id,trace) in traces)trace.Set(history.Read(DeviceId,id),history.LatestTime,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
 }
 private static void DrawGlassTrack(DrawingContext dc,Rect shape,double r,Color track,GlassStyle style,bool light)
 {
  var t=style.TrackLift>0?Mix(track,Colors.White,style.TrackLift):track;
  var hairline=light?Color.FromArgb(40,0,0,0):Color.FromArgb(28,255,255,255);
  dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb((byte)(style.TrackLift>0?170:150),t.R,t.G,t.B)),r>=3?new Pen(new SolidColorBrush(hairline),1):null,shape,r,r);
 }
 // Liquid fill to the reading's level with a bright meniscus at its leading edge.
 public sealed class Capsule:FrameworkElement
 {
  private readonly Color track,start,end,warn=C(WarningColor);private readonly GlassStyle style;private readonly double stroke;private readonly bool light;private double? ratio;private bool hot;
  public Capsule(string track,string accent,string secondary,GlassStyle style,double stroke,bool light){this.track=C(track);start=C(accent);end=C(secondary);this.style=style;this.stroke=stroke;this.light=light;IsHitTestVisible=false;}
  // Exact: the fill shows the true share without a minimum stub (link use is often near zero).
  public bool Exact {get;set;}
  public void Set(double? r,bool w){ratio=r;hot=w;InvalidateVisual();}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<=0||h<=0)return;double r=h/2;var shape=new Rect(0,0,w,h);
   DrawGlassTrack(dc,shape,r,track,style,light);
   if(ratio is not double v||v<=0)return;
   var clip=new RectangleGeometry(shape,r,r);clip.Freeze();dc.PushClip(clip);
   var a=hot?warn:start;var b=hot?warn:end;
   var liquid=new LinearGradientBrush(Color.FromArgb(style.LiquidA,a.R,a.G,a.B),Color.FromArgb(style.LiquidB,b.R,b.G,b.B),new Point(0,0),new Point(1,0));
   double fillWidth=Exact?w*v:Math.Max(h*.6,w*v);dc.DrawRectangle(liquid,null,new Rect(0,0,fillWidth,h));
   // Meniscus: a bright edge at the liquid level that fades out across the middle, where the
   // reading text sits, so it never cuts through digits.
   double meniscus=2*stroke;
   if(h>=10&&fillWidth>=meniscus){var edge=Color.FromArgb(210,a.R,a.G,a.B);var clear=Color.FromArgb(0,a.R,a.G,a.B);
    var fade=new LinearGradientBrush(new GradientStopCollection{new GradientStop(edge,0),new GradientStop(edge,.22),new GradientStop(clear,.36),new GradientStop(clear,.64),new GradientStop(edge,.78),new GradientStop(edge,1)},new Point(0,0),new Point(0,1));fade.Freeze();
    dc.DrawRectangle(fade,null,new Rect(Math.Max(0,fillWidth-meniscus),0,meniscus,h));}
   // Smoke glass: the liquid glows, a soft lighter core along its upper half.
   if(style.Smoke>0)dc.DrawRectangle(new LinearGradientBrush(Color.FromArgb(70,255,255,255),Color.FromArgb(0,255,255,255),new Point(0,0),new Point(0,.6)),null,new Rect(0,0,fillWidth,h));
   dc.Pop();
  }
 }
 // A live 60-second throughput trace held inside a glass capsule, scaled to its own peak.
 public sealed class TraceCapsule:FrameworkElement
 {
  private readonly Color track,start,end;private readonly GlassStyle style;private readonly double stroke;private readonly bool full,light;
  private IReadOnlyList<HistorySample> samples=Array.Empty<HistorySample>();private DateTime now;private TimeSpan gap;
  // full: the reading row sits outside, so the trace may use the whole capsule height.
  public TraceCapsule(string track,string accent,string secondary,GlassStyle style,double stroke,bool full,bool light){this.track=C(track);start=C(accent);end=C(secondary);this.style=style;this.stroke=stroke;this.full=full;this.light=light;IsHitTestVisible=false;}
  public void Set(IReadOnlyList<HistorySample> s,DateTime now,TimeSpan gap){samples=s;this.now=now;this.gap=gap;InvalidateVisual();}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<=4||h<=4)return;double r=Math.Min(h/2,style.Tubes?h/2:12);var shape=new Rect(0,0,w,h);
   DrawGlassTrack(dc,shape,r,track,style,light);
   var clip=new RectangleGeometry(shape,r,r);clip.Freeze();dc.PushClip(clip);
   // A small floor keeps idle noise from filling the capsule.
   double peak=Math.Max(.05,samples.Where(s=>s.Value is double v&&double.IsFinite(v)).Select(s=>s.Value!.Value).DefaultIfEmpty(0).Max()*1.15);
   double top=full?1+stroke:h*.42,bottom=h;// with text inside, the upper part stays clear for it
   var segments=new List<List<Point>>();var points=new List<Point>();DateTime? previous=null;
   foreach(var s in samples)
   {
    double age=(now-s.Time).TotalSeconds;if(age<0||age>60)continue;
    if(s.Value is not double v||!double.IsFinite(v)){if(points.Count>0)segments.Add(points);points=new();previous=null;continue;}
    if(previous.HasValue&&s.Time-previous.Value>gap&&points.Count>0){segments.Add(points);points=new();}
    points.Add(new Point(w*(1-age/60),bottom-(bottom-top)*Math.Clamp(Math.Max(0,v)/peak,0,1)));previous=s.Time;
   }
   if(points.Count>0)segments.Add(points);
   var fill=new LinearGradientBrush(Color.FromArgb(style.LiquidB,start.R,start.G,start.B),Color.FromArgb((byte)(style.LiquidA/4),end.R,end.G,end.B),new Point(0,0),new Point(0,1));fill.Freeze();
   var pen=new Pen(new SolidColorBrush(Color.FromArgb(230,start.R,start.G,start.B)),1.5*stroke){LineJoin=PenLineJoin.Round};pen.Freeze();
   foreach(var seg in segments)
   {
    if(seg.Count<2)continue;
    var area=new StreamGeometry();using(var c=area.Open()){c.BeginFigure(new Point(seg[0].X,h),true,true);foreach(var pt in seg)c.LineTo(pt,true,true);c.LineTo(new Point(seg[^1].X,h),true,false);}area.Freeze();dc.DrawGeometry(fill,null,area);
    var line=new StreamGeometry();using(var c=line.Open()){c.BeginFigure(seg[0],false,false);for(int i=1;i<seg.Count;i++)c.LineTo(seg[i],true,true);}line.Freeze();dc.DrawGeometry(null,pen,line);
   }
   dc.Pop();
  }
 }
}
