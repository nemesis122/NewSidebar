using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Native, per-reading compositions. Sensor acquisition remains outside the view.
public sealed class LabHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly List<(string Id,StudioGraphic Art)> graphics=new();
 private readonly MetricHistory history;
 private readonly string deviceId;
 private readonly Brush edge;
 public LabHardwareCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool notes=Composition=="MarginNotes",bridge=Composition=="BridgeRows",fold=Composition=="FoldedSheet",corners=Composition=="CornerTiles",dots=Composition=="ChronoDots",dock=Composition=="LabelDock",ladder=Composition=="LadderGrid",rail=Composition=="ValueRail";
  var d=DesignCatalog.Resolve(p);double s=p.FontScale,g=Preferences.TextFactor(p.InstrumentScale);string accent=DesignCatalog.CategoryColor(d,device.Category,p),text=DesignCatalog.ColorOr(p.TextColor,d.Text),track=DesignCatalog.ColorOr(p.GraphTrackColor,d.Track);
  Resources["Text"]=Ui.Brush(text);Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,d.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,text));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(9,9,9,8);Margin=new Thickness(3,0,3,6);CornerRadius=new CornerRadius(corners?3:dock?18:12);BorderThickness=new Thickness(1);edge=notes||fold?Brushes.Transparent:Ui.Brush(d.Edge);BorderBrush=edge;var back=Ui.Brush(d.Panel);back.Opacity=notes||fold?0:p.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(p,2);Child=body;
  var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,12*s*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,11*s*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(new Viewbox{Child=header,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*s*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,8);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);Panel target=readings;
  if(corners||dock||ladder){var cells=new StudioFields(s*(dock?105:96));readings.Children.Add(cells);target=cells;}
  if(rail){var sheet=new Grid();sheet.ColumnDefinitions.Add(new(){Width=new GridLength(1.15,GridUnitType.Star)});sheet.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var tint=Ui.Brush(accent);tint.Opacity=.13*p.Opacity;var column=new Border{Background=tint,CornerRadius=new(4),Margin=new Thickness(0)};Grid.SetColumn(column,1);sheet.Children.Add(column);Grid.SetColumnSpan(readings,2);sheet.Children.Add(readings);body.Children.Add(sheet);}else body.Children.Add(readings);
  int index=0;
  foreach(var m in device.Metrics)
  {
   var label=Ui.SidebarText(m.Label,10*s*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   double font=(notes?16:corners?17:fold?16:dock?15:ladder?14:11)*s*Preferences.TextFactor(p.ValueTextScale);var value=Ui.RoleText(SidebarTextRole.Readings,"—",font);value.FontFamily=Ui.Font(p,notes||fold?"Bahnschrift, Segoe UI":"Cascadia Mono, Consolas");value.FontWeight=FontWeights.SemiBold;value.Foreground=Ui.ValueBrush(p,text);value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;values[m.Id]=value;
   var field=new Grid{Margin=new Thickness(0,0,0,fold?4:3)};
   if(m.Unit=="IP")
   {
    StudioFields.SetWide(field);field.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(3,4,3,4)});
   }
   else if(notes)
   {
    var row=new ReverseReadout(label,value){Margin=new Thickness(3,5,3,5)};field.Children.Add(row);
   }
   else if(bridge)
   {
    var art=new StudioGraphic("bridge",accent,track,p){Height=10*g};graphics.Add((m.Id,art));field.Children.Add(new BridgeReadout(label,value,art,Math.Min(52,32*g)){Margin=new Thickness(3,4,3,4)});
   }
   else if(fold)
   {
    field.Children.Add(new StudioGraphic("fold",accent,track,p));var stack=new StackPanel{Margin=new Thickness(12,6,12,8)};stack.Children.Add(label);value.TextAlignment=TextAlignment.Right;stack.Children.Add(value);field.Children.Add(stack);
   }
   else if(corners)
   {
    var stack=new StackPanel{Margin=new Thickness(7)};stack.Children.Add(value);stack.Children.Add(label);stack.Children.Add(new StudioGraphic("corner",accent,track,p){Height=22*g,Margin=new Thickness(0,3,0,0)});graphics.Add((m.Id,(StudioGraphic)stack.Children[^1]));field.Children.Add(new Border{BorderBrush=Ui.Brush(track),BorderThickness=new Thickness(0,0,0,1),Margin=new Thickness(2),Child=stack});
   }
   else if(dots)
   {
    var stack=new StackPanel();stack.Children.Add(label);stack.Children.Add(value);var art=new StudioGraphic("dots",accent,track,p){Height=32*g};graphics.Add((m.Id,art));var row=new Grid{Margin=new Thickness(3,4,3,4)};row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});row.ColumnDefinitions.Add(new(){Width=new GridLength(.8,GridUnitType.Star)});row.Children.Add(stack);Grid.SetColumn(art,1);art.Margin=new Thickness(10,0,0,0);art.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(art);field.Children.Add(row);
   }
   else if(dock)
   {
    var stack=new StackPanel{Margin=new Thickness(4)};var tab=Ui.Brush(accent);tab.Opacity=.14*p.Opacity;label.Margin=new Thickness(7,3,7,3);stack.Children.Add(new Border{Background=tab,CornerRadius=new(5,5,0,0),Child=label,HorizontalAlignment=HorizontalAlignment.Left,MaxWidth=160*s});value.Margin=new Thickness(7,5,7,6);stack.Children.Add(new Border{CornerRadius=new(0,7,7,7),BorderThickness=new Thickness(1),BorderBrush=Ui.Brush(accent),Child=value});field.Children.Add(stack);
   }
   else if(ladder)
   {
    var stack=new StackPanel{Margin=new Thickness(5,index%2==0?0:9,5,index%2==0?9:0)};label.TextAlignment=value.TextAlignment=TextAlignment.Center;stack.Children.Add(value);var art=new StudioGraphic("ladder",accent,track,p){Height=13*g,Margin=new Thickness(0,2,0,2)};graphics.Add((m.Id,art));stack.Children.Add(art);stack.Children.Add(label);field.Children.Add(stack);
   }
   else field.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,font){Margin=new Thickness(3,6,3,6)});
   target.Children.Add(field);index++;
  }
  Update(device,p);
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values)pair.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key)?.Display(p.Fahrenheit)??"—";
  foreach(var (id,art) in graphics){var metric=device.Metrics.FirstOrDefault(m=>m.Id==id);art.Ratio=metric?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,metric.Unit is "%" or "°C"?100:double.IsFinite(metric.Maximum)?metric.Maximum:100),0,1):null;art.SetHistory(history.Read(deviceId,id),history.LatestTime);}
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 // Values form the left column here; labels deliberately form the right column.
 private sealed class ReverseReadout:Panel
 {
  readonly TextBlock label,value;bool inline;
  public ReverseReadout(TextBlock label,TextBlock value){this.label=label;this.value=value;Children.Add(value);Children.Add(label);}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):280;inline=w>=150;value.Measure(new Size(inline?w*.55:w,double.PositiveInfinity));label.Measure(new Size(inline?Math.Max(0,w*.45-10):w,double.PositiveInfinity));return new Size(w,inline?Math.Max(value.DesiredSize.Height,label.DesiredSize.Height):value.DesiredSize.Height+label.DesiredSize.Height);}
  protected override Size ArrangeOverride(Size s){double h=inline?Math.Max(value.DesiredSize.Height,label.DesiredSize.Height):value.DesiredSize.Height+label.DesiredSize.Height,y=Math.Max(0,(s.Height-h)/2);value.Arrange(new Rect(0,y,inline?s.Width*.55:s.Width,value.DesiredSize.Height));label.Arrange(new Rect(inline?s.Width*.55+10:0,y+(inline?0:value.DesiredSize.Height),inline?Math.Max(0,s.Width*.45-10):s.Width,label.DesiredSize.Height));return s;}
 }
 private sealed class BridgeReadout:Panel
 {
  readonly TextBlock label,value;readonly FrameworkElement art;readonly double wanted;double gap,lv,vw;
  public BridgeReadout(TextBlock label,TextBlock value,FrameworkElement art,double wanted){this.label=label;this.value=value;this.art=art;this.wanted=wanted;Children.Add(label);Children.Add(art);Children.Add(value);value.TextAlignment=TextAlignment.Right;}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):280;value.Measure(new Size(w*.48,double.PositiveInfinity));vw=Math.Min(w*.48,value.DesiredSize.Width);gap=Math.Max(0,Math.Min(wanted,w-vw-60));lv=Math.Max(0,w-vw-gap-(gap>0?12:4));label.Measure(new Size(lv,double.PositiveInfinity));art.Measure(new Size(gap,art.Height));return new Size(w,Math.Max(Math.Max(label.DesiredSize.Height,value.DesiredSize.Height),art.DesiredSize.Height));}
  protected override Size ArrangeOverride(Size s){label.Arrange(new Rect(0,Math.Max(0,(s.Height-label.DesiredSize.Height)/2),lv,label.DesiredSize.Height));art.Arrange(new Rect(lv+4,Math.Max(0,(s.Height-art.DesiredSize.Height)/2),gap,art.DesiredSize.Height));value.Arrange(new Rect(Math.Max(0,s.Width-vw),Math.Max(0,(s.Height-value.DesiredSize.Height)/2),vw,value.DesiredSize.Height));return s;}
 }
 private sealed class StudioFields:Panel
 {
  readonly double minimum;int columns;readonly List<(UIElement Child,int Row,int Col,int Span)> slots=new();readonly List<double> heights=new();
  static readonly DependencyProperty Wide=DependencyProperty.RegisterAttached("Wide",typeof(bool),typeof(StudioFields),new PropertyMetadata(false));public static void SetWide(DependencyObject d)=>d.SetValue(Wide,true);
  public StudioFields(double minimum){this.minimum=minimum;}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):280;columns=Math.Clamp((int)(w/Math.Max(1,minimum)),1,2);slots.Clear();heights.Clear();int row=0,col=0;foreach(UIElement child in InternalChildren){int span=(bool)child.GetValue(Wide)?columns:1;if(col+span>columns){row++;col=0;}while(heights.Count<=row)heights.Add(0);child.Measure(new Size(w*span/columns,double.PositiveInfinity));heights[row]=Math.Max(heights[row],child.DesiredSize.Height);slots.Add((child,row,col,span));col+=span;if(col==columns){row++;col=0;}}return new Size(w,heights.Sum());}
  protected override Size ArrangeOverride(Size s){double extra=heights.Count==0?0:Math.Max(0,s.Height-heights.Sum())/heights.Count;foreach(var x in slots)x.Child.Arrange(new Rect(s.Width*x.Col/columns,heights.Take(x.Row).Sum()+x.Row*extra,s.Width*x.Span/columns,heights[x.Row]+extra));return s;}
 }
 public sealed class StudioGraphic:FrameworkElement
 {
  public string Mode{get;}public double? Ratio;public int SampleCount=>samples.Count;
  readonly Brush ink,track,wash;readonly double stroke;IReadOnlyList<HistorySample> samples=Array.Empty<HistorySample>();DateTime now;
  public StudioGraphic(string mode,string accent,string track,Preferences p){Mode=mode;ink=Ui.Brush(accent);this.track=Ui.Brush(track);var tint=Ui.Brush(accent);tint.Opacity=.12*p.Opacity;wash=tint;stroke=DesignCatalog.GraphSize(p);ClipToBounds=true;IsHitTestVisible=false;}
  public void SetHistory(IReadOnlyList<HistorySample> points,DateTime time){samples=points;now=time;InvalidateVisual();}
  static Geometry Shape(params Point[] pts){var geo=new StreamGeometry();using(var c=geo.Open()){c.BeginFigure(pts[0],true,true);foreach(var p in pts.Skip(1))c.LineTo(p,true,false);}geo.Freeze();return geo;}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<2||h<2)return;var pen=new Pen(ink,Math.Max(.5,stroke));
   if(Mode=="fold"){double cut=Math.Min(12,w/5);dc.DrawGeometry(wash,new Pen(track,1),Shape(new Point(0,0),new Point(w-cut,0),new Point(w,cut),new Point(w,h),new Point(cut,h),new Point(0,h-cut)));dc.DrawLine(pen,new Point(w-cut,0),new Point(w-cut,cut));dc.DrawLine(pen,new Point(w-cut,cut),new Point(w,cut));return;}
   if(Mode=="corner"){double right=w-1;dc.DrawGeometry(null,new Pen(track,1),Shape(new Point(right,1),new Point(right,h-1),new Point(Math.Max(0,right-h),h-1)));if(Ratio is double v){double side=(h-2)*Math.Sqrt(v);dc.DrawGeometry(ink,null,Shape(new Point(right,h-1-side),new Point(right,h-1),new Point(right-side,h-1)));}return;}
   if(Mode=="bridge"){double y=h/2;dc.DrawLine(new Pen(track,2*stroke),new Point(1,y),new Point(w-1,y));if(Ratio is double v){double x=1+(w-2)*v;dc.DrawLine(new Pen(ink,2*stroke),new Point(1,y),new Point(x,y));dc.DrawEllipse(ink,null,new Point(x,y),2*stroke,2*stroke);}return;}
   if(Mode=="ladder"){for(int i=0;i<8;i++){double x=w*i/8,b=Math.Max(.5,w/8-2);dc.DrawRectangle(Ratio is double v&&i<Math.Ceiling(v*8)?ink:track,null,new Rect(x,(i%2)*h*.35,b,h*.65));}return;}
   if(Mode=="dots"){dc.DrawLine(new Pen(track,1),new Point(1,h-1),new Point(w-1,h-1));var valid=samples.Where(x=>x.Value is double v&&double.IsFinite(v)&&(now-x.Time).TotalSeconds>=0&&(now-x.Time).TotalSeconds<=60).ToArray();if(valid.Length==0)return;double max=Math.Max(1,valid.Max(x=>Math.Max(0,x.Value!.Value)));foreach(var point in valid){double age=(now-point.Time).TotalSeconds,x=(w-4)*(1-age/60)+2,y=h-2-(h-4)*Math.Clamp(point.Value!.Value/max,0,1);dc.DrawEllipse(ink,null,new Point(x,y),Math.Min(2,1.2*stroke),Math.Min(2,1.2*stroke));}return;}
  }
 }
}
