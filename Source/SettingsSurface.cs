using System;
using System.Windows;
using System.Windows.Controls;
namespace Aurora;
// Only the settings window uses these native styles. Sidebar designs keep their resources.
public static class SettingsSurface
{
 private static ResourceDictionary? resources;
 public static ResourceDictionary Resources=>resources??=Create();
 private static ResourceDictionary Create(){var r=new ResourceDictionary();r.MergedDictionaries.Add(new Wpf.Ui.Markup.ThemesDictionary{Theme=Wpf.Ui.Appearance.ApplicationTheme.Dark});r.MergedDictionaries.Add(new Wpf.Ui.Markup.ControlsDictionary());return r;}
}
