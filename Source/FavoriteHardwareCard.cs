using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

public sealed class FavoriteHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> readings=new();
 private readonly SidebarDesign design;
 private readonly TextBlock number,unit;
 private readonly string? primaryId;
 private readonly Meter meter;
 public FavoriteHardwareCard(Device device,Preferences prefs)
 {
  design=DesignCatalog.Resolve(prefs);string layout=CardLayouts.BaseLayout(prefs.Layout),accent=DesignCatalog.CategoryColor(design,device.Category,prefs);double scale=prefs.FontScale,readingScale=scale*CardLayouts.ReadingMultiplier(prefs.Layout)*CardLayouts.SpaceMultiplier(prefs);
  CornerRadius=new(layout=="Rackmount"?0:layout=="SideRail"?3:design.Radius);BorderThickness=new(layout=="SideRail"?3:1,1,1,1);Margin=new(3,0,3,design.Gap);Padding=new(11,10,11,10);SetResourceReference(BackgroundProperty,"Panel");BorderBrush=Ui.Brush(layout=="SideRail"?accent:design.Edge);
  var body=new ExpandedReadingsPanel(prefs,2);Child=body;var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,14*readingScale*Preferences.TextFactor(prefs.TitleTextScale)));var title=Ui.SidebarText(device.Category,12*readingScale*Preferences.TextFactor(prefs.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(prefs.TitleTextScale==1?header:new Viewbox{Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Child=header});
  var name=Ui.HardwareTitle(device.Name,11*readingScale*Preferences.TextFactor(prefs.HardwareTextScale));name.TextTrimming=TextTrimming.None;name.TextWrapping=TextWrapping.Wrap;name.Margin=new(0,4,0,10);body.Children.Add(name);
  var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(layout=="Panorama"?140:112)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
  var instrument=new StackPanel{VerticalAlignment=CardLayouts.SpaceMultiplier(prefs)>1?VerticalAlignment.Center:VerticalAlignment.Stretch,Width=layout=="Panorama"?130:102,HorizontalAlignment=HorizontalAlignment.Left};content.Children.Add(instrument);
  number=Ui.Text("—",(layout=="Digital"?26:24)*scale*Preferences.TextFactor(prefs.InstrumentTextScale));number.FontWeight=FontWeights.SemiBold;number.FontFamily=new("Cascadia Mono, Consolas");number.Foreground=Ui.ValueBrush(prefs,accent);number.TextTrimming=TextTrimming.CharacterEllipsis;number.HorizontalAlignment=HorizontalAlignment.Center;
  System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
  unit=Ui.Text("",10*scale*Preferences.TextFactor(prefs.UnitTextScale),"Muted");unit.HorizontalAlignment=HorizontalAlignment.Center;unit.Margin=new(0,4,0,0);
  var digits=new StackPanel{VerticalAlignment=VerticalAlignment.Center};digits.Children.Add(number);digits.Children.Add(unit);
  var screen=new Border{Height=layout=="Digital"?87:layout is "DotMatrix" or "Bitfield"?58:layout=="Panorama"?62:79,Padding=new(3),Child=digits,CornerRadius=new(layout=="Panorama"?28:layout=="Console"?10:2),BorderThickness=new(layout is "SideRail" or "DotMatrix" or "Tape"?0:1),BorderBrush=Ui.Brush(layout=="Digital"?accent:design.Edge)};
  if(layout=="Rackmount"){
   screen.Child=null;var rack=new Grid();rack.Children.Add(digits);foreach(var corner in new[]{(HorizontalAlignment.Left,VerticalAlignment.Top),(HorizontalAlignment.Right,VerticalAlignment.Top),(HorizontalAlignment.Left,VerticalAlignment.Bottom),(HorizontalAlignment.Right,VerticalAlignment.Bottom)})rack.Children.Add(new System.Windows.Shapes.Ellipse{Width=3,Height=3,Fill=Ui.Brush(design.Edge),HorizontalAlignment=corner.Item1,VerticalAlignment=corner.Item2,Margin=new Thickness(1)});screen.Child=rack;
  }
  if(layout=="Digital"){var background=Ui.Brush(design.Track);background.Opacity=.35*prefs.Opacity;screen.Background=background;}
  if(prefs.InstrumentTextScale!=1||prefs.UnitTextScale!=1){var screenContent=screen.Child;screen.Child=null;number.TextTrimming=TextTrimming.None;screen.Child=new Viewbox{Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,Child=screenContent};}
  instrument.Children.Add(screen);
  meter=new Meter(accent,design.Track,DesignCatalog.GraphSize(prefs),layout){Height=(layout=="Bitfield"?25:layout=="Rackmount"?22:layout=="DotMatrix"?38:layout=="Console"?15:6)*DesignCatalog.GraphSize(prefs),Margin=new(2,9,2,0)};instrument.Children.Add(meter);
  if(layout=="Tape"){instrument.Children.Clear();var tape=new Grid{Height=104};tape.ColumnDefinitions.Add(new(){Width=new GridLength(22)});tape.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});meter.Height=96;meter.Margin=new(0,0,5,0);meter.VerticalAlignment=VerticalAlignment.Center;screen.VerticalAlignment=VerticalAlignment.Center;Grid.SetColumn(screen,1);tape.Children.Add(meter);tape.Children.Add(screen);instrument.Children.Add(tape);}
  var values=new ExpandedReadingsPanel(prefs);Grid.SetColumn(values,1);content.Children.Add(values);
  foreach(var metric in device.Metrics){var label=Ui.SidebarText(metric.Label,10*readingScale*Preferences.TextFactor(prefs.LabelTextScale),"Muted");var value=Ui.Text("—",11*readingScale*Preferences.TextFactor(prefs.ValueTextScale));value.Foreground=Ui.ValueBrush(prefs,design.Text);value.FontFamily=new("Cascadia Mono, Consolas");value.TextTrimming=TextTrimming.CharacterEllipsis;if(metric.Unit=="IP"){value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;}readings[metric.Id]=value;values.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*readingScale*Preferences.TextFactor(prefs.ValueTextScale)){Margin=new(0,1,0,2)});}
  primaryId=CardLayouts.MainMetric(device)?.Id;if(primaryId==null){instrument.Visibility=Visibility.Collapsed;content.ColumnDefinitions[0].Width=new GridLength(0);}
  CardLayouts.PositionInstrument(content,instrument,values,prefs);
 }
 public void Update(Device device,Preferences prefs)
 {
  foreach(var metric in device.Metrics)if(readings.TryGetValue(metric.Id,out var value)){value.Text=metric.Display(prefs.Fahrenheit);value.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+value.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(prefs.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split>=0?display[..split]:display;unit.Text=split>=0?display[(split+1)..]:primary?.Unit??"";number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+display;
  if(primary?.Value is double raw&&double.IsFinite(raw))meter.WholeValue=(uint)Math.Clamp(Math.Floor(raw),0,uint.MaxValue);else meter.WholeValue=0;
  if(CardLayouts.BaseLayout(prefs.Layout)=="Bitfield")meter.ToolTip="Lower 16 bits of the whole-number reading · most significant bit first";
  var secondary=device.Category=="Network"?device.Metrics.FirstOrDefault(m=>m.Label=="Upload"):device.Metrics.FirstOrDefault(m=>m.Id!=primaryId&&m.Unit=="%");meter.SecondaryRatio=secondary?.Value is double other&&double.IsFinite(other)?Math.Clamp(other/Math.Max(1,secondary.Maximum),0,1):0;
  if(CardLayouts.BaseLayout(prefs.Layout)=="Rackmount")meter.ToolTip="Upper: "+(primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label))+" · Lower: "+(secondary==null?"Unavailable":Ui.SidebarFullLabel(secondary.Label));
  meter.Ratio=primary?.Value is double v&&double.IsFinite(v)?Math.Clamp(v/Math.Max(1,primary.Maximum),0,1):0;
  bool alert=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)));BorderBrush=Ui.Brush(alert?"#F47777":CardLayouts.BaseLayout(prefs.Layout)=="SideRail"?DesignCatalog.CategoryColor(design,device.Category,prefs):design.Edge);
 }
 private sealed class Meter:FrameworkElement
 {
  private readonly Brush accent,track;private readonly double strength;private readonly string style;private double ratio;
  private uint bits;private double secondRatio;
  public uint WholeValue{get=>bits;set{if(bits==value)return;bits=value;InvalidateVisual();}}
  public double SecondaryRatio{get=>secondRatio;set{if(Math.Abs(secondRatio-value)<.0001)return;secondRatio=value;InvalidateVisual();}}
  public double Ratio{get=>ratio;set{if(Math.Abs(ratio-value)<.0001)return;ratio=value;InvalidateVisual();}}
  public Meter(string accent,string track,double strength,string style){this.accent=Ui.Brush(accent);this.track=Ui.Brush(track);this.strength=strength;this.style=style;}
  protected override void OnRender(DrawingContext dc)
  {
   base.OnRender(dc);double width=Math.Max(0,ActualWidth),height=Math.Min(ActualHeight,(style=="Console"?9:3)*strength),top=(ActualHeight-height)/2;
   if(style=="Bitfield"){double cellX=width/8,cellY=ActualHeight/2;for(int i=0;i<16;i++)dc.DrawRoundedRectangle((bits&(1u<<(15-i)))!=0?accent:track,null,new Rect(i%8*cellX,i/8*cellY,Math.Max(0,cellX-3),Math.Max(0,cellY-3)),1,1);}
   else if(style=="Rackmount"){double cell=width/16,h=Math.Max(1,(ActualHeight-4)/2);for(int row=0;row<2;row++)for(int i=0;i<16;i++)dc.DrawRectangle(i/16d<(row==0?ratio:secondRatio)?accent:track,null,new Rect(i*cell,row*(h+4),Math.Max(0,cell-2),h));}
   else if(style=="DotMatrix"){const int columns=12,rows=4;double cellX=width/columns,cellY=ActualHeight/rows,radius=Math.Min(cellX,cellY)*.3;for(int i=0;i<columns*rows;i++)dc.DrawEllipse(i/(double)(columns*rows)<ratio?accent:track,null,new Point((i%columns+.5)*cellX,(i/columns+.5)*cellY),radius,radius);}
   else if(style=="Tape"){double rail=Math.Min(width*.35,5*strength);dc.DrawRoundedRectangle(track,null,new Rect(0,0,rail,ActualHeight),2,2);double fill=ActualHeight*ratio;dc.DrawRoundedRectangle(accent,null,new Rect(0,ActualHeight-fill,rail,fill),2,2);var pen=new Pen(track,strength);for(int i=0;i<=10;i++){double y=Math.Clamp(i*ActualHeight/10,1,Math.Max(1,ActualHeight-1));dc.DrawLine(pen,new Point(rail+3,y),new Point(width,y));}}
   else if(style=="Console"){const int count=14;double slot=width/count;for(int i=0;i<count;i++)dc.DrawRoundedRectangle(i/(double)count<ratio?accent:track,null,new Rect(i*slot,top,Math.Max(0,slot-2),height),1,1);}
   else {dc.DrawRoundedRectangle(track,null,new Rect(0,top,width,height),height/2,height/2);dc.DrawRoundedRectangle(accent,null,new Rect(0,top,width*ratio,height),height/2,height/2);}
  }
 }
}
