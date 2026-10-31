using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// A deliberately dense layout: no repeated hero value and no reserved chart space.
public sealed class NanoHardwareCard:Border,IHardwareCard
{
 public FrameworkElement View=>this;
 private readonly Dictionary<string,TextBlock> values=new();
 private readonly SidebarDesign design;
 public NanoHardwareCard(Device device,Preferences p)
 {
  design=DesignCatalog.Resolve(p);double scale=p.FontScale;
  System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(p,"Segoe UI"));
  Resources["Text"]=Ui.Brush(DesignCatalog.ColorOr(p.TextColor,design.Text));Resources["Muted"]=Ui.Brush(DesignCatalog.ColorOr(p.LabelColor,design.Muted));Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(p.HardwareNameColor,design.Text));
  CornerRadius=new CornerRadius(8);Padding=new Thickness(9,7,9,7);Margin=new Thickness(3,0,3,5);BorderThickness=new Thickness(1);BorderBrush=Ui.Brush(design.Edge);var background=Ui.Brush(design.Panel);background.Opacity=p.Opacity;Background=background;
  var body=new ExpandedReadingsPanel(p,2);Child=body;
  var header=new StackPanel{Orientation=Orientation.Horizontal};header.Children.Add(Ui.HardwareIcon(device.Category,13*scale*Preferences.TextFactor(p.TitleTextScale)));var title=Ui.SidebarText(device.Category,11*scale*Preferences.TextFactor(p.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);body.Children.Add(new Viewbox{Child=header,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left});
  var name=Ui.HardwareTitle(device.Name,10*scale*Preferences.TextFactor(p.HardwareTextScale),p.HardwareNames.ContainsKey(device.Id));name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.ToolTip=name.Text;name.Margin=new Thickness(0,2,0,5);body.Children.Add(name);
  var readings=new ExpandedReadingsPanel(p);body.Children.Add(readings);var primary=CardLayouts.MainMetric(device);
  foreach(var metric in device.Metrics)
  {
   var label=Ui.SidebarText(metric.Label,10*scale*Preferences.TextFactor(p.LabelTextScale),"Muted");label.ToolTip=Ui.SidebarFullLabel(metric.Label);
   var value=Ui.RoleText(SidebarTextRole.Readings,"—",11*scale*Preferences.TextFactor(p.ValueTextScale));value.FontFamily=Ui.Font(p,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(p,metric.Id==primary?.Id?DesignCatalog.CategoryColor(design,device.Category,p):design.Text);value.FontWeight=FontWeights.SemiBold;value.TextTrimming=TextTrimming.None;values[metric.Id]=value;
   if(metric.Unit=="IP")readings.Children.Add(new AddressReadoutPanel(label,value){Margin=new Thickness(0,.5,0,.5)});
   else readings.Children.Add(new RadialHardwareCard.ReadoutPanel(label,value,11*scale*Preferences.TextFactor(p.ValueTextScale)){Margin=new Thickness(0,.5,0,.5)});
  }
  Update(device,p);
 }
 public void Update(Device device,Preferences p)
 {
  foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var text)){text.Text=metric.Display(p.Fahrenheit);text.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+text.Text+(metric.Source==null?"":"\n"+metric.Source);}
  BorderBrush=Ui.Brush(device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=p.TempLimit)||(m.Label=="Auslastung"&&m.Value>=p.LoadLimit)))?"#F47777":design.Edge);
 }
}
