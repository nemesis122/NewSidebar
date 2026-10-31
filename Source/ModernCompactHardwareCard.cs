using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Dense compositions share the existing sensor snapshot, history and preferences.
// No timers, hardware access or generated history are owned by a card.
public sealed class ModernCompactHardwareCard : Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly MetricHistory history;
 private readonly string deviceId;
 private readonly string? primaryId;
 private readonly Brush edge;
 private readonly TextBlock? number,unit;
 private readonly HistoryAreaGraph? plot;
 private readonly MiniRing? ring;
 public ModernCompactHardwareCard(Device device,Preferences p,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool edgeCard=Composition=="EdgeCompact",tiles=Composition=="MicroGrid",flow=Composition=="FlowCompact",orbit=Composition=="OrbitMini";
  var design=DesignCatalog.Resolve(p);double scale=p.FontScale;
  string accent=DesignCatalog.CategoryColor(design,device.Category,p),text=DesignCatalog.ColorOr(p.TextColor,design.Text),track=DesignCatalog.ColorOr(p.GraphTrackColor,design.Track);
  Resources["Text"]=Ui.Brush(text);Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,design.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,text));
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(edgeCard?12:10,8,10,8);Margin=new Thickness(3,0,3,5);CornerRadius=new CornerRadius(tiles?12:edgeCard?7:14);BorderThickness=new Thickness(1);edge=Ui.Brush(design.Edge);BorderBrush=edge;
  var back=Ui.Brush(design.Panel);back.Opacity=p.Opacity;Background=back;
  var surface=new Grid();Child=surface;
  if(edgeCard)surface.Children.Add(new Border{Width=2,Margin=new Thickness(-8,1,0,1),HorizontalAlignment=HorizontalAlignment.Left,Background=Ui.Brush(accent),CornerRadius=new CornerRadius(1),IsHitTestVisible=false});
  var body=new ExpandedReadingsPanel(p,2);surface.Children.Add(body);
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale)));
  var title=Ui.SidebarText(device.Category,11*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);
  body.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,6);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);var content=new Grid();content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var primary=CardLayouts.MainMetric(device);primaryId=primary?.Id;
  if(flow||orbit)
  {
   double width=orbit?60:220,height=orbit?60:35;
   var figure=new Grid{Width=width,Height=height};
   number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",(orbit?19:22)*scale*Preferences.TextFactor(p.InstrumentTextScale));number.FontWeight=FontWeights.SemiBold;number.Foreground=Ui.ValueBrush(p,accent);number.TextTrimming=TextTrimming.None;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
   unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",8*scale*Preferences.TextFactor(p.UnitTextScale),"Muted");
   var digits=new StackPanel{HorizontalAlignment=orbit?HorizontalAlignment.Center:HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center,Orientation=orbit?Orientation.Vertical:Orientation.Horizontal};digits.Children.Add(number);digits.Children.Add(unit);
   if(orbit){ring=new MiniRing(accent,track,DesignCatalog.GraphSize(p));figure.Children.Add(ring);number.HorizontalAlignment=unit.HorizontalAlignment=HorizontalAlignment.Center;var fit=new Viewbox{Child=digits,Width=43,Height=40,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};figure.Children.Add(fit);}
   else{figure.ColumnDefinitions.Add(new(){Width=new GridLength(100)});figure.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});unit.Margin=new Thickness(4,0,0,4);unit.VerticalAlignment=VerticalAlignment.Bottom;figure.Children.Add(new Viewbox{Child=digits,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center});plot=new HistoryAreaGraph(accent,track,DesignCatalog.GraphSize(p)){Margin=new Thickness(6,2,0,2)};Grid.SetColumn(plot,1);figure.Children.Add(plot);}
   var instrument=new CompactGraphic(figure,new Size(width,height),Preferences.TextFactor(p.InstrumentScale));System.Windows.Automation.AutomationProperties.SetAutomationId(instrument,"CardInstrument");
   bool below=flow||CardLayouts.ReadingsBelow(p);
   if(below){content.RowDefinitions.Clear();content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});instrument.HorizontalAlignment=HorizontalAlignment.Center;instrument.Margin=new Thickness(0,0,0,5);content.Children.Add(instrument);Grid.SetRow(readings,1);}
   else{double room=Math.Min(width*Preferences.TextFactor(p.InstrumentScale),Math.Max(25,p.ColumnWidth-128));content.ColumnDefinitions.Add(new(){Width=new GridLength(room+9)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});instrument.Margin=new Thickness(0,0,9,0);instrument.VerticalAlignment=VerticalAlignment.Center;content.Children.Add(instrument);Grid.SetColumn(readings,1);}
  }
  content.Children.Add(readings);
  var cells=tiles?new Grid():null;int row=0,column=0;
  if(cells!=null){cells.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});cells.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});readings.Children.Add(cells);}
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");
   var value=Ui.RoleText(SidebarTextRole.Readings,"—",11*scale*Preferences.TextFactor(p.ValueTextScale));value.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");value.FontWeight=FontWeights.Medium;value.Foreground=Ui.ValueBrush(p,metric.Id==primaryId?accent:text);value.TextTrimming=TextTrimming.None;values[metric.Id]=value;
   if(cells!=null)
   {
    int span=metric.Unit=="IP"?2:1;if(column+span>2){row++;column=0;}while(cells.RowDefinitions.Count<=row)cells.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});
    label.TextTrimming=TextTrimming.CharacterEllipsis;label.TextWrapping=TextWrapping.NoWrap;value.TextWrapping=TextWrapping.Wrap;value.Margin=new Thickness(0,2,0,0);value.HorizontalAlignment=HorizontalAlignment.Right;
    var field=new StackPanel{VerticalAlignment=VerticalAlignment.Center};field.Children.Add(label);field.Children.Add(value);var tint=Ui.Brush(accent);tint.Opacity=(metric.Id==primaryId?.12:.045)*p.Opacity;
    var cell=new Border{Child=field,Padding=new Thickness(7,5,7,5),Margin=new Thickness(column==0?0:2,0,column==0&&span==1?2:0,4),Background=tint,CornerRadius=new CornerRadius(6)};Grid.SetColumn(cell,column);Grid.SetRow(cell,row);Grid.SetColumnSpan(cell,span);cells.Children.Add(cell);column+=span;if(column==2){column=0;row++;}
   }
   else if(metric.Unit=="IP")
   {
    readings.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(0,1,0,1)});
   }
   else readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(0,1,0,1)});
  }
  Update(device,p);
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var pair in values)pair.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key)?.Display(p.Fahrenheit)??"—";
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(p.Fahrenheit)??"—";int split=display.LastIndexOf(' ');
  if(number!=null)number.Text=split<0?display:display[..split];if(unit!=null)unit.Text=split<0?(primary?.Unit=="°C"&&p.Fahrenheit?"°F":primary?.Unit??""):display[(split+1)..];
  if(ring!=null){ring.Available=primary?.Value is double v&&double.IsFinite(v);ring.Ratio=primary?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,primary.Unit is "%" or "°C"?100:double.IsFinite(primary.Maximum)?primary.Maximum:100),0,1):0;ring.InvalidateVisual();}
  plot?.SetSamples(primaryId==null?Array.Empty<HistorySample>():history.Read(deviceId,primaryId),history.LatestTime,primary?.Unit=="%"?100:primary?.Unit=="°C"?Math.Max(100,primary.Maximum):0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class CompactGraphic:Decorator
 {
  private readonly Size natural;private readonly double requested;private double scale;
  public CompactGraphic(UIElement child,Size natural,double requested){Child=child;this.natural=natural;this.requested=requested;ClipToBounds=true;}
  protected override Size MeasureOverride(Size available){scale=Math.Min(requested,double.IsFinite(available.Width)?Math.Max(.001,available.Width/natural.Width):requested);Child.Measure(natural);return new Size(natural.Width*scale,natural.Height*scale);}
  protected override Size ArrangeOverride(Size final){Child.Arrange(new Rect(new Point(),natural));double fit=Math.Min(scale,Math.Max(.001,final.Width/natural.Width));Child.RenderTransform=new ScaleTransform(fit,fit);return final;}
 }
 private sealed class MiniRing:FrameworkElement
 {
  public bool Available;public double Ratio;private readonly Pen ink,track;
  public MiniRing(string accent,string background,double thickness){ink=new Pen(Ui.Brush(accent),3*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};track=new Pen(Ui.Brush(background),3*thickness);IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc){double r=Math.Max(0,Math.Min(ActualWidth,ActualHeight)/2-4);var center=new Point(ActualWidth/2,ActualHeight/2);dc.DrawEllipse(null,track,center,r,r);if(!Available||Ratio<=0)return;double sweep=Math.Min(.999999,Ratio)*Math.PI*2;var end=new Point(center.X+r*Math.Sin(sweep),center.Y-r*Math.Cos(sweep));var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(new Point(center.X,center.Y-r),false,false);c.ArcTo(end,new Size(r,r),0,sweep>Math.PI,SweepDirection.Clockwise,true,false);}path.Freeze();dc.DrawGeometry(null,ink,path);}
 }
}
