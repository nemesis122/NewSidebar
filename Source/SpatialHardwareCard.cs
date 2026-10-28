using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// These layouts change the composition of the readings, rather than the dial shape.
public sealed class SpatialHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly Preferences prefs;
 private readonly SidebarDesign design;
 private readonly string accent,layout;
 private readonly double scale;
 private readonly TextBlock number,unit;
 private readonly string? primaryId;
 private readonly RatioSurface surface;
 public SpatialHardwareCard(Device device,Preferences p)
 {
  prefs=p;design=DesignCatalog.Resolve(p);layout=CardLayouts.BaseLayout(p.Layout);accent=DesignCatalog.CategoryColor(design,device.Category,p);
  scale=p.FontScale*CardLayouts.ReadingMultiplier(p.Layout)*CardLayouts.SpaceMultiplier(p);
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,design.Font));
  CornerRadius=new(layout=="MetroMosaic"?0:layout=="SensorMap"?18:layout=="Blueprint"?2:10);BorderThickness=new(1);Padding=new(layout=="Blueprint"?14:11);Margin=new(3,0,3,design.Gap);
  BorderBrush=Ui.Brush(layout=="Blueprint"?accent:design.Edge);SetResourceReference(BackgroundProperty,"Panel");
  if(layout=="Blueprint")
  {
   var pen=new Pen(Tint(design.Track,.6*p.Opacity),.6);var group=new DrawingGroup();
   group.Children.Add(new GeometryDrawing(Tint(design.Panel,p.Opacity),null,new RectangleGeometry(new Rect(0,0,16,16))));
   group.Children.Add(new GeometryDrawing(null,pen,Geometry.Parse("M 0,16 L 0,0 L 16,0")));
   Background=new DrawingBrush(group){TileMode=TileMode.Tile,ViewportUnits=BrushMappingMode.Absolute,Viewport=new Rect(0,0,16,16),Stretch=Stretch.None};
  }
  var body=new ExpandedReadingsPanel(p,2);Child=body;
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,14*scale*Preferences.TextFactor(p.TitleTextScale)));
  var title=Ui.SidebarText(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);
  body.Children.Add(new Viewbox{Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Child=heading});
  var name=Ui.HardwareTitle(device.Name,11*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new(0,4,0,10);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);body.Children.Add(readings);
  number=Ui.Text("—",(layout=="Typographic"?58:35)*p.FontScale*Preferences.TextFactor(p.InstrumentTextScale));number.Foreground=Ui.ValueBrush(p,accent);number.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");number.FontWeight=FontWeights.SemiBold;number.TextTrimming=TextTrimming.None;
  System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
  unit=Ui.Text("",11*p.FontScale*Preferences.TextFactor(p.UnitTextScale),"Muted");
  surface=new RatioSurface(layout,accent,design.Track,DesignCatalog.GraphSize(p),p.Opacity);
  primaryId=CardLayouts.MainMetric(device)?.Id;
  FrameworkElement Hero(bool tile=false)
  {
   var digits=new StackPanel{VerticalAlignment=VerticalAlignment.Center};digits.Children.Add(number);digits.Children.Add(unit);
   double width=Math.Min(Math.Max(62,Math.Max(200,p.ColumnWidth)-74),110);
   var fitted=new Viewbox{Width=layout=="Typographic"?Math.Min(230,Math.Max(148,Math.Max(200,p.ColumnWidth)-52)):width,Height=layout=="Typographic"?92:74,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Child=digits};
   var group=new ResponsiveHero(fitted){MinHeight=layout=="MetroMosaic"?132:layout=="Typographic"?100:90};group.Children.Add(surface);group.Children.Add(fitted);
   if(layout=="SensorMap"){fitted.HorizontalAlignment=HorizontalAlignment.Center;group.Width=width+20;group.Height=90;group.HorizontalAlignment=HorizontalAlignment.Center;}
   double factor=Preferences.TextFactor(p.InstrumentScale);if(layout=="SensorMap")factor=Math.Min(factor,Math.Max(1,(Math.Max(200,p.ColumnWidth)-48)/(width+20)));group.LayoutTransform=new ScaleTransform(factor,factor);
   System.Windows.Automation.AutomationProperties.SetAutomationId(group,"SpatialGraphic");
   if(!tile)return group;
   return new Border{Background=Tint(accent,.13*p.Opacity),BorderBrush=Ui.Brush(accent),BorderThickness=new(1),Padding=new(9),Margin=new(3),Child=group};
  }
  if(layout=="MetroMosaic")
  {
   var mosaic=new Grid{Margin=new(-3)};mosaic.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});mosaic.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});
   for(int r=0;r<2+Math.Max(0,(device.Metrics.Count-1)/2);r++)mosaic.RowDefinitions.Add(new(){Height=p.Columns>1?new GridLength(1,GridUnitType.Star):GridLength.Auto});
   var hero=Hero(true);Grid.SetRowSpan(hero,2);mosaic.Children.Add(hero);
   int i=0;foreach(var m in device.Metrics){var tile=Cell(m,true);int row=i<2?i:2+(i-2)/2,col=i<2?1:(i-2)%2;Grid.SetRow(tile,row);Grid.SetColumn(tile,col);mosaic.Children.Add(tile);i++;}readings.Children.Add(mosaic);
  }
  else if(layout=="SensorMap")
  {
   readings.Children.Add(Hero());
   var nodes=new Grid();nodes.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});nodes.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});
   int count=(device.Metrics.Count+1)/2;for(int r=0;r<count;r++)nodes.RowDefinitions.Add(new(){Height=p.Columns>1?new GridLength(1,GridUnitType.Star):GridLength.Auto});
   nodes.Children.Add(new ConnectionSurface(accent,count){Opacity=.7});Grid.SetColumnSpan(nodes.Children[0],2);Grid.SetRowSpan(nodes.Children[0],Math.Max(1,count));
   int i=0;foreach(var m in device.Metrics){var cell=Cell(m,true);cell.Margin=new(i%2==0?0:10,9,i%2==0?10:0,4);Grid.SetRow(cell,i/2);Grid.SetColumn(cell,i%2);nodes.Children.Add(cell);i++;}readings.Children.Add(nodes);
  }
  else if(layout=="SplitPanel")
  {
   var split=new Grid();bool below=p.Columns>1&&p.ExpandMultiColumnReadings;
   split.ColumnDefinitions.Add(new(){Width=new(below?1:.9,GridUnitType.Star)});if(!below)split.ColumnDefinitions.Add(new(){Width=new(1.35,GridUnitType.Star)});
   if(below){split.RowDefinitions.Add(new(){Height=GridLength.Auto});split.RowDefinitions.Add(new(){Height=new(1,GridUnitType.Star)});}
   var hero=new Border{Background=Tint(accent,.16*p.Opacity),BorderThickness=new(3,0,0,0),BorderBrush=Ui.Brush(accent),Padding=new(7),Margin=new(0,0,below?0:9,below?9:0),Child=Hero()};split.Children.Add(hero);
   var list=new ExpandedReadingsPanel(p);foreach(var m in device.Metrics)list.Children.Add(Cell(m,false));Grid.SetColumn(list,below?0:1);Grid.SetRow(list,below?1:0);split.Children.Add(list);readings.Children.Add(split);
  }
  else
  {
   readings.Children.Add(Hero());int index=0;
   foreach(var m in device.Metrics)
   {
    var row=Cell(m,false);
    if(layout=="Blueprint")
    {
     var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new(27*scale)});grid.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});
     var order=Ui.Text((++index).ToString("00"),8*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");order.VerticalAlignment=VerticalAlignment.Center;grid.Children.Add(order);Grid.SetColumn(row,1);grid.Children.Add(row);readings.Children.Add(grid);
    }
    else readings.Children.Add(row);
   }
  }
 }
 private Brush Tint(string color,double opacity){var b=Ui.Brush(color);b.Opacity=Math.Clamp(opacity,0,1);return b;}
 private Border Cell(Metric metric,bool tile)
 {
  var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(prefs.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;
  var value=Ui.Text("—",(tile?15:12)*scale*Preferences.TextFactor(prefs.ValueTextScale));value.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(prefs,design.Text);value.TextTrimming=TextTrimming.None;if(tile||metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
  Panel row;if(tile){var vertical=new StackPanel{VerticalAlignment=VerticalAlignment.Center};vertical.Children.Add(label);value.Margin=new(0,5,0,0);vertical.Children.Add(value);row=vertical;}else row=new RadialHardwareCard.ReadoutPanel(label,value,12*scale*Preferences.TextFactor(prefs.ValueTextScale));
  return new Border{Child=row,Padding=new(tile?8:0,tile?7:5,tile?8:0,tile?7:5),Margin=new(tile?3:0),Background=tile?Tint(design.Track,.55*prefs.Opacity):null,BorderBrush=Ui.Brush(layout=="Blueprint"?accent:design.Edge),BorderThickness=new(tile?1:0,0,tile?1:0,1),CornerRadius=new(layout=="SensorMap"?8:0)};
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var value)){value.Text=metric.Display(p.Fahrenheit);value.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+value.Text;}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string text=primary?.Display(p.Fahrenheit)??"—";int split=text.LastIndexOf(' ');number.Text=split<0?text:text[..split];unit.Text=split<0?primary?.Unit??"":text[(split+1)..];
  surface.Ratio=primary?.Value is double valueNumber&&double.IsFinite(valueNumber)?Math.Clamp(valueNumber/Math.Max(1,primary.Maximum),0,1):0;
  bool alert=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)));BorderBrush=Ui.Brush(alert?"#F47777":layout=="Blueprint"?accent:design.Edge);
 }
 private sealed class ResponsiveHero:Grid
 {
  private readonly Viewbox digits;private readonly double desired;
  public ResponsiveHero(Viewbox digits){this.digits=digits;desired=digits.Width;}
  protected override Size MeasureOverride(Size available){digits.Width=double.IsFinite(available.Width)?Math.Max(1,Math.Min(desired,available.Width)):desired;return base.MeasureOverride(available);}
 }
 private sealed class RatioSurface:FrameworkElement
 {
  private readonly string layout;private readonly Brush accent,track;private readonly double strength,opacity;private double ratio;
  public RatioSurface(string layout,string accent,string track,double strength,double opacity){this.layout=layout;this.accent=Ui.Brush(accent);this.track=Ui.Brush(track);this.strength=strength;this.opacity=opacity;IsHitTestVisible=false;}
  public double Ratio{get=>ratio;set{ratio=value;InvalidateVisual();}}
  protected override void OnRender(DrawingContext dc)
  {
   double w=Math.Max(0,ActualWidth),h=Math.Max(0,ActualHeight),line=3*strength;
   if(layout=="SensorMap")
   {
    var chip=new Rect(6,4,Math.Max(0,w-12),Math.Max(0,h-12));dc.DrawRoundedRectangle(null,new Pen(accent,1),chip,5,5);
    for(int i=1;i<=4;i++){double y=8+i*Math.Max(0,h-20)/5;dc.DrawLine(new Pen(track,2),new Point(0,y),new Point(6,y));dc.DrawLine(new Pen(track,2),new Point(Math.Max(0,w-6),y),new Point(w,y));}
   }
   else if(layout=="Blueprint")
   {
    var pen=new Pen(accent,1);dc.DrawLine(pen,new Point(0,8),new Point(w,8));for(int i=0;i<=10;i++){double x=w*i/10;dc.DrawLine(pen,new Point(x,5),new Point(x,i%5==0?17:12));}
   }
   double yBar=Math.Max(0,h-line-2);dc.DrawRectangle(track,null,new Rect(0,yBar,w,line));dc.DrawRectangle(accent,null,new Rect(0,yBar,w*ratio,line));
  }
 }
 private sealed class ConnectionSurface:FrameworkElement
 {
  private readonly Brush color;private readonly int rows;
  public ConnectionSurface(string color,int rows){this.color=Ui.Brush(color);this.rows=rows;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc)
  {
   double mid=ActualWidth/2;var pen=new Pen(color,.8);dc.DrawLine(pen,new Point(mid,0),new Point(mid,ActualHeight));
   for(int r=0;r<rows;r++){double y=ActualHeight*(r+.5)/Math.Max(1,rows);dc.DrawLine(pen,new Point(mid-14,y),new Point(mid+14,y));dc.DrawEllipse(color,null,new Point(mid,y),2,2);}
  }
 }
}
