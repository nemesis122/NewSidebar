using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// WPF UI resources are scoped: legacy layouts keep their existing styles.
public static class FluentSurface
{
 private static ResourceDictionary? resources;
 public static ResourceDictionary Resources=>resources??=Create();
 private static ResourceDictionary Create(){var r=new ResourceDictionary();r.MergedDictionaries.Add(new Wpf.Ui.Markup.ThemesDictionary{Theme=Wpf.Ui.Appearance.ApplicationTheme.Dark});r.MergedDictionaries.Add(new Wpf.Ui.Markup.ControlsDictionary());foreach(var t in new[]{typeof(Button),typeof(CheckBox),typeof(ComboBox),typeof(ComboBoxItem),typeof(Slider),typeof(TextBox),typeof(ListView),typeof(ListViewItem),typeof(ListBox),typeof(ListBoxItem),typeof(GridViewColumnHeader),typeof(ScrollViewer),typeof(ProgressBar),typeof(ToolTip),typeof(Expander),typeof(System.Windows.Controls.Primitives.ToggleButton),typeof(System.Windows.Controls.Primitives.RepeatButton),typeof(System.Windows.Controls.Primitives.Thumb),typeof(System.Windows.Controls.Primitives.Track)})r[t]=Application.Current.TryFindResource(t) is Style existing?existing:new Style(t);return r;}
}
public sealed class FluentHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly TextBlock number,unit;
 private readonly ProgressBar meter;
 private readonly string? primaryId;
 private readonly Wpf.Ui.Controls.Card surface;
 private readonly Brush edge;
 public FluentHardwareCard(Device device,Preferences prefs)
 {
  string layout=CardLayouts.BaseLayout(prefs.Layout);bool board=layout=="WidgetBoard",compact=layout=="FluentUICompact",center=layout=="CommandCenter";
  var d=DesignCatalog.Resolve(prefs);if(board){var light=DesignCatalog.Get("Light");d=light with{Text=DesignCatalog.ColorOr(prefs.TextColor,light.Text),Muted=DesignCatalog.ColorOr(prefs.LabelColor,light.Muted),Track=DesignCatalog.ColorOr(prefs.GraphTrackColor,light.Track)};}
  Resources["Text"]=Ui.Brush(d.Text);Resources["Muted"]=Ui.Brush(d.Muted);Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(prefs.HardwareNameColor,d.Text));string accent=DesignCatalog.CategoryColor(d,device.Category,prefs);double scale=prefs.FontScale*CardLayouts.ReadingMultiplier(prefs.Layout)*CardLayouts.SpaceMultiplier(prefs);
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(prefs,d.Font));Margin=new Thickness(3,0,3,d.Gap);Resources.MergedDictionaries.Add(FluentSurface.Resources);
  var body=new ExpandedReadingsPanel(prefs,2);var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,15*scale*Preferences.TextFactor(prefs.TitleTextScale)));
  var title=Ui.SidebarText(device.Category,12*scale*Preferences.TextFactor(prefs.TitleTextScale));title.FontWeight=FontWeights.SemiBold;title.Foreground=Ui.Brush(d.Text);header.Children.Add(title);body.Children.Add(new Viewbox{StretchDirection=StretchDirection.DownOnly,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left,Child=header});
  var name=Ui.HardwareTitle(device.Name,11*scale*Preferences.TextFactor(prefs.HardwareTextScale),prefs.HardwareNames.ContainsKey(device.Id));name.Foreground=Ui.Brush(DesignCatalog.ColorOr(prefs.HardwareNameColor,d.Text));name.TextWrapping=TextWrapping.Wrap;name.Margin=new Thickness(0,5,0,10);body.Children.Add(name);
  var primary=CardLayouts.MainMetric(device);primaryId=primary?.Id;
  var hero=new StackPanel{Width=140,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,9)};
  var digits=new StackPanel{Orientation=Orientation.Horizontal};number=Ui.Text("—",(layout=="FluentUIHero"?43:board?34:32)*prefs.FontScale*Preferences.TextFactor(prefs.InstrumentTextScale));number.Foreground=Ui.ValueBrush(prefs,accent);number.FontWeight=FontWeights.SemiBold;number.TextTrimming=TextTrimming.None;System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");digits.Children.Add(number);
  unit=Ui.Text("",11*prefs.FontScale*Preferences.TextFactor(prefs.UnitTextScale));unit.Foreground=Ui.Brush(d.Muted);unit.Margin=new Thickness(6,0,0,5);unit.VerticalAlignment=VerticalAlignment.Bottom;digits.Children.Add(unit);
  var fitted=new Viewbox{Child=digits,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left};hero.Children.Add(fitted);
  meter=new ProgressBar{Minimum=0,Maximum=1,Height=4*DesignCatalog.GraphSize(prefs),BorderThickness=new Thickness(0),Background=Ui.Brush(d.Track),Foreground=Ui.Brush(accent),Margin=new Thickness(0,8,0,0),IsHitTestVisible=false};hero.Children.Add(meter);
  double graphicScale=Math.Min(Preferences.TextFactor(prefs.InstrumentScale),Math.Max(40,Math.Max(200,prefs.ColumnWidth)-60)/140);if(graphicScale!=1)hero.LayoutTransform=new ScaleTransform(graphicScale,graphicScale);
  System.Windows.Automation.AutomationProperties.SetAutomationId(hero,"CardInstrument");var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});content.RowDefinitions.Add(new(){Height=GridLength.Auto});content.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});content.Children.Add(hero);body.Children.Add(content);
  var readings=new ExpandedReadingsPanel(prefs);Grid.SetRow(readings,1);content.Children.Add(readings);var tiles=new System.Windows.Controls.Primitives.UniformGrid{Columns=2};if(center)readings.Children.Add(tiles);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(prefs.LabelTextScale));label.Foreground=Ui.Brush(d.Muted);var value=Ui.Text("—",11*scale*Preferences.TextFactor(prefs.ValueTextScale));value.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(prefs,d.Text);value.TextTrimming=TextTrimming.None;if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;values[metric.Id]=value;
   if(center){var tile=Ui.Brush(d.Track);tile.Opacity=.35*prefs.Opacity;var cell=new StackPanel{VerticalAlignment=VerticalAlignment.Center};label.TextWrapping=TextWrapping.Wrap;cell.Children.Add(label);value.TextWrapping=TextWrapping.Wrap;value.Margin=new Thickness(0,5,0,0);cell.Children.Add(value);tiles.Children.Add(new Border{Child=cell,Background=tile,Padding=new Thickness(8,7,8,7),CornerRadius=new CornerRadius(7),Margin=new Thickness(2)});}else readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(prefs.ValueTextScale)){Margin=new Thickness(0,compact?1:3,0,compact?1:3)});
  }
  var back=Ui.Brush(d.Panel);back.Opacity=prefs.Opacity;edge=Ui.Brush(d.Edge);
  surface=new Wpf.Ui.Controls.Card{VerticalAlignment=VerticalAlignment.Stretch,HorizontalAlignment=HorizontalAlignment.Stretch,Content=body,FontFamily=Ui.Font(prefs,d.Font),Padding=new Thickness(compact?11:16),Background=back,BorderBrush=edge,BorderThickness=new Thickness(1),HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Stretch};
  if(center){var color=(Color)ColorConverter.ConvertFromString(accent);surface.Background=new LinearGradientBrush(new GradientStopCollection{new(Color.FromArgb((byte)(255*prefs.Opacity),((SolidColorBrush)Ui.Brush(d.Panel)).Color.R,((SolidColorBrush)Ui.Brush(d.Panel)).Color.G,((SolidColorBrush)Ui.Brush(d.Panel)).Color.B),0),new(Color.FromArgb((byte)(95*prefs.Opacity),color.R,color.G,color.B),1)},new Point(0,0),new Point(1,1));}
  Child=surface;Update(device,prefs);
 }
 public void Update(Device device,Preferences prefs)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var text)){text.Text=metric.Display(prefs.Fahrenheit);text.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text.Text+(metric.Source==null?"":"\n"+metric.Source);}
  var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);string display=primary?.Display(prefs.Fahrenheit)??"—";int split=display.LastIndexOf(' ');number.Text=split<0?display:display[..split];unit.Text=split<0?primary?.Unit??"":display[(split+1)..];number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+display;
  meter.Value=primary?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,primary.Maximum),0,1):0;
  surface.BorderBrush=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)))?Ui.Brush("#F47777"):edge;
 }
}








