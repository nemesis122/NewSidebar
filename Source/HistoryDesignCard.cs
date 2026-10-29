using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Six independent compositions using the shared, memory-only sensor history.
public sealed class HistoryDesignCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 public string Composition {get;}
 private readonly Preferences prefs;
 private readonly MetricHistory history;
 private readonly string deviceId;
 private readonly string? primaryId;
 private readonly TextBlock number,unit;
 private readonly Brush edge;
 private readonly Dictionary<string,List<TextBlock>> values=new();
 private readonly List<(HistoryAreaGraph Graph,string Id,string Unit,double Maximum)> plots=new();
 public HistoryDesignCard(Device device,Preferences p,MetricHistory history)
 {
  prefs=p;this.history=history;deviceId=device.Id;Composition=CardLayouts.BaseLayout(p.Layout);
  bool strip=Composition=="SignalStrip",tiles=Composition=="TelemetryGrid",below=CardLayouts.ReadingsBelow(p);
  double scale=p.FontScale*CardLayouts.SpaceMultiplier(p);
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Resources["Text"]=Ui.Brush(DesignCatalog.ColorOr(p.TextColor,"#F5F7FB"));Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,"#A6AFBD"));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,"#DDE5EF"));
  CornerRadius=new CornerRadius(strip?9:15);Padding=new Thickness(strip?11:14);Margin=new Thickness(3,0,3,8);BorderThickness=new Thickness(1);edge=Ui.Brush("#29313D");BorderBrush=edge;var background=Ui.Brush("#080B10");background.Opacity=p.Opacity;Background=background;
  System.Windows.Automation.AutomationProperties.SetAutomationId(this,"HistoryComposition");Tag=Composition;
  var body=new ExpandedReadingsPanel(p,2);Child=body;
  var header=new Grid();header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});header.ColumnDefinitions.Add(new(){Width=GridLength.Auto});var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,15*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,12*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);header.Children.Add(new Viewbox{Child=heading,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var menu=new Button{Content="⋯",Background=Brushes.Transparent,BorderBrush=Brushes.Transparent,Padding=new Thickness(3,0,3,0),FontSize=14,ToolTip="Widget settings"};System.Windows.Automation.AutomationProperties.SetAutomationId(menu,"BoardWidgetSettings");menu.Click+=(_,_)=>{foreach(var window in Application.Current.Windows.OfType<SidebarWindow>()){window.OpenSettingsForDevice(deviceId);break;}};Grid.SetColumn(menu,1);header.Children.Add(menu);body.Children.Add(header);
  var hardware=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));hardware.TextWrapping=TextWrapping.Wrap;hardware.Margin=new Thickness(0,5,0,strip?7:12);body.Children.Add(hardware);
  var primary=HistoryBoardCard.FeaturedMetric(device);primaryId=primary?.Id;
  string accent=device.Category switch{"CPU"=>"#4FC3DF","RAM"=>"#6F9FFF","GPU"=>"#CA86EA","Network"=>"#F07DAD","Drives"=>"#E9B35F",_=>"#66DEC5"};if(p.GraphColors.TryGetValue(device.Category,out var custom))accent=DesignCatalog.ColorOr(custom,accent);
  var heroDigits=new StackPanel{Orientation=Orientation.Horizontal};number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",(strip?27:Composition=="FloatingChart"?42:36)*p.FontScale*Preferences.TextFactor(p.InstrumentTextScale));number.Foreground=Ui.ValueBrush(p,"#F5F7FB");number.FontWeight=FontWeights.SemiBold;number.TextTrimming=TextTrimming.None;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");heroDigits.Children.Add(number);unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",14*p.FontScale*Preferences.TextFactor(p.UnitTextScale));unit.VerticalAlignment=VerticalAlignment.Bottom;unit.Margin=new Thickness(5,0,0,5);heroDigits.Children.Add(unit);
  var hero=new Viewbox{Child=heroDigits,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,10)};
  var readings=new ExpandedReadingsPanel(p);System.Windows.Automation.AutomationProperties.SetAutomationId(readings,"HistoryReadings");
  var cells=new System.Windows.Controls.Primitives.UniformGrid{Columns=2,Margin=new Thickness(-2)};if(tiles)readings.Children.Add(cells);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,(strip?9:10)*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");var value=Value(metric,(tiles?13:strip?10:11)*scale*Preferences.TextFactor(p.ValueTextScale));
   if(tiles){label.TextWrapping=TextWrapping.Wrap;value.TextWrapping=TextWrapping.Wrap;var cell=new StackPanel{VerticalAlignment=VerticalAlignment.Center};cell.Children.Add(label);value.Margin=new Thickness(0,5,0,0);cell.Children.Add(value);var back=Ui.Brush("#17202B");back.Opacity=p.Opacity;cells.Children.Add(new Border{Child=cell,Background=back,Padding=new Thickness(9,8,9,8),Margin=new Thickness(2),CornerRadius=new CornerRadius(7)});}
   else readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,(strip?10:11)*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(0,strip?1:3,0,strip?1:3)});
  }
  var plotContent=new Grid();var first=CreatePlot(primary,accent,Composition=="DualTrace");plotContent.Children.Add(first);
  if(Composition=="DualTrace")
  {
   var secondary=SecondaryMetric(device,primary);if(secondary!=null){plotContent.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});plotContent.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});var second=CreatePlot(secondary,DesignCatalog.ColorOr(p.SecondaryGraphColor,device.Category=="Drives"?"#4FC3DF":device.Category=="RAM"||device.Category=="Network"?"#66DEC5":"#F2BE66"),true);second.Margin=new Thickness(0,10,0,0);Grid.SetRow(second,1);plotContent.Children.Add(second);}
  }
  double room=Math.Max(200,p.ColumnWidth)-(strip?46:58);bool side=(Composition is "SplitWave" or "TelemetryGrid")&&!below;
  double natural=room*(side?.6:1),requested=natural*Preferences.TextFactor(p.InstrumentScale),limit=Math.Max(40,room-(side?92:0));
  double height=Composition switch{"SignalStrip"=>26,"SplitWave"=>145,"DualTrace"=>150,"FloatingChart"=>150,"TelemetryGrid"=>82,_=>86};
  var graphic=new ResponsiveHistoryPlot(plotContent,Math.Min(requested,limit),height,Preferences.TextFactor(p.InstrumentScale)){HorizontalAlignment=HorizontalAlignment.Left};System.Windows.Automation.AutomationProperties.SetAutomationId(graphic,"CardInstrument");graphic.ToolTip=Composition=="DualTrace"?"Last 60 seconds · separate scales and units":"Last 60 seconds · live samples only";
  var content=new Grid();body.Children.Add(content);
  void AddBelow(){readings.Children.Insert(0,hero);content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});graphic.HorizontalAlignment=HorizontalAlignment.Center;graphic.Margin=new Thickness(0,0,0,10);content.Children.Add(graphic);Grid.SetRow(readings,1);content.Children.Add(readings);}
  if(below)AddBelow();
  else switch(Composition)
  {
   case "PanoramicFlow":case "DualTrace":readings.Children.Insert(0,hero);graphic.Margin=new Thickness(0,0,0,12);readings.Children.Insert(1,graphic);content.Children.Add(readings);break;
   case "SignalStrip":readings.Children.Insert(0,hero);content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.Children.Add(readings);graphic.Margin=new Thickness(0,10,0,0);Grid.SetRow(graphic,1);content.Children.Add(graphic);break;
   case "SplitWave":readings.Children.Insert(0,hero);content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1.25,GridUnitType.Star)});readings.Margin=new Thickness(0,0,10,0);content.Children.Add(readings);Grid.SetColumn(graphic,1);graphic.HorizontalAlignment=HorizontalAlignment.Right;content.Children.Add(graphic);break;
   case "FloatingChart":readings.Children.Insert(0,hero);graphic.HorizontalAlignment=HorizontalAlignment.Right;graphic.Opacity=.58;content.Children.Add(graphic);var veil=new LinearGradientBrush(new GradientStopCollection{new(Color.FromArgb((byte)(235*p.Opacity),8,11,16),0),new(Color.FromArgb((byte)(145*p.Opacity),8,11,16),.55),new(Color.FromArgb(0,8,11,16),1)},new Point(0,0),new Point(1,0));content.Children.Add(new Border{Background=veil,IsHitTestVisible=false});content.Children.Add(readings);break;
   default:content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});var top=new Grid{Margin=new Thickness(0,0,0,10)};top.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});top.ColumnDefinitions.Add(new(){Width=new GridLength(1.25,GridUnitType.Star)});hero.VerticalAlignment=VerticalAlignment.Center;top.Children.Add(hero);Grid.SetColumn(graphic,1);graphic.HorizontalAlignment=HorizontalAlignment.Right;top.Children.Add(graphic);content.Children.Add(top);Grid.SetRow(readings,1);content.Children.Add(readings);break;
  }
  Update(device,p);
 }
 private TextBlock Value(Metric metric,double size){var value=Ui.Text("—",size);value.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(prefs,"#DEE6F0");value.TextTrimming=TextTrimming.None;if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;if(!values.TryGetValue(metric.Id,out var entries))values[metric.Id]=entries=new();entries.Add(value);return value;}
 private Grid CreatePlot(Metric? metric,string color,bool legend)
 {
  var frame=new Grid();if(legend){frame.RowDefinitions.Add(new(){Height=GridLength.Auto});frame.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});var header=new Grid{Margin=new Thickness(0,0,0,4)};header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});var label=Ui.SidebarText(metric?.Label??"Unavailable",10*prefs.FontScale*CardLayouts.SpaceMultiplier(prefs)*Preferences.TextFactor(prefs.LabelTextScale),"Muted");label.Foreground=Ui.Brush(DesignCatalog.ColorOr(prefs.LabelColor,color));label.TextWrapping=TextWrapping.Wrap;header.Children.Add(label);if(metric!=null){var value=Value(metric,10*prefs.FontScale*CardLayouts.SpaceMultiplier(prefs)*Preferences.TextFactor(prefs.ValueTextScale));value.TextWrapping=TextWrapping.Wrap;value.TextAlignment=TextAlignment.Right;Grid.SetColumn(value,1);header.Children.Add(value);}frame.Children.Add(header);}
  var graph=new HistoryAreaGraph(color,DesignCatalog.ColorOr(prefs.GraphTrackColor,"#26313D"),DesignCatalog.GraphSize(prefs));System.Windows.Automation.AutomationProperties.SetAutomationId(graph,legend?"LabeledHistory":"HistoryPlot");if(metric!=null){graph.Tag=metric.Id;plots.Add((graph,metric.Id,metric.Unit,metric.Maximum));}if(legend)Grid.SetRow(graph,1);frame.Children.Add(graph);return frame;
 }
 public static Metric? SecondaryMetric(Device device,Metric? primary)
 {
  var candidates=device.Metrics.Where(m=>m.Id!=primary?.Id&&m.Unit!="IP"&&m.TextValue==null).ToArray();
  return device.Category switch{"Network"=>candidates.FirstOrDefault(m=>m.Label=="Upload"),"CPU" or "GPU"=>candidates.FirstOrDefault(m=>m.Unit=="°C")??candidates.FirstOrDefault(),"RAM"=>candidates.FirstOrDefault(m=>m.Label=="Belegt")??candidates.FirstOrDefault(),"Drives"=>candidates.FirstOrDefault(m=>m.Label=="Schreiben")??candidates.FirstOrDefault(m=>m.Label=="Lesen")??candidates.FirstOrDefault(),_=>candidates.FirstOrDefault()};
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var entries))foreach(var text in entries){text.Text=metric.Display(p.Fahrenheit);text.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(p.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split<0?display:display[..split];unit.Text=split<0?primary?.Unit??"":display[(split+1)..];number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+display;
  foreach(var plot in plots)plot.Graph.SetSamples(history.Read(deviceId,plot.Id),history.LatestTime,plot.Unit=="%"?100:plot.Unit=="°C"?Math.Max(100,plot.Maximum):0,TimeSpan.FromMilliseconds(Math.Max(2500,p.Interval*2.5)));
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
 private sealed class ResponsiveHistoryPlot:Grid
 {
  private readonly Grid content;private readonly double requestedWidth,baseHeight,plotScale;
  public ResponsiveHistoryPlot(Grid content,double requestedWidth,double baseHeight,double plotScale){this.content=content;this.requestedWidth=requestedWidth;this.baseHeight=baseHeight;this.plotScale=plotScale;RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});Children.Add(content);}
  protected override Size MeasureOverride(Size available){double width=double.IsFinite(available.Width)?Math.Max(1,available.Width)*Math.Min(1,plotScale):requestedWidth;content.MinHeight=Math.Max(8,baseHeight*plotScale);var measured=base.MeasureOverride(new Size(width,available.Height));return new Size(width,measured.Height);}
 }
}
