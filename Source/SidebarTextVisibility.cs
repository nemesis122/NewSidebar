using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;
public static class SidebarTextVisibility
{
 private sealed class State {public SidebarTextRole Role;public bool Configured,Mixed,Numbers=true,Units=true,Editing;public string Unit="";public Visibility Original;}
 private static readonly ConditionalWeakTable<TextBlock,State> States=new();
 private static State For(TextBlock text)=>States.GetValue(text,_=>new State());
 public static void SetRole(TextBlock text,SidebarTextRole role)=>For(text).Role=role;
 public static SidebarTextRole GetRole(TextBlock text)=>States.TryGetValue(text,out var state)?state.Role:SidebarTextRole.None;
 public static void Apply(IHardwareCard card,Device device,Preferences prefs)
 {
  bool mixed=card is HardwareCard||card is AlternateHardwareCard&&CardLayouts.BaseLayout(prefs.Layout)=="Dashboard";
  var visited=new HashSet<DependencyObject>();
  void Visit(DependencyObject node){if(!visited.Add(node))return;if(node is FrameworkElement icon&&System.Windows.Automation.AutomationProperties.GetAutomationId(icon)=="HardwareIcon"){icon.Visibility=prefs.ShowHardwareIcons?Visibility.Visible:Visibility.Collapsed;double scale=Preferences.TextFactor(prefs.HardwareIconScale);icon.LayoutTransform=new ScaleTransform(scale,scale);}if(node is TextBlock text){var state=For(text);string id=System.Windows.Automation.AutomationProperties.GetAutomationId(text);if(id=="HardwareName")state.Role=SidebarTextRole.HardwareNames;else if(id=="PrimaryMetric")state.Role=SidebarTextRole.InstrumentNumbers;else if(state.Role==SidebarTextRole.None){if(string.Equals(text.Text,Ui.SidebarLabel(device.Category),StringComparison.OrdinalIgnoreCase))state.Role=SidebarTextRole.WidgetTitles;else if(text.Text=="—"||text.Text.Length>0&&char.IsDigit(text.Text[0]))state.Role=SidebarTextRole.Readings;else state.Role=SidebarTextRole.Labels;}
   if(!state.Configured)state.Original=text.Visibility;state.Configured=true;state.Mixed=mixed&&id=="PrimaryMetric";state.Numbers=prefs.ShowInstrumentNumbers;state.Units=prefs.ShowInstrumentUnits;var main=CardLayouts.MainMetric(device);string display=main?.Display(prefs.Fahrenheit)??"";int split=display.LastIndexOf(' ');state.Unit=split>=0?display[(split+1)..]:main?.Unit=="°C"&&prefs.Fahrenheit?"°F":main?.Unit??"";
   bool visible=state.Mixed?state.Numbers||state.Units:prefs.IsTextVisible(state.Role);text.Visibility=visible?state.Original:Visibility.Collapsed;if(state.Mixed)FilterText(text,text.Text);
  }foreach(var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().ToArray())Visit(child);if(node is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)Visit(VisualTreeHelper.GetChild(node,i));}
  Visit(card.View);
 }
 internal static void FilterText(TextBlock text,string raw)
 {
  if(!States.TryGetValue(text,out var s)||!s.Configured||!s.Mixed||s.Editing||s.Numbers&&s.Units)return;
  int split=raw.LastIndexOf(' ');if(split>=0)s.Unit=raw[(split+1)..];string shown=s.Numbers?(split>=0?raw[..split]:raw):s.Units?s.Unit:"";if(shown==raw)return;s.Editing=true;try{text.SetCurrentValue(TextBlock.TextProperty,shown);}finally{s.Editing=false;}
 }
}
// A per-element callback also handles legacy instruments whose number and unit share one TextBlock.
// No global event subscriptions are retained when a widget is rebuilt or closed.
public sealed class SidebarTextBlock:TextBlock
{
 static SidebarTextBlock(){TextProperty.OverrideMetadata(typeof(SidebarTextBlock),new FrameworkPropertyMetadata("",(node,args)=>SidebarTextVisibility.FilterText((TextBlock)node,args.NewValue as string??"")));}
}
