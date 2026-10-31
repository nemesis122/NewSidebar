using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Deliberate instrument, history and typographic compositions; no hardware access.
public sealed class CraftedHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly List<(string Id,CraftedGraphic Graphic)> graphics=new();
 private readonly MetricHistory history;
 private readonly string deviceId;
 private readonly Brush edge;
 private readonly CraftedGraphic? aggregate;
 public CraftedHardwareCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool fan=Composition=="SignalFan",petals=Composition=="PetalMatrix",ranges=Composition=="RangeDeck",film=Composition=="ColumnFilm",tickets=Composition=="TicketRail",type=Composition=="TypeStack",rulers=Composition=="RulerTracks",iris=Composition=="ApertureGrid";
  var d=DesignCatalog.Resolve(p);double scale=p.FontScale;string accent=DesignCatalog.CategoryColor(d,device.Category,p),text=DesignCatalog.ColorOr(p.TextColor,d.Text),track=DesignCatalog.ColorOr(p.GraphTrackColor,d.Track);
  Resources["Text"]=Ui.Brush(text);Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,d.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,text));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  bool open=tickets||type||rulers;Padding=new Thickness(9,8,9,8);Margin=new Thickness(3,0,3,6);CornerRadius=new CornerRadius(fan||iris?17:petals?24:film?2:10);BorderThickness=new Thickness(1);edge=open?Brushes.Transparent:Ui.Brush(d.Edge);BorderBrush=edge;var back=Ui.Brush(d.Panel);back.Opacity=open?0:p.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(p,2);Child=body;var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,11*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);body.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,7);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);body.Children.Add(readings);
  if((fan||petals)&&device.Metrics.Any(m=>m.Unit!="IP")){aggregate=new CraftedGraphic(fan?"fan":"petals",accent,track,p);var figure=new GraphicHost(aggregate,new Size(fan?164:136,fan?96:122),Preferences.TextFactor(p.InstrumentScale)){HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,0,0,5)};readings.Children.Add(figure);}
  var fields=iris?new CellPanel(3,78*scale):null;if(fields!=null)readings.Children.Add(fields);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   double valueFont=(type?18:11)*scale*Preferences.TextFactor(p.ValueTextScale);
   var value=Ui.RoleText(SidebarTextRole.Readings,"—",valueFont);value.FontFamily=Ui.Font(p,type?"Bahnschrift, Segoe UI":"Cascadia Mono, Consolas");value.FontWeight=type?FontWeights.Normal:FontWeights.SemiBold;value.Foreground=Ui.ValueBrush(p,text);value.TextTrimming=TextTrimming.None;value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   var field=new Grid{Margin=new Thickness(1,1,1,tickets?4:3)};
   if(metric.Unit=="IP")
   {
    CellPanel.SetWide(field);field.Children.Add(new AddressReadoutPanel(label,value){Margin=tickets?new Thickness(12,5,12,5):new Thickness(3,rulers?3:1,3,rulers?3:1)});
   }
   else if(iris)
   {
    var stack=new StackPanel{VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(3)};label.TextAlignment=value.TextAlignment=TextAlignment.Center;stack.Children.Add(label);var art=new CraftedGraphic("iris",accent,track,p){Width=46,Height=46};graphics.Add((metric.Id,art));stack.Children.Add(new Viewbox{Child=art,Height=42*Preferences.TextFactor(p.InstrumentScale),Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,3,0,3)});stack.Children.Add(value);field.Children.Add(stack);
   }
   else if(tickets)
   {
    field.Children.Add(new CraftedGraphic("ticket",accent,track,p));field.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,valueFont){Margin=new Thickness(12,5,12,5)});
   }
   else
   {
    var stack=new StackPanel{Margin=new Thickness(3,0,3,0)};stack.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,valueFont){Margin=new Thickness(0,type?3:1,0,type?3:1)});
    if(ranges||film||rulers){var art=new CraftedGraphic(ranges?"range":film?"film":"ruler",accent,track,p){Height=(rulers?14:film?23:18)*Preferences.TextFactor(p.InstrumentScale),Margin=new Thickness(0,1,0,2)};graphics.Add((metric.Id,art));stack.Children.Add(art);}
    else if(type)stack.Children.Add(new Border{Height=1,Background=Ui.Brush(track),Margin=new Thickness(0,0,0,1)});
    field.Children.Add(stack);
   }
   if(fields==null)readings.Children.Add(field);else fields.Children.Add(field);
  }
  Update(device,p);
 }
 private static double? Ratio(Metric? metric)=>metric?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,metric.Unit is "%" or "°C"?100:double.IsFinite(metric.Maximum)?metric.Maximum:100),0,1):null;
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values)pair.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key)?.Display(p.Fahrenheit)??"—";
  foreach(var (id,art) in graphics){var metric=device.Metrics.FirstOrDefault(m=>m.Id==id);art.Ratio=Ratio(metric);art.Maximum=metric?.Unit is "%" or "°C"?100:metric?.Maximum??100;art.SetHistory(history.Read(deviceId,id),history.LatestTime,p.Interval);}
  if(aggregate!=null){aggregate.Ratios=device.Metrics.Where(m=>m.Unit!="IP").Take(8).Select(Ratio).ToArray();aggregate.InvalidateVisual();}
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class GraphicHost:Decorator
 {
  private readonly Size natural;private readonly double requested;private double scale;
  public GraphicHost(UIElement child,Size natural,double requested){Child=child;this.natural=natural;this.requested=requested;ClipToBounds=true;}
  protected override Size MeasureOverride(Size available){scale=Math.Min(requested,double.IsFinite(available.Width)?Math.Max(.001,available.Width/natural.Width):requested);Child.Measure(natural);return new Size(natural.Width*scale,natural.Height*scale);}
  protected override Size ArrangeOverride(Size size){Child.Arrange(new Rect(new Point(),natural));double fit=Math.Min(scale,Math.Max(.001,size.Width/natural.Width));Child.RenderTransform=new ScaleTransform(fit,fit);return size;}
 }
 private sealed class CellPanel:Panel
 {
  private readonly int maximum;private readonly double minimum;private int columns=1;private readonly List<(UIElement Child,int Row,int Col,int Span)> slots=new();private readonly List<double> heights=new();
  private static readonly DependencyProperty WideProperty=DependencyProperty.RegisterAttached("Wide",typeof(bool),typeof(CellPanel),new PropertyMetadata(false));public static void SetWide(DependencyObject child)=>child.SetValue(WideProperty,true);
  public CellPanel(int maximum,double minimum){this.maximum=maximum;this.minimum=minimum;}
  protected override Size MeasureOverride(Size available){double width=double.IsFinite(available.Width)?Math.Max(0,available.Width):300;columns=Math.Clamp((int)(width/Math.Max(1,minimum)),1,maximum);slots.Clear();heights.Clear();int row=0,col=0;foreach(UIElement child in InternalChildren){int span=(bool)child.GetValue(WideProperty)?columns:1;if(col+span>columns){row++;col=0;}while(heights.Count<=row)heights.Add(0);child.Measure(new Size(width*span/columns,double.PositiveInfinity));heights[row]=Math.Max(heights[row],child.DesiredSize.Height);slots.Add((child,row,col,span));col+=span;if(col==columns){row++;col=0;}}return new Size(width,heights.Sum());}
  protected override Size ArrangeOverride(Size final){double extra=heights.Count==0?0:Math.Max(0,final.Height-heights.Sum())/heights.Count;foreach(var s in slots)s.Child.Arrange(new Rect(final.Width*s.Col/columns,heights.Take(s.Row).Sum()+s.Row*extra,final.Width*s.Span/columns,heights[s.Row]+extra));return final;}
 }
 public sealed class CraftedGraphic:FrameworkElement
 {
  public string Mode{get;}public double? Ratio;public double?[] Ratios=Array.Empty<double?>();public double Maximum=100;
  public int SampleCount=>samples.Count;
  private IReadOnlyList<HistorySample> samples=Array.Empty<HistorySample>();private DateTime now;
  private readonly Brush ink,track,wash;private readonly double thickness;
  public CraftedGraphic(string mode,string accent,string track,Preferences p){Mode=mode;ink=Ui.Brush(accent);this.track=Ui.Brush(track);var fill=Ui.Brush(accent);fill.Opacity=.11*p.Opacity;wash=fill;thickness=DesignCatalog.GraphSize(p);IsHitTestVisible=false;ClipToBounds=true;}
  public void SetHistory(IReadOnlyList<HistorySample> points,DateTime time,int interval){samples=points;now=time;InvalidateVisual();}
  private static Geometry Polygon(IEnumerable<Point> points){var a=points.ToArray();var shape=new StreamGeometry();using(var c=shape.Open()){c.BeginFigure(a[0],true,true);foreach(var x in a.Skip(1))c.LineTo(x,true,false);}shape.Freeze();return shape;}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<3||h<3)return;var pen=new Pen(ink,1.5*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
   if(Mode=="ticket"){double notch=Math.Min(4,h/4);dc.DrawGeometry(wash,new Pen(track,1),Polygon(new[]{new Point(5,1),new Point(w-5,1),new Point(w-1,5),new Point(w-1,h/2-notch),new Point(w-5,h/2),new Point(w-1,h/2+notch),new Point(w-1,h-5),new Point(w-5,h-1),new Point(5,h-1),new Point(1,h-5),new Point(1,h/2+notch),new Point(5,h/2),new Point(1,h/2-notch),new Point(1,5)}));for(double y=7;y<h-5;y+=5)dc.DrawLine(new Pen(track,1),new Point(w-8,y),new Point(w-6,y));return;}
   if(Mode=="fan"){int count=Ratios.Length;if(count==0)return;double outer=Math.Min(w/2-5,h-10),step=Math.Min(7,outer/(count+2)),cy=h-5,cx=w/2;for(int i=0;i<count;i++){double r=outer-i*step;var arc=new StreamGeometry();using(var c=arc.Open()){c.BeginFigure(new Point(cx-r,cy),false,false);c.ArcTo(new Point(cx+r,cy),new Size(r,r),0,false,SweepDirection.Clockwise,true,false);}dc.DrawGeometry(null,new Pen(track,Math.Max(1,step*.45)),arc);if(Ratios[i] is double v){double angle=Math.PI+Math.PI*v;var fill=new StreamGeometry();using(var c=fill.Open()){c.BeginFigure(new Point(cx-r,cy),false,false);c.ArcTo(new Point(cx+r*Math.Cos(angle),cy+r*Math.Sin(angle)),new Size(r,r),0,false,SweepDirection.Clockwise,true,false);}dc.DrawGeometry(null,new Pen(ink,Math.Max(1,step*.45)*thickness),fill);}}return;}
   if(Mode=="petals"){int count=Ratios.Length;if(count==0)return;var center=new Point(w/2,h/2);double radius=Math.Min(w,h)/2-5;for(int i=0;i<count;i++){double a=2*Math.PI*i/count-Math.PI/2,spread=Math.Min(.5,2.3/count);Point At(double t,double r)=>new(center.X+Math.Cos(t)*r,center.Y+Math.Sin(t)*r);dc.DrawGeometry(wash,new Pen(track,1),Polygon(new[]{At(a-spread,8),At(a-spread*.75,radius*.73),At(a,radius),At(a+spread*.75,radius*.73),At(a+spread,8)}));if(Ratios[i] is double value){double r=9+(radius-9)*value;dc.DrawLine(new Pen(ink,3*thickness){EndLineCap=PenLineCap.Round},At(a,9),At(a,r));dc.DrawEllipse(ink,null,At(a,r),2,2);}}return;}
   if(Mode=="iris"){var center=new Point(w/2,h/2);double r=Math.Min(w,h)/2-3;for(int i=0;i<6;i++){double a=i*Math.PI/3-Math.PI/2;Point At(double angle,double radius)=>new(center.X+Math.Cos(angle)*radius,center.Y+Math.Sin(angle)*radius);dc.DrawGeometry(wash,new Pen(track,1),Polygon(new[]{At(a,r),At(a+Math.PI/3,r),At(a+Math.PI/2,r*.45),At(a+Math.PI/6,r*.18)}));}if(Ratio is double value){double hole=3+(r-3)*(1-value);dc.DrawEllipse(null,pen,center,hole,hole);}return;}
   double left=3,right=w-3;
   if(Mode=="ruler"){dc.DrawLine(new Pen(track,1),new Point(left,h*.5),new Point(right,h*.5));for(int i=0;i<=10;i++){double x=left+(right-left)*i/10;dc.DrawLine(new Pen(track,1),new Point(x,h*.25),new Point(x,h*(i%5==0?.85:.65)));}if(Ratio is double value){double x=left+(right-left)*value;dc.DrawGeometry(ink,null,Polygon(new[]{new Point(x,1),new Point(x-3,h*.4),new Point(x+3,h*.4)}));}return;}
   var valid=samples.Where(s=>s.Value is double v&&double.IsFinite(v)&&(now-s.Time).TotalSeconds>=0&&(now-s.Time).TotalSeconds<=60).ToArray();
   dc.DrawLine(new Pen(track,1),new Point(left,h-2),new Point(right,h-2));if(valid.Length==0)return;
   if(Mode=="range"){double minimum=valid.Min(s=>s.Value!.Value),maximum=valid.Max(s=>s.Value!.Value),ceiling=Math.Max(1,double.IsFinite(Maximum)?Maximum:100);double x1=left+(right-left)*Math.Clamp(minimum/ceiling,0,1),x2=left+(right-left)*Math.Clamp(maximum/ceiling,0,1),y=h*.5;dc.DrawLine(new Pen(track,2),new Point(left,y),new Point(right,y));dc.DrawLine(new Pen(ink,5*thickness),new Point(x1,y),new Point(Math.Max(x1+.5,x2),y));if(Ratio is double value)dc.DrawEllipse(ink,null,new Point(left+(right-left)*value,y),2.5*thickness,2.5*thickness);return;}
   if(Mode=="film"){double max=Math.Max(1,valid.Max(s=>Math.Max(0,s.Value!.Value)));foreach(var sample in valid){double age=(now-sample.Time).TotalSeconds,x=right-(right-left)*age/60,height=(h-4)*Math.Clamp(sample.Value!.Value/max,0,1);dc.DrawRectangle(ink,null,new Rect(Math.Max(left,x-1),h-2-height,Math.Max(.6,Math.Min(2*thickness,right-left)),height));}return;}
  }
 }
}
