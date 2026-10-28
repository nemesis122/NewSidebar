using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Aurora;
public record CardLayout(string Key,string Name,string Description){public override string ToString()=>Name;}
public static class CardLayouts
{
 private static readonly CardLayout[] BaseLayouts=new[]{new CardLayout("Original","Original","Klassische Sensorliste"),new("Modern","Karten","Große Hauptzahl mit Werteliste"),new("Compact","Kompakt","Dichte Tabellen"),new("Dashboard","Dashboard","Hauptkennzahl und Statistikraster"),new("Gauges","Instrumente","Momentanwert als Rundinstrument"),new("Text","Textansicht","Rahmenlose Texttabellen"),new("Tiles","Wertekacheln","Jeder Wert in einer Kachel"),new("Bars","Balken","Große Pegelbalken für alle Werte"),new("OrbitGauge","Orbit","Rundinstrument mit Messwerten rechts"),new("TwinRings","Doppelring","Zwei konzentrische Ringe mit Messwerten rechts"),new("ArcGauge","Halbkreis","Kompaktes Halbkreis-Instrument mit klarer Werteliste"),new("SegmentGauge","Segmentring","Segmentiertes Rundinstrument mit Messwerten rechts"),new("SpeedGauge","Tachometer","Offener 270-Grad-Bogen mit Messwerten rechts"),new("ColumnGauge","Sensor-Säule","Senkrechte Sensor-Säule mit Messwerten rechts"),new("Console","Messpult","Kompaktes Messpult mit Segmentanzeige"),new("Digital","Digital","Digitale Hauptanzeige mit präziser Werteliste"),new("SideRail","Seitenbalken","Schlichte Messkarte mit seitlicher Akzentlinie"),new("HorseshoeGauge","Hufeisen","Offenes Rundinstrument mit Positionspunkt"),new("DotMatrix","Punktmatrix","LED-Punktmatrix mit digitalem Hauptwert"),new("Panorama","Panorama","Breite Hauptanzeige mit kompakter Werteliste"),new("Tape","Messstreifen","Vertikaler Messstreifen mit Skalenstrichen"),new("HexCore","Hex-Core","Sechseckiges Instrument mit Werteliste"),new("Bitfield","Bitfield","Digitale Hauptzahl mit echter 16-Bit-Darstellung"),new("Rackmount","Rackmount","Server-Panel mit zwei LED-Pegelreihen"),new("Fluent","Fluent","Weiche Flächen und klare Typografie"),new("Glass","Glass","Transparente Karten mit feinen Konturen"),new("Bento","Bento","Kompakte Messwert-Kacheln"),new("Precision","Precision","Präzise Skala und ausgerichtete Werte"),new("NeonEdge","Neon Edge","Lichtkonturen mit geometrischem Instrument"),new("OrbitStudio","Orbit Studio","Orbit-Instrument mit seitlichen Messwerten"),new("MetroMosaic","Metro Mosaic","Asymmetrische Messwert-Kacheln"),new("SensorMap","Sensor Map","Verbundene Sensorfelder um einen Chip"),new("SplitPanel","Split Panel","Geteilte Hardware- und Datenflächen"),new("Typographic","Typographic","Große Typografie mit ruhiger Werteliste"),new("Blueprint","Blueprint","Technische Zeichnung mit Messfeldern"),new("FluentUI","Fluent UI","Klare Fluent-Karten mit großer Hauptzahl"),new("FluentUICompact","Fluent UI Compact","Kompakte Fluent-Karten"),new("FluentUIHero","Fluent UI Hero","Große Hauptzahl und ruhige Messwertliste"),new("WidgetBoard","Widget Board","Helle Widget-Flächen mit kompakten Zusatzwerten"),new("CommandCenter","Command Center","Dashboard mit Glasflächen und Datenkacheln"),new("WidgetBoardBlack","Widget Board Black","Schwarze Referenz-Karten mit echtem 60-Sekunden-Verlauf")};
 public static IReadOnlyList<CardLayout> All {get;}=Array.AsReadOnly(BaseLayouts);
 public static CardLayout Get(string? key){if(key?.EndsWith(".Large",StringComparison.Ordinal)==true)key=key[..^6];return All.FirstOrDefault(l=>l.Key==key)??All[0];}
 public static string BaseLayout(string? key){string value=Get(key).Key;return value.EndsWith(".Large",StringComparison.Ordinal)?value[..^6]:value;}
 public static double ReadingMultiplier(string? key)=>1;
 public static double SpaceMultiplier(Preferences prefs)=>prefs.ExpandMultiColumnReadings&&prefs.Columns>1?1.2:1;
 public static void PositionInstrument(Grid content,FrameworkElement? instrument,FrameworkElement readings,Preferences prefs)
 {
  if(instrument!=null)System.Windows.Automation.AutomationProperties.SetAutomationId(instrument,"CardInstrument");
  double requested=Preferences.TextFactor(prefs.InstrumentScale);
  if(instrument!=null&&requested!=1){
   double natural=double.IsFinite(instrument.Width)?instrument.Width:instrument is Panel figurePanel&&figurePanel.Children.OfType<FrameworkElement>().FirstOrDefault() is FrameworkElement graphic&&double.IsFinite(graphic.Width)?graphic.Width+instrument.Margin.Left+instrument.Margin.Right:112;
   double room=Math.Max(40,Math.Max(200,prefs.ColumnWidth)-48-(SpaceMultiplier(prefs)==1?80:0));
   double applied=Math.Min(requested,Math.Max(1,room/natural));
   instrument.LayoutTransform=new ScaleTransform(applied,applied);
   if(SpaceMultiplier(prefs)==1)content.ColumnDefinitions[0].Width=new GridLength(content.ColumnDefinitions[0].Width.Value*applied);
  }
  if(prefs.Columns>1){readings.VerticalAlignment=VerticalAlignment.Stretch;if(instrument!=null)instrument.VerticalAlignment=VerticalAlignment.Center;}
  if(SpaceMultiplier(prefs)==1)return;
  content.ColumnDefinitions.Clear();content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
  content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});
  Grid.SetColumn(readings,0);Grid.SetRow(readings,1);readings.VerticalAlignment=VerticalAlignment.Stretch;
  if(instrument!=null){Grid.SetColumn(instrument,0);Grid.SetRow(instrument,0);instrument.HorizontalAlignment=HorizontalAlignment.Center;instrument.VerticalAlignment=VerticalAlignment.Top;instrument.Margin=new Thickness(0,0,0,8);}
 }
 public static Metric? MainMetric(Device device)=>device.Category=="Network"?device.Metrics.FirstOrDefault(m=>m.Unit!="IP"):device.Metrics.FirstOrDefault(m=>m.Unit=="°C" && m.Label.StartsWith("Temperatur",StringComparison.OrdinalIgnoreCase))??device.Metrics.FirstOrDefault(m=>m.Unit=="°C")??device.Metrics.FirstOrDefault();
 public static IHardwareCard Create(Device d,Preferences p,MetricHistory? history=null)
 {
  string layout=BaseLayout(p.Layout);
  if(layout=="WidgetBoardBlack")return new HistoryBoardCard(d,p,history??new MetricHistory());
  if(layout is "FluentUI" or "FluentUICompact" or "FluentUIHero" or "WidgetBoard" or "CommandCenter")return new FluentHardwareCard(d,p);
  if(layout is "MetroMosaic" or "SensorMap" or "SplitPanel" or "Typographic" or "Blueprint")return new SpatialHardwareCard(d,p);
  if(layout is "Fluent" or "Glass" or "Bento" or "Precision" or "NeonEdge" or "OrbitStudio")return new ContemporaryHardwareCard(d,p);
  if(layout is "Console" or "Digital" or "SideRail" or "DotMatrix" or "Panorama" or "Tape" or "Bitfield" or "Rackmount")return new FavoriteHardwareCard(d,p);
  if(layout is "Gauges" or "OrbitGauge" or "TwinRings" or "ArcGauge" or "SegmentGauge" or "SpeedGauge" or "ColumnGauge" or "HorseshoeGauge" or "HexCore")return new RadialHardwareCard(d,p);
  if(layout is "Modern" or "Dashboard" or "Gauges" && MainMetric(d) is Metric primary)d=d with{Metrics=new[]{primary}.Concat(d.Metrics.Where(m=>m.Id!=primary.Id)).ToList()};
  return layout is "Original" or "Modern"?new HardwareCard(d,p):new AlternateHardwareCard(d,p);
 }
}
public interface IHardwareCard {FrameworkElement View {get;} void Update(Device d,Preferences p);}
public sealed class AlternateHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,List<(TextBlock Text,Border? Fill,Border? Track)>> values=new();
 private readonly SidebarDesign design;
 private readonly string accent,layout;
 private readonly double scale,readingScale;
 private readonly Preferences preferences;
 private TextBlock? gaugeNumber,gaugeUnit;
 private readonly System.Windows.Shapes.Path arc=new(){StrokeThickness=6,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round};
 public AlternateHardwareCard(Device device,Preferences prefs)
 {
  preferences=prefs;design=DesignCatalog.Resolve(prefs);System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(prefs,design.Font));accent=DesignCatalog.CategoryColor(design,device.Category,prefs);layout=CardLayouts.BaseLayout(prefs.Layout);scale=prefs.FontScale;readingScale=scale*CardLayouts.ReadingMultiplier(prefs.Layout)*CardLayouts.SpaceMultiplier(prefs);
  CornerRadius=new(layout is "Text" or "Compact"?0:design.Radius);BorderThickness=layout=="Text"?new Thickness(0,0,0,1):new Thickness(1);
  Padding=new Thickness(layout is "Text" or "Compact"?9:13);Margin=new Thickness(3,0,3,layout=="Compact"?5:10);SetResourceReference(BackgroundProperty,"Panel");SetResourceReference(BorderBrushProperty,"Edge");
  var body=new ExpandedReadingsPanel(prefs,2);Child=body;var header=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,4)};header.Children.Add(Ui.HardwareIcon(device.Category,15*readingScale*Preferences.TextFactor(prefs.TitleTextScale)));var title=Ui.SidebarText(device.Category,13*readingScale*Preferences.TextFactor(prefs.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(prefs.TitleTextScale==1?header:new Viewbox{Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Child=header});
  var name=Ui.HardwareTitle(device.Name,11*readingScale*Preferences.TextFactor(prefs.HardwareTextScale),prefs.HardwareNames.ContainsKey(device.Id));name.TextTrimming=TextTrimming.None;name.TextWrapping=TextWrapping.Wrap;name.ToolTip=name.Text;name.Margin=new Thickness(0,0,0,layout=="Compact"?4:10);body.Children.Add(name);
  if(layout=="Gauges" && device.Metrics.Count>0)Gauge(body,device);
  else if(layout=="Dashboard" && device.Metrics.Count>0 && CardLayouts.MainMetric(device)!=null)Dashboard(body,device);
  else if(layout=="Tiles")Tiles(body,device.Metrics);
  else foreach(var m in device.Metrics)Row(body,m,layout=="Bars",layout=="Compact");
 }
 private TextBlock Value(Metric m,double size,bool primary=false){var v=Ui.Text("—",size*(primary?scale:readingScale)*Preferences.TextFactor(primary?preferences.InstrumentTextScale:preferences.ValueTextScale));v.Foreground=Ui.ValueBrush(preferences,design.Text);v.TextTrimming=TextTrimming.CharacterEllipsis;v.FontFamily=Ui.Font(preferences,design.Font=="Consolas"?"Consolas":"Cascadia Mono, Consolas");if(!values.TryGetValue(m.Id,out var entries))values[m.Id]=entries=new();entries.Add((v,null,null));return v;}
 private void Row(Panel parent,Metric m,bool bar,bool compact=false)
 {
  if(m.Unit=="IP"){var ipRow=new StackPanel{Margin=new Thickness(0,3,0,3)};ipRow.Children.Add(Ui.SidebarText(m.Label,11*readingScale*Preferences.TextFactor(preferences.LabelTextScale),"Muted"));var ipValue=Value(m,compact?11:12);ipValue.TextWrapping=TextWrapping.Wrap;ipValue.TextTrimming=TextTrimming.None;ipRow.Children.Add(ipValue);parent.Children.Add(ipRow);return;}
  var row=new DockPanel{Margin=new Thickness(0,compact?1:bar?5:3,0,compact?1:3),ClipToBounds=true};var v=Value(m,compact?11:12);DockPanel.SetDock(v,Dock.Right);row.Children.Add(v);row.SizeChanged+=(_,_)=>v.MaxWidth=Math.Max(30,row.ActualWidth*.68);var label=Ui.SidebarText(m.Label,11*readingScale*Preferences.TextFactor(preferences.LabelTextScale),"Muted");label.TextTrimming=TextTrimming.CharacterEllipsis;label.ToolTip=Ui.SidebarFullLabel(m.Label);label.Margin=new Thickness(0,0,10,0);if(preferences.LabelTextScale!=1||preferences.ValueTextScale!=1){row.Children.Clear();parent.Children.Add(new RadialHardwareCard.ReadoutPanel(label,v,(compact?11:12)*readingScale*Preferences.TextFactor(preferences.ValueTextScale)){Margin=row.Margin});}else{row.Children.Add(label);parent.Children.Add(row);}
  if(bar){var track=new Border{Background=Ui.Brush(design.Track),Height=8*DesignCatalog.GraphSize(preferences)*Preferences.TextFactor(preferences.InstrumentScale),CornerRadius=new(4),Margin=new Thickness(0,0,0,6)};var fill=new Border{Background=Ui.Brush(accent),Height=8*DesignCatalog.GraphSize(preferences)*Preferences.TextFactor(preferences.InstrumentScale),Width=0,HorizontalAlignment=HorizontalAlignment.Left,CornerRadius=new(4)};track.Child=fill;parent.Children.Add(track);values[m.Id][^1]=(v,fill,track);track.SizeChanged+=(_,_)=>{if(fill.Tag is double ratio)fill.Width=track.ActualWidth*ratio;};}
 }
 private void Tiles(Panel parent,IEnumerable<Metric> metrics)
 {
  var grid=new System.Windows.Controls.Primitives.UniformGrid{Columns=2,Margin=new Thickness(-3)};
  foreach(var m in metrics){var cell=new StackPanel();var label=Ui.SidebarText(m.Label,9*readingScale*Preferences.TextFactor(preferences.LabelTextScale),"Muted");label.TextTrimming=TextTrimming.CharacterEllipsis;label.ToolTip=Ui.SidebarFullLabel(m.Label);cell.Children.Add(label);var v=Value(m,17);v.Foreground=Ui.ValueBrush(preferences,accent);v.Margin=new Thickness(0,6,0,0);if(readingScale>scale||m.Unit=="IP"){label.TextWrapping=TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;v.TextWrapping=TextWrapping.Wrap;v.TextTrimming=TextTrimming.None;}cell.Children.Add(v);grid.Children.Add(new Border{Padding=new Thickness(9),Margin=new Thickness(3),CornerRadius=new(6),BorderBrush=Ui.Brush(design.Edge),BorderThickness=new Thickness(1),Child=cell});}parent.Children.Add(grid);
 }
 private void Dashboard(Panel parent,Device device)
 {
  var hero=Value(device.Metrics[0],32,true);System.Windows.Automation.AutomationProperties.SetAutomationId(hero,"PrimaryMetric");hero.Foreground=Ui.ValueBrush(preferences,accent);hero.FontWeight=FontWeights.SemiBold;parent.Children.Add(hero);var label=Ui.SidebarText(device.Metrics[0].Label,10*scale*Preferences.TextFactor(preferences.LabelTextScale),"Muted");label.Margin=new Thickness(0,2,0,10);parent.Children.Add(label);var grid=new System.Windows.Controls.Primitives.UniformGrid{Columns=2};
  foreach(var m in device.Category=="Network"?device.Metrics:device.Metrics.Skip(1)){var cell=new StackPanel{Margin=new Thickness(0,5,6,6)};var v=Value(m,15);cell.Children.Add(v);var text=Ui.SidebarText(m.Label,9*readingScale*Preferences.TextFactor(preferences.LabelTextScale),"Muted");text.TextTrimming=TextTrimming.CharacterEllipsis;text.ToolTip=Ui.SidebarFullLabel(m.Label);text.Margin=new Thickness(0,2,0,0);if(readingScale>scale||m.Unit=="IP"){text.TextWrapping=TextWrapping.Wrap;text.TextTrimming=TextTrimming.None;v.TextWrapping=TextWrapping.Wrap;v.TextTrimming=TextTrimming.None;}cell.Children.Add(text);grid.Children.Add(cell);}parent.Children.Add(grid);
 }
 private void Gauge(Panel parent,Device device)
 {
  var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(112)});grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var dial=new Grid{Width=100,Height=100,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,0,12,0)};dial.Children.Add(new Ellipse{Stroke=Ui.Brush(design.Track),StrokeThickness=6,Margin=new Thickness(6)});arc.Stroke=Ui.Brush(accent);dial.Children.Add(arc);
  var center=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};var v=Value(device.Metrics[0],18);v.HorizontalAlignment=HorizontalAlignment.Center;v.Foreground=Ui.ValueBrush(preferences,accent);v.MaxWidth=88;center.Children.Add(v);gaugeNumber=v;gaugeUnit=Ui.Text(device.Metrics[0].Unit,10*scale*Preferences.TextFactor(preferences.UnitTextScale),"Muted");gaugeUnit.HorizontalAlignment=HorizontalAlignment.Center;gaugeUnit.Margin=new Thickness(0,2,0,0);center.Children.Add(gaugeUnit);dial.Children.Add(center);grid.Children.Add(dial);var list=new StackPanel();Grid.SetColumn(list,1);
  if(device.Category=="Network")foreach(var m in device.Metrics){var cell=new StackPanel{Margin=new Thickness(0,0,0,7)};var label=Ui.SidebarText(m.Label,10*scale*Preferences.TextFactor(preferences.LabelTextScale),"Muted");label.TextWrapping=TextWrapping.Wrap;cell.Children.Add(label);cell.Children.Add(Value(m,11));list.Children.Add(cell);}
  else foreach(var m in device.Metrics.Skip(1))Row(list,m,false,true);
  System.Windows.Automation.AutomationProperties.SetAutomationId(gaugeNumber!,"PrimaryMetric");grid.Children.Add(list);parent.Children.Add(grid);
 }
 public void Update(Device device,Preferences prefs)
 {
  bool alert=false;
  foreach(var m in device.Metrics){if(!values.TryGetValue(m.Id,out var entries))continue;foreach(var visual in entries){visual.Text.Text=m.Display(prefs.Fahrenheit);visual.Text.ToolTip=m.Source??m.Id;if(visual.Fill!=null && visual.Track!=null){double ratio=m.Value.HasValue?Math.Clamp(m.Value.Value/Math.Max(1,m.Maximum),0,1):0;visual.Fill.Tag=ratio;visual.Fill.Width=Math.Max(0,visual.Track.ActualWidth*ratio);}}if(m.Value.HasValue && ((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)))alert=true;}
  BorderBrush=Ui.Brush(alert?"#F47777":design.Edge);
  if(layout=="Gauges" && CardLayouts.MainMetric(device) is Metric first){string formatted=first.Display(prefs.Fahrenheit);int split=formatted.LastIndexOf(' ');if(gaugeNumber!=null)gaugeNumber.Text=split>=0?formatted.Substring(0,split):formatted;if(gaugeUnit!=null)gaugeUnit.Text=split>=0?formatted.Substring(split+1):first.Unit;if(!first.Value.HasValue){arc.Data=null;return;}double ratio=Math.Clamp(first.Value.Value/Math.Max(1,first.Maximum),0,.99999),angle=ratio*Math.PI*2;var figure=new PathFigure{StartPoint=new Point(50,9),IsClosed=false};figure.Segments.Add(new ArcSegment(new Point(50+41*Math.Sin(angle),50-41*Math.Cos(angle)),new Size(41,41),0,ratio>.5,SweepDirection.Clockwise,true));arc.Data=new PathGeometry(new[]{figure});}
 }
}





