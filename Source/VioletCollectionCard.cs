using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Eight requested variations of the black/violet reference style; no sensor acquisition.
public sealed class VioletCollectionCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;public string Composition{get;}
 readonly Dictionary<string,TextBlock> values=new();readonly List<(string Id,TextBlock Number,TextBlock Unit)> instruments=new();
 readonly List<(string Id,VioletMonitorCard.VioletBar Bar)> bars=new();readonly List<(string Id,HistoryAreaGraph Plot)> plots=new();
 readonly MetricHistory history;readonly string deviceId;
 public VioletCollectionCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool top=Composition=="VioletStack",mirror=Composition=="VioletMirror",twin=Composition=="VioletTwin",tiles=Composition=="VioletTiles",console=Composition=="VioletConsole",crown=Composition=="VioletCrown",ribbon=Composition=="VioletRibbon",compact=Composition=="VioletCompact";
  double s=p.FontScale,g=Preferences.TextFactor(p.InstrumentScale);string ink=DesignCatalog.ColorOr(p.TextColor,"#F0F0F0"),muted=DesignCatalog.ColorOr(p.LabelColor,"#A4A4A4"),track=DesignCatalog.ColorOr(p.GraphTrackColor,"#303030"),accent=p.GraphColors.TryGetValue(device.Category,out var selected)?DesignCatalog.ColorOr(selected,"#9300DC"):"#9300DC",end=DesignCatalog.ColorOr(p.SecondaryGraphColor,"#E000CB");
  Resources["Text"]=Ui.Brush(ink);Resources["Muted"]=Ui.Brush(muted);Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,ink));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(compact?8:10);Margin=new Thickness(3,0,3,7);CornerRadius=new CornerRadius(console?3:7);BorderThickness=new Thickness(1);BorderBrush=Ui.Brush("#4A4A4A");var black=Ui.Brush("#000000");black.Opacity=p.Opacity;Background=black;
  var root=new Grid();root.RowDefinitions.Add(new(){Height=GridLength.Auto});root.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});Child=root;
  var header=new StackPanel{Margin=new Thickness(0,0,0,8)};var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,16*s*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,12*s*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.Bold;heading.Children.Add(title);header.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});var hardware=Ui.HardwareTitle(device.Name,10*s*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));hardware.TextWrapping=TextWrapping.Wrap;hardware.TextTrimming=TextTrimming.None;hardware.Margin=new Thickness(0,3,0,0);header.Children.Add(hardware);root.Children.Add(header);
  var readings=new ExpandedReadingsPanel(p);Panel target=readings;
  if(tiles||top){var cells=new Fields(tiles?104*s:102*s);var holder=new Grid();holder.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});holder.Children.Add(cells);readings.Children.Add(holder);target=cells;}
  var main=CardLayouts.MainMetric(device);FigureHost? figure=null;
  if(!tiles&&main!=null)
  {
   FrameworkElement fig;Size natural;
   if(twin)
   {
    var pair=new Grid{Width=236,Height=122};pair.ColumnDefinitions.Add(new(){Width=new GridLength(118)});pair.ColumnDefinitions.Add(new(){Width=new GridLength(118)});pair.Children.Add(Dial(main,false));
    var secondary=device.Metrics.FirstOrDefault(m=>m.Id!=main.Id&&m.Unit=="%")??device.Metrics.FirstOrDefault(m=>m.Id!=main.Id&&m.Unit!="IP");
    if(secondary!=null){var other=Dial(secondary,true);Grid.SetColumn(other,1);pair.Children.Add(other);}else{pair.ColumnDefinitions.RemoveAt(1);pair.Width=118;}
    fig=pair;natural=new Size(pair.Width,122);
   }
   else if(console||crown||ribbon)
   {
    natural=crown?new Size(180,104):ribbon?new Size(220,64):new Size(116,94);var screen=new Grid{Width=natural.Width,Height=natural.Height};screen.Children.Add(new AccentFrame(crown?"crown":ribbon?"ribbon":"console",accent,end,track,DesignCatalog.GraphSize(p)));var digits=Digits(main,false,ribbon?29:28,ribbon);digits.Margin=crown?new Thickness(0,20,0,10):new Thickness(0);screen.Children.Add(new Viewbox{Child=digits,MaxWidth=natural.Width*.76,MaxHeight=crown?natural.Height*.7:ribbon?32:54,Margin=crown?new Thickness(0):new Thickness(0,ribbon?5:10,0,0),Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=crown?VerticalAlignment.Center:VerticalAlignment.Top});var caption=Caption(main);if(!crown)caption.Margin=new Thickness(0,0,0,10);screen.Children.Add(caption);fig=screen;
   }
   else{fig=Dial(main,false);natural=new Size(116,122);}
   figure=new FigureHost(fig,natural,g*(compact?.62:1)){HorizontalAlignment=HorizontalAlignment.Center};System.Windows.Automation.AutomationProperties.SetAutomationId(figure,"CardInstrument");
  }
  bool stacked=top||twin||crown||ribbon||p.ExpandMultiColumnReadings;
  var content=new FigureAndReadings(figure,readings,stacked,mirror,Math.Max(94,80*s*Preferences.TextFactor(p.ValueTextScale)));Grid.SetRow(content,1);root.Children.Add(content);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*s*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   double font=(tiles?16:11)*s*Preferences.TextFactor(p.ValueTextScale);var value=Ui.RoleText(SidebarTextRole.Readings,"—",font);value.FontFamily=Ui.Font(p,"Segoe UI");value.FontWeight=FontWeights.SemiBold;value.Foreground=Ui.ValueBrush(p,ink);value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;values[metric.Id]=value;
   if(metric.Unit=="IP"){var address=new AddressReadoutPanel(label,value){Margin=new Thickness(0,3,0,3)};Fields.SetWide(address);target.Children.Add(address);continue;}
   var field=new StackPanel{Margin=new Thickness(tiles||top?4:0,2,tiles||top?4:0,compact?2:5)};
   if(tiles){field.Children.Add(label);value.Margin=new Thickness(0,2,0,3);field.Children.Add(value);}else field.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(0,0,0,2)});
   if(device.Category=="Network")
   {
    if(metric.Label is "Download" or "Upload"){var plot=new HistoryAreaGraph(accent,track,DesignCatalog.GraphSize(p));var frame=new Grid{Height=20*g};frame.Children.Add(new AccentFrame("grid",accent,end,track,1));frame.Children.Add(plot);plots.Add((metric.Id,plot));field.Children.Add(frame);}
   }
   else
   {
    var bar=new VioletMonitorCard.VioletBar(accent,track){Height=(compact?4:tiles?5:7)*g*DesignCatalog.GraphSize(p)};bars.Add((metric.Id,bar));field.Children.Add(bar);
   }
   if(tiles)target.Children.Add(new Border{Child=field,BorderBrush=Ui.Brush("#3B3B3B"),BorderThickness=new Thickness(1),CornerRadius=new(3),Padding=new Thickness(4),Margin=new Thickness(2)});else target.Children.Add(field);
  }
  Update(device,p);

  FrameworkElement Dial(Metric m,bool secondary)
  {
   var dial=new Grid{Width=116,Height=122};dial.Children.Add(new VioletMonitorCard.VioletRing(accent,end,DesignCatalog.GraphSize(p)));var digits=Digits(m,secondary,28,false);digits.Margin=new Thickness(0,0,0,10);dial.Children.Add(new Viewbox{Child=digits,MaxWidth=70,MaxHeight=66,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center});dial.Children.Add(Caption(m));return dial;
  }
  StackPanel Digits(Metric m,bool secondary,double font,bool horizontal)
  {
   var n=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",font*s*Preferences.TextFactor(p.InstrumentTextScale));n.FontWeight=FontWeights.Bold;n.Foreground=Ui.ValueBrush(p,ink);n.HorizontalAlignment=HorizontalAlignment.Center;System.Windows.Automation.AutomationProperties.SetAutomationId(n,secondary?"SecondaryMetric":"PrimaryMetric");var u=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",11*s*Preferences.TextFactor(p.UnitTextScale),"Muted");u.HorizontalAlignment=HorizontalAlignment.Center;u.VerticalAlignment=horizontal?VerticalAlignment.Bottom:VerticalAlignment.Center;if(horizontal)u.Margin=new Thickness(6,0,0,5);instruments.Add((m.Id,n,u));var panel=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Orientation=horizontal?Orientation.Horizontal:Orientation.Vertical};panel.Children.Add(n);panel.Children.Add(u);return panel;
  }
  FrameworkElement Caption(Metric m)
  {
   var caption=Ui.SidebarText(m.Label,10*s*Preferences.TextFactor(p.LabelTextScale),"Muted");return new Viewbox{Child=caption,MaxWidth=100,MaxHeight=18,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Bottom};
  }
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values)pair.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key)?.Display(p.Fahrenheit)??"—";
  foreach(var (id,n,u) in instruments){var m=device.Metrics.FirstOrDefault(m=>m.Id==id);string text=m?.Display(p.Fahrenheit)??"—";int split=text.LastIndexOf(' ');n.Text=split<0?text:text[..split];u.Text=split<0?m?.Unit??"":text[(split+1)..];}
  foreach(var (id,bar) in bars){var m=device.Metrics.FirstOrDefault(m=>m.Id==id);bar.Ratio=m?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,m.Unit is "%" or "°C"?100:double.IsFinite(m.Maximum)?m.Maximum:100),0,1):null;bar.InvalidateVisual();}
  foreach(var (id,plot) in plots)plot.SetSamples(history.Read(deviceId,id),history.LatestTime,0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  BorderBrush=Ui.Brush(device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?"#F47777":"#4A4A4A");
 }
 private sealed class FigureHost:Decorator
 {
  readonly Size natural;readonly double request;double scale;
  public FigureHost(UIElement child,Size natural,double request){Child=child;this.natural=natural;this.request=request;ClipToBounds=true;}
  protected override Size MeasureOverride(Size a){scale=Math.Min(request,double.IsFinite(a.Width)?Math.Max(0,a.Width)/natural.Width:request);Child.Measure(natural);return new Size(natural.Width*scale,natural.Height*scale);}
  protected override Size ArrangeOverride(Size size){Child.Arrange(new Rect(new Point(),natural));Child.RenderTransform=new ScaleTransform(scale,scale);return size;}
 }
 private sealed class FigureAndReadings:Panel
 {
  readonly FrameworkElement? figure;readonly FrameworkElement readings;readonly bool stack,mirror;readonly double minimum;double fw,fh;
  public FigureAndReadings(FrameworkElement? figure,FrameworkElement readings,bool stack,bool mirror,double minimum){this.figure=figure;this.readings=readings;this.stack=stack;this.mirror=mirror;this.minimum=minimum;if(figure!=null)Children.Add(figure);Children.Add(readings);}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):300;figure?.Measure(new Size(stack?w:Math.Max(0,w-Math.Min(w*.65,minimum)-8),double.PositiveInfinity));fw=figure?.DesiredSize.Width??0;fh=figure?.DesiredSize.Height??0;readings.Measure(new Size(stack?w:Math.Max(0,w-fw-(figure!=null?8:0)),double.PositiveInfinity));return new Size(w,stack?fh+(figure!=null?7:0)+readings.DesiredSize.Height:Math.Max(fh,readings.DesiredSize.Height));}
  protected override Size ArrangeOverride(Size s){if(stack){figure?.Arrange(new Rect(Math.Max(0,(s.Width-fw)/2),0,fw,fh));double y=fh+(figure!=null?7:0);readings.Arrange(new Rect(0,y,s.Width,Math.Max(0,s.Height-y)));}else{figure?.Arrange(new Rect(mirror?Math.Max(0,s.Width-fw):0,Math.Max(0,(s.Height-fh)/2),fw,fh));readings.Arrange(new Rect(mirror||figure==null?0:fw+8,0,Math.Max(0,s.Width-fw-(figure!=null?8:0)),s.Height));}return s;}
 }
 private sealed class Fields:Panel
 {
  readonly double minimum;int cols;readonly List<(UIElement C,int R,int Col,int Span)> slots=new();readonly List<double> heights=new();static readonly DependencyProperty Wide=DependencyProperty.RegisterAttached("Wide",typeof(bool),typeof(Fields),new PropertyMetadata(false));public static void SetWide(DependencyObject child)=>child.SetValue(Wide,true);public Fields(double minimum){this.minimum=minimum;}
  protected override Size MeasureOverride(Size a){double w=double.IsFinite(a.Width)?Math.Max(0,a.Width):300;cols=Math.Clamp((int)(w/Math.Max(1,minimum)),1,2);slots.Clear();heights.Clear();int r=0,c=0;foreach(UIElement child in InternalChildren){int span=(bool)child.GetValue(Wide)?cols:1;if(c+span>cols){r++;c=0;}while(heights.Count<=r)heights.Add(0);child.Measure(new Size(w*span/cols,double.PositiveInfinity));heights[r]=Math.Max(heights[r],child.DesiredSize.Height);slots.Add((child,r,c,span));c+=span;if(c==cols){r++;c=0;}}return new Size(w,heights.Sum());}
  protected override Size ArrangeOverride(Size s){double extra=heights.Count==0?0:Math.Max(0,s.Height-heights.Sum())/heights.Count;foreach(var x in slots)x.C.Arrange(new Rect(s.Width*x.Col/cols,heights.Take(x.R).Sum()+x.R*extra,s.Width*x.Span/cols,heights[x.R]+extra));return s;}
 }
 private sealed class AccentFrame:FrameworkElement
 {
  readonly string mode;readonly Brush ink,track;readonly double thickness;
  public AccentFrame(string mode,string a,string b,string track,double thickness){this.mode=mode;ink=new LinearGradientBrush(Ui.Brush(a).Color,Ui.Brush(b).Color,new Point(0,0),new Point(1,0));this.track=Ui.Brush(track);this.thickness=thickness;ClipToBounds=true;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){double w=ActualWidth,h=ActualHeight;if(w<2||h<2)return;if(mode=="grid"){var p=new Pen(track,.4);for(double x=0;x<w;x+=5)dc.DrawLine(p,new Point(x,0),new Point(x,h));for(double y=0;y<h;y+=5)dc.DrawLine(p,new Point(0,y),new Point(w,y));return;}if(mode=="crown"){double r=Math.Min(w*.5-9,h-28),cx=w/2,cy=h-24;var f=new PathFigure{StartPoint=new Point(cx-r,cy)};f.Segments.Add(new ArcSegment(new Point(cx+r,cy),new Size(r,r),0,false,SweepDirection.Clockwise,true));dc.DrawGeometry(null,new Pen(ink,8*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},new PathGeometry(new[]{f}));dc.DrawEllipse(ink,null,new Point(cx-r+2,cy+10),3,3);dc.DrawEllipse(ink,null,new Point(cx-r+8,cy+17),3,3);return;}dc.DrawRoundedRectangle(null,new Pen(track,1),new Rect(1,1,w-2,h-2),mode=="ribbon"?7:2,mode=="ribbon"?7:2);double lineY=mode=="ribbon"?h-4:4;dc.DrawLine(new Pen(ink,3*thickness),new Point(5,lineY),new Point(w-5,lineY));if(mode=="console")dc.DrawLine(new Pen(ink,1),new Point(5,h-5),new Point(w-5,h-5));}
 }
}
