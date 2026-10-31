using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
namespace Aurora;

// Shared building blocks of the v3.01 designs: one header, one number style, one ratio rule
// and one warning rule, so every new card reads as part of the same family.
public abstract class ModernCardBase:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 protected readonly SidebarDesign D;
 protected readonly string Accent,Secondary,Edge;
 protected readonly double S;
 protected readonly string DeviceId;
 protected readonly Dictionary<string,TextBlock> Values=new();
 protected ModernCardBase(Device device,Preferences p)
 {
  D=DesignCatalog.Resolve(p);S=p.FontScale;DeviceId=device.Id;
  Accent=DesignCatalog.CategoryColor(D,device.Category,p);Secondary=DesignCatalog.ColorOr(p.SecondaryGraphColor,Accent);Edge=D.Edge;
  Resources["Text"]=Ui.Brush(D.Text);Resources["Muted"]=Ui.Brush(D.Muted);Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,D.Text));
  TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI Variable Text, Segoe UI"));
 }
 protected static double F(double v)=>Preferences.TextFactor(v);
 protected SolidColorBrush PanelBrush(Preferences p,double extra=1){var b=Ui.Brush(D.Panel);b.Opacity=Math.Clamp(p.Opacity*extra,0,1);return b;}
 // Icon + widget title on one line, hardware name beneath: the name is the most identifying text.
 protected FrameworkElement Header(Device device,Preferences p)
 {
  var stack=new StackPanel{Margin=new Thickness(0,0,0,8)};
  var line=new StackPanel{Orientation=Orientation.Horizontal};line.Children.Add(Ui.HardwareIcon(device.Category,15*S*F(p.TitleTextScale)));
  var title=Ui.SidebarText(device.Category,12*S*F(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;title.VerticalAlignment=VerticalAlignment.Center;line.Children.Add(title);stack.Children.Add(line);
  var name=Ui.HardwareTitle(device.Name,13*S*F(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,3,0,0);stack.Children.Add(name);
  return stack;
 }
 protected TextBlock Label(string text,Preferences p,double size=10.5){var t=Ui.SidebarText(text,size*S*F(p.LabelTextScale),"Muted");t.TextTrimming=TextTrimming.CharacterEllipsis;t.ToolTip=Ui.SidebarFullLabel(text);return t;}
 // Bahnschrift is Windows' built-in DIN face: instrument vernacular, steady tabular digits.
 protected TextBlock Value(Metric metric,Preferences p,double size=12.5)
 {
  var t=Ui.RoleText(SidebarTextRole.Readings,"—",size*S*F(p.ValueTextScale));t.FontFamily=Ui.Font(p,"Bahnschrift, Segoe UI");t.FontWeight=FontWeights.SemiBold;t.Foreground=Ui.ValueBrush(p,D.Text);
  Typography.SetNumeralAlignment(t,FontNumeralAlignment.Tabular);t.TextTrimming=TextTrimming.None;Values[metric.Id]=t;return t;
 }
 protected (TextBlock Number,TextBlock Unit) HeroNumber(Preferences p,double size=30)
 {
  var number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",size*S*F(p.InstrumentTextScale));number.FontFamily=Ui.Font(p,"Bahnschrift, Segoe UI");number.FontWeight=FontWeights.SemiBold;number.Foreground=Ui.ValueBrush(p,D.Text);
  Typography.SetNumeralAlignment(number,FontNumeralAlignment.Tabular);System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
  var unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",size*.42*S*F(p.UnitTextScale),"Muted");unit.Margin=new Thickness(3,0,0,size*.12*S);unit.VerticalAlignment=VerticalAlignment.Bottom;
  return (number,unit);
 }
 protected static void SetHero(TextBlock? number,TextBlock? unit,Metric? metric,Preferences p)
 {
  if(number==null)return;string text=metric?.Display(p.Fahrenheit)??"—";int split=text.LastIndexOf(' ');
  number.Text=split<0?text:text[..split];if(unit!=null)unit.Text=split<0?(text=="—"?"":metric?.Unit??""):text[(split+1)..];
 }
 // Load is the most "live" reading for processors; network leads with download.
 protected static Metric? HeroMetric(Device device)=>device.Metrics.FirstOrDefault(m=>m.Label=="Auslastung"&&m.Unit=="%")??CardLayouts.MainMetric(device);
 protected static bool Plottable(Metric m)=>m.Unit!="IP"&&m.TextValue==null;
 public static double? Ratio(Metric? m)
 {
  if(m?.Value is not double v||!double.IsFinite(v))return null;
  double max=m.Unit is "%" or "°C"?100:m.Unit=="B/s"?0:double.IsFinite(m.Maximum)&&m.Maximum>0?m.Maximum:0;
  return max>0?Math.Clamp(v/max,0,1):null;
 }
 protected static bool Warn(Metric m,Preferences p)=>m.Value is double v&&((m.Unit=="°C"&&v>=p.TempLimit)||(m.Label=="Auslastung"&&v>=p.LoadLimit));
 protected const string WarningColor="#F47777";
 protected void UpdateValues(Device device,Preferences p)
 {
  foreach(var pair in Values){var metric=device.Metrics.FirstOrDefault(m=>m.Id==pair.Key);pair.Value.Text=metric?.Display(p.Fahrenheit)??"—";pair.Value.ToolTip=metric==null?null:Ui.SidebarFullLabel(metric.Label)+(metric.Source==null?"":"\n"+metric.Source);}
  BorderBrush=Ui.Brush(device.Metrics.Any(m=>Warn(m,p))?WarningColor:Edge);
 }
 public abstract void Update(Device device,Preferences p);
 // Linear blend in sRGB; adequate for a single-hue ramp from the track toward the accent.
 public static Color Mix(Color a,Color b,double t){t=Math.Clamp(t,0,1);return Color.FromArgb((byte)(a.A+(b.A-a.A)*t),(byte)(a.R+(b.R-a.R)*t),(byte)(a.G+(b.G-a.G)*t),(byte)(a.B+(b.B-a.B)*t));}
 public static Color C(string hex)=>(Color)ColorConverter.ConvertFromString(hex);
}

// Label left, value right on one baseline; the shared reading row of the new designs.
public sealed class ReadingRow:Grid
{
 public ReadingRow(TextBlock label,TextBlock value)
 {
  ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});ColumnDefinitions.Add(new(){Width=GridLength.Auto});
  label.VerticalAlignment=VerticalAlignment.Center;value.VerticalAlignment=VerticalAlignment.Center;value.TextAlignment=TextAlignment.Right;value.Margin=new Thickness(8,0,0,0);
  SetColumn(value,1);Children.Add(label);Children.Add(value);
 }
}

// Stacks children like a StackPanel, but when the sidebar stretches a card (multi-column
// fill), the extra height goes to the graphics marked with Grow (each capped at MaxGrow ×
// its natural height). Only what remains becomes even spacing between rows, never a
// block of empty space above or below the content. Derives from Grid on purpose:
// ExpandedReadingsPanel hands its spare height to a lone Grid child.
public sealed class GrowPanel:Grid
{
 public static readonly DependencyProperty GrowProperty=DependencyProperty.RegisterAttached("Grow",typeof(double),typeof(GrowPanel),new FrameworkPropertyMetadata(0.0,FrameworkPropertyMetadataOptions.AffectsParentArrange));
 public static readonly DependencyProperty MaxGrowProperty=DependencyProperty.RegisterAttached("MaxGrow",typeof(double),typeof(GrowPanel),new FrameworkPropertyMetadata(4.0,FrameworkPropertyMetadataOptions.AffectsParentArrange));
 public static T Grow<T>(T element,double weight,double maxFactor=4) where T:UIElement{element.SetValue(GrowProperty,weight);element.SetValue(MaxGrowProperty,maxFactor);return element;}
 protected override Size MeasureOverride(Size available)
 {
  double width=0,height=0;
  foreach(UIElement child in InternalChildren){child.Measure(new Size(available.Width,double.PositiveInfinity));width=Math.Max(width,child.DesiredSize.Width);height+=child.DesiredSize.Height;}
  return new Size(double.IsFinite(available.Width)?available.Width:width,height);
 }
 protected override Size ArrangeOverride(Size size)
 {
  var kids=InternalChildren.Cast<UIElement>().Where(c=>c.Visibility!=Visibility.Collapsed).ToArray();
  double natural=kids.Sum(c=>c.DesiredSize.Height),extra=Math.Max(0,size.Height-natural);
  var add=new double[kids.Length];
  // Water-filling: share by weight, re-share whatever a capped child cannot take.
  for(int pass=0;pass<4&&extra>.5;pass++)
  {
   double weights=0;for(int i=0;i<kids.Length;i++){double w=(double)kids[i].GetValue(GrowProperty);if(w>0&&add[i]<Room(kids[i]))weights+=w;}
   if(weights<=0)break;double given=0;
   for(int i=0;i<kids.Length;i++){double w=(double)kids[i].GetValue(GrowProperty);if(w<=0)continue;double room=Room(kids[i])-add[i];if(room<=0)continue;double share=Math.Min(room,extra*w/weights);add[i]+=share;given+=share;}
   extra-=given;
  }
  double gap=kids.Length>1?extra/(kids.Length-1):0,top=0;
  for(int i=0;i<kids.Length;i++){double h=kids[i].DesiredSize.Height+add[i];kids[i].Arrange(new Rect(0,top,size.Width,Math.Max(0,h)));top+=h+gap;}
  return size;
 }
 private static double Room(UIElement c)=>Math.Max(0,c.DesiredSize.Height*((double)c.GetValue(MaxGrowProperty)-1));
}
