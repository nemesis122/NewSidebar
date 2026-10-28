using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

public sealed class HistoryBoardCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly TextBlock number,unit;
 private readonly string? primaryId;
 private readonly string deviceId;
 private readonly MetricHistory history;
 private readonly HistoryAreaGraph graph;
 private readonly Brush normalEdge;
 public HistoryBoardCard(Device device,Preferences prefs,MetricHistory history)
 {
  this.history=history;deviceId=device.Id;var d=DesignCatalog.Get("OLED");
  Resources["Text"]=Ui.Brush(DesignCatalog.ColorOr(prefs.TextColor,"#F3F4F6"));Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(prefs.LabelColor,"#A7ACB5"));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(prefs.HardwareNameColor,"#D9DEE7"));
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(prefs,"Segoe UI"));
  double scale=prefs.FontScale*CardLayouts.ReadingMultiplier(prefs.Layout)*CardLayouts.SpaceMultiplier(prefs);
  CornerRadius=new CornerRadius(13);Padding=new Thickness(14);Margin=new Thickness(3,0,3,8);BorderThickness=new Thickness(1);normalEdge=Ui.Brush("#292B30");BorderBrush=normalEdge;
  var back=Ui.Brush("#08090B");back.Opacity=prefs.Opacity;Background=back;
  var body=new ExpandedReadingsPanel(prefs,2);Child=body;
  var header=new Grid();header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});header.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Ui.HardwareIcon(device.Category,16*scale*Preferences.TextFactor(prefs.TitleTextScale)));var title=Ui.SidebarText(device.Category,13*scale*Preferences.TextFactor(prefs.TitleTextScale));title.FontWeight=FontWeights.SemiBold;heading.Children.Add(title);header.Children.Add(new Viewbox{Child=heading,StretchDirection=StretchDirection.DownOnly,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left});
  var menu=new Button{Background=Brushes.Transparent,BorderBrush=Brushes.Transparent,Content="⋯",Padding=new Thickness(4,0,4,0),FontSize=15,ToolTip="Widget settings",VerticalAlignment=VerticalAlignment.Top};System.Windows.Automation.AutomationProperties.SetAutomationId(menu,"BoardWidgetSettings");menu.Click+=(_,_)=>{foreach(var window in Application.Current.Windows.OfType<SidebarWindow>()){window.OpenSettingsForDevice(deviceId);break;}};Grid.SetColumn(menu,1);header.Children.Add(menu);body.Children.Add(header);
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(prefs.HardwareTextScale),prefs.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.Margin=new Thickness(0,5,0,12);body.Children.Add(name);
  var primary=FeaturedMetric(device);primaryId=primary?.Id;
  var content=new Grid();body.Children.Add(content);content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});content.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
  var readings=new ExpandedReadingsPanel(prefs);content.Children.Add(readings);
  var hero=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,6)};number=Ui.Text("—",30*prefs.FontScale*Preferences.TextFactor(prefs.InstrumentTextScale));number.FontWeight=FontWeights.SemiBold;number.Foreground=Ui.ValueBrush(prefs,"#F8FAFC");System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");number.TextTrimming=TextTrimming.None;hero.Children.Add(number);
  unit=Ui.Text("",17*prefs.FontScale*Preferences.TextFactor(prefs.UnitTextScale));unit.Margin=new Thickness(5,0,0,4);unit.VerticalAlignment=VerticalAlignment.Bottom;hero.Children.Add(unit);readings.Children.Add(new Viewbox{Child=hero,StretchDirection=StretchDirection.DownOnly,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left});
  foreach(var metric in device.Metrics){var label=Ui.SidebarText(metric.Label,9*scale*Preferences.TextFactor(prefs.LabelTextScale),"Muted");var value=Ui.Text("—",10*scale*Preferences.TextFactor(prefs.ValueTextScale));value.FontFamily=Ui.Font(prefs,"Segoe UI");value.Foreground=Ui.ValueBrush(prefs,"#DFE3EA");value.TextTrimming=TextTrimming.None;if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,10*scale*Preferences.TextFactor(prefs.ValueTextScale)){Margin=new Thickness(0,1,0,1)});}
  string accent=device.Category switch{"CPU"=>"#4FC3DF","RAM"=>"#6F9FFF","GPU"=>"#CA86EA","Network"=>"#F07DAD","Drives"=>"#E9B35F",_=>"#66DEC5"};if(prefs.GraphColors.TryGetValue(device.Category,out var custom))accent=DesignCatalog.ColorOr(custom,accent);
  graph=new HistoryAreaGraph(accent,DesignCatalog.ColorOr(prefs.GraphTrackColor,"#292D35"),DesignCatalog.GraphSize(prefs)){ToolTip="Last 60 seconds · live samples only"};
  double requested=Preferences.TextFactor(prefs.InstrumentScale),naturalWidth=(Math.Max(200,prefs.ColumnWidth)-60)*.52,width=Math.Min(naturalWidth*requested,Math.Max(40,Math.Max(200,prefs.ColumnWidth)-100));
  var chart=new Grid{Width=width,Height=80*width/naturalWidth,Margin=new Thickness(12,4,0,0),VerticalAlignment=prefs.Columns>1?VerticalAlignment.Center:VerticalAlignment.Bottom};chart.Children.Add(graph);Grid.SetColumn(chart,1);content.Children.Add(chart);System.Windows.Automation.AutomationProperties.SetAutomationId(chart,"CardInstrument");
  if(CardLayouts.SpaceMultiplier(prefs)>1){content.ColumnDefinitions.Clear();content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});Grid.SetColumn(chart,0);chart.HorizontalAlignment=HorizontalAlignment.Center;chart.VerticalAlignment=VerticalAlignment.Top;chart.Margin=new Thickness(0,0,0,10);Grid.SetRow(readings,1);}
  Update(device,prefs);
 }
 public static Metric? FeaturedMetric(Device device)=>device.Category=="Network"?device.Metrics.FirstOrDefault(m=>m.Label=="Download"&&m.Unit!="IP")??device.Metrics.FirstOrDefault(m=>m.Unit!="IP"):device.Metrics.FirstOrDefault(m=>m.Unit=="%"&&(m.Label=="Auslastung"||m.Label=="Belegt"))??CardLayouts.MainMetric(device);
 public void Update(Device device,Preferences prefs)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var value)){value.Text=metric.Display(prefs.Fahrenheit);value.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+value.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(prefs.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split<0?display:display[..split];unit.Text=split<0?primary?.Unit??"":display[(split+1)..];number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+display;
  graph.SetSamples(primaryId==null?Array.Empty<HistorySample>():history.Read(deviceId,primaryId),history.LatestTime,primary?.Unit=="%"?100:primary?.Unit=="°C"?Math.Max(100,primary.Maximum):0,TimeSpan.FromMilliseconds(Math.Max(2500,prefs.Interval*2.5)));
  BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)))?Ui.Brush("#F47777"):normalEdge;
 }
}

public sealed class HistoryAreaGraph:FrameworkElement
{
 private readonly Brush ink,fill,track;private readonly double thickness;
 private IReadOnlyList<HistorySample> samples=Array.Empty<HistorySample>();private DateTime now;private double fixedMaximum;private TimeSpan maximumGap;
 public int SampleCount=>samples.Count;
 public HistoryAreaGraph(string accent,string track,double thickness){var color=(Color)ColorConverter.ConvertFromString(accent);ink=new SolidColorBrush(color);fill=new LinearGradientBrush(Color.FromArgb(90,color.R,color.G,color.B),Color.FromArgb(24,color.R,color.G,color.B),new Point(0,0),new Point(0,1));this.track=Ui.Brush(track);this.thickness=thickness;ClipToBounds=true;}
 public void SetSamples(IReadOnlyList<HistorySample> samples,DateTime now,double maximum,TimeSpan maximumGap){this.samples=samples;this.now=now;fixedMaximum=maximum;this.maximumGap=maximumGap;InvalidateVisual();}
 protected override void OnRender(DrawingContext dc)
 {
  base.OnRender(dc);double width=ActualWidth-4,height=ActualHeight-4;if(width<=0||height<=0)return;
  dc.DrawLine(new Pen(track,1),new Point(2,height+2),new Point(width+2,height+2));
  double max=fixedMaximum>0?fixedMaximum:Math.Max(1,samples.Where(s=>s.Value.HasValue&&double.IsFinite(s.Value.Value)).Select(s=>Math.Max(0,s.Value!.Value)).DefaultIfEmpty(1).Max()*1.15);
  var segment=new List<Point>();DateTime? previous=null;
  void DrawSegment(){if(segment.Count==0)return;if(segment.Count==1){dc.DrawEllipse(ink,null,segment[0],1.6*thickness,1.6*thickness);segment.Clear();return;}var area=new StreamGeometry();using(var c=area.Open()){c.BeginFigure(new Point(segment[0].X,height+2),true,true);foreach(var point in segment)c.LineTo(point,true,false);c.LineTo(new Point(segment[^1].X,height+2),true,false);}area.Freeze();dc.DrawGeometry(fill,null,area);var line=new StreamGeometry();using(var c=line.Open()){c.BeginFigure(segment[0],false,false);foreach(var point in segment.Skip(1))c.LineTo(point,true,false);}line.Freeze();dc.DrawGeometry(null,new Pen(ink,1.5*thickness){LineJoin=PenLineJoin.Round},line);segment.Clear();}
  foreach(var sample in samples){double age=(now-sample.Time).TotalSeconds;if(age<0||age>60)continue;if(!sample.Value.HasValue||!double.IsFinite(sample.Value.Value)){DrawSegment();previous=null;continue;}if(previous.HasValue&&sample.Time-previous.Value>maximumGap)DrawSegment();segment.Add(new Point(2+width*(1-age/60),2+height*(1-Math.Clamp(sample.Value.Value/max,0,1))));previous=sample.Time;}DrawSegment();
 }
}

