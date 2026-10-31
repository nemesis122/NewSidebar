using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Each sensor owns a visual field; there is no main dial followed by a repeated list.
public sealed class DistinctHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition{get;}
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly List<(string Id,SensorGlyph Glyph)> glyphs=new();
 private readonly Brush normalEdge;
 public DistinctHardwareCard(Device device,Preferences p)
 {
  Composition=CardLayouts.BaseLayout(p.Layout);bool capsules=Composition=="CapsuleDock",rings=Composition=="RingMatrix";
  var d=DesignCatalog.Resolve(p);double scale=p.FontScale;string accent=DesignCatalog.CategoryColor(d,device.Category,p),text=DesignCatalog.ColorOr(p.TextColor,d.Text),track=DesignCatalog.ColorOr(p.GraphTrackColor,d.Track);
  Resources["Text"]=Ui.Brush(text);Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,d.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,text));System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Padding=new Thickness(9,8,9,8);Margin=new Thickness(3,0,3,6);CornerRadius=new CornerRadius(capsules?16:rings?18:3);BorderThickness=new Thickness(1);normalEdge=capsules?Brushes.Transparent:Ui.Brush(d.Edge);BorderBrush=normalEdge;
  var back=Ui.Brush(d.Panel);back.Opacity=capsules?0:p.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(p,2);Child=body;var titleRow=new StackPanel{Orientation=Orientation.Horizontal};titleRow.Children.Add(Ui.HardwareIcon(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,11*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;titleRow.Children.Add(title);body.Children.Add(new Viewbox{Child=titleRow,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,7);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);body.Children.Add(readings);
  var fields=new SensorFieldsPanel(capsules?2:3,(capsules?108:68)*scale);readings.Children.Add(fields);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,9*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
   var value=Ui.RoleText(SidebarTextRole.Readings,"—",11*scale*Preferences.TextFactor(p.ValueTextScale));value.Foreground=Ui.ValueBrush(p,text);value.FontWeight=FontWeights.SemiBold;value.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");value.TextTrimming=TextTrimming.None;value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   var wash=Ui.Brush(accent);wash.Opacity=(capsules?.09:.035)*p.Opacity;
   var cell=new Border{Padding=new Thickness(capsules?9:4,capsules?6:5,capsules?9:4,capsules?6:5),Margin=new Thickness(2),CornerRadius=new CornerRadius(capsules?18:rings?10:2),Background=wash};
   if(metric.Unit=="IP")
   {
    SensorFieldsPanel.SetWide(cell,true);var address=new StackPanel();address.Children.Add(label);value.TextAlignment=TextAlignment.Right;address.Children.Add(value);cell.Child=address;
   }
   else if(capsules)
   {
    var row=new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale));cell.Child=row;cell.BorderBrush=Tint(accent,.3);cell.BorderThickness=new Thickness(1);
   }
   else
   {
    var stack=new StackPanel{HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Center};label.TextAlignment=TextAlignment.Center;value.TextAlignment=TextAlignment.Center;
    var glyph=new SensorGlyph(rings,accent,track,DesignCatalog.GraphSize(p));glyphs.Add((metric.Id,glyph));
    glyph.Width=rings?48:28;glyph.Height=48;UIElement artwork=glyph;
    if(rings){var dial=new Grid{Width=48,Height=48};dial.Children.Add(glyph);dial.Children.Add(new Viewbox{Child=value,Width=35,Height=24,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center});artwork=dial;}
    double size=(rings?43:34)*Preferences.TextFactor(p.InstrumentScale);var graphic=new Viewbox{Child=artwork,Width=size,Height=rings?size:42*Preferences.TextFactor(p.InstrumentScale),Stretch=Stretch.Uniform,StretchDirection=StretchDirection.Both,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,3,0,4)};
    stack.Children.Add(label);stack.Children.Add(graphic);if(!rings)stack.Children.Add(value);cell.Child=stack;
   }
   fields.Children.Add(cell);
  }
  Update(device,p);
 }
 private static Brush Tint(string color,double alpha){var b=Ui.Brush(color);b.Opacity=alpha;return b;}
 public void Update(Device device,Preferences p)
 {
  foreach(var entry in values)entry.Value.Text=device.Metrics.FirstOrDefault(m=>m.Id==entry.Key)?.Display(p.Fahrenheit)??"—";
  foreach(var (id,glyph) in glyphs){var metric=device.Metrics.FirstOrDefault(m=>m.Id==id);glyph.Available=metric?.Value is double v&&double.IsFinite(v);glyph.Ratio=metric?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,metric.Unit is "%" or "°C"?100:double.IsFinite(metric.Maximum)?metric.Maximum:100),0,1):0;glyph.InvalidateVisual();}
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):normalEdge;
 }
 // Reflows by measured width, including long hardware names and full-width addresses.
 private sealed class SensorFieldsPanel:Panel
 {
  private readonly int maximumColumns;private readonly double minimumWidth;private int columns=1;private readonly List<(UIElement Child,int Row,int Column,int Span)> slots=new();private readonly List<double> heights=new();
  private static readonly DependencyProperty WideProperty=DependencyProperty.RegisterAttached("Wide",typeof(bool),typeof(SensorFieldsPanel),new FrameworkPropertyMetadata(false));
  public static void SetWide(DependencyObject child,bool value)=>child.SetValue(WideProperty,value);
  public SensorFieldsPanel(int maximumColumns,double minimumWidth){this.maximumColumns=maximumColumns;this.minimumWidth=minimumWidth;}
  protected override Size MeasureOverride(Size available)
  {
   double width=double.IsFinite(available.Width)?Math.Max(0,available.Width):300;columns=Math.Clamp((int)(width/Math.Max(1,minimumWidth)),1,maximumColumns);slots.Clear();heights.Clear();int row=0,column=0;
   foreach(UIElement child in InternalChildren){if(child.Visibility==Visibility.Collapsed)continue;int span=(bool)child.GetValue(WideProperty)?columns:1;if(column+span>columns){row++;column=0;}while(heights.Count<=row)heights.Add(0);child.Measure(new Size(width*span/columns,double.PositiveInfinity));heights[row]=Math.Max(heights[row],child.DesiredSize.Height);slots.Add((child,row,column,span));column+=span;if(column==columns){row++;column=0;}}
   return new Size(width,heights.Sum());
  }
  protected override Size ArrangeOverride(Size final)
  {
   double extra=heights.Count==0?0:Math.Max(0,final.Height-heights.Sum())/heights.Count;
   foreach(var slot in slots){double top=heights.Take(slot.Row).Sum()+extra*slot.Row;slot.Child.Arrange(new Rect(final.Width*slot.Column/columns,top,final.Width*slot.Span/columns,heights[slot.Row]+extra));}return final;
  }
 }
 private sealed class SensorGlyph:FrameworkElement
 {
  public bool Available;public double Ratio;private readonly bool ring;private readonly Brush ink,track;private readonly double thickness;
  public SensorGlyph(bool ring,string accent,string track,double thickness){this.ring=ring;ink=Ui.Brush(accent);this.track=Ui.Brush(track);this.thickness=thickness;IsHitTestVisible=false;}
  protected override void OnRender(DrawingContext dc)
  {
   double w=ActualWidth,h=ActualHeight;if(w<2||h<2)return;
   if(!ring){int levels=10;double gap=1.6,band=(h-4-gap*(levels-1))/levels;for(int i=0;i<levels;i++){var rect=new Rect(3,h-2-band-i*(band+gap),w-6,Math.Max(.5,band));dc.DrawRoundedRectangle(Available&&Ratio>i/(double)levels?ink:track,null,rect,.5,.5);}return;}
   double radius=Math.Max(0,Math.Min(w,h)/2-3);var center=new Point(w/2,h/2);dc.DrawEllipse(null,new Pen(track,3*thickness),center,radius,radius);if(!Available||Ratio<=0)return;double sweep=Math.Min(.999999,Ratio)*2*Math.PI;var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(new Point(center.X,center.Y-radius),false,false);c.ArcTo(new Point(center.X+radius*Math.Sin(sweep),center.Y-radius*Math.Cos(sweep)),new Size(radius,radius),0,sweep>Math.PI,SweepDirection.Clockwise,true,false);}dc.DrawGeometry(null,new Pen(ink,3*thickness){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},path);
  }
 }
}
