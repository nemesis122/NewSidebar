using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;

// Addresses use the same label/value edges as ordinary rows. A long address can
// wrap below its label, without shrinking its font or clipping IPv6 characters.
public sealed class AddressReadoutPanel:Panel
{
 private readonly TextBlock label,value;private bool inline;
 public AddressReadoutPanel(TextBlock label,TextBlock value)
 {
  this.label=label;this.value=value;value.TextWrapping=TextWrapping.Wrap;value.TextTrimming=TextTrimming.None;value.TextAlignment=TextAlignment.Right;
  Children.Add(label);Children.Add(value);
 }
 private static double TextWidth(TextBlock text)=>text.Visibility==Visibility.Collapsed?0:new FormattedText(text.Text,CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface(text.FontFamily,text.FontStyle,text.FontWeight,text.FontStretch),text.FontSize,Brushes.White,VisualTreeHelper.GetDpi(text).PixelsPerDip).WidthIncludingTrailingWhitespace;
 protected override Size MeasureOverride(Size available)
 {
  double width=double.IsFinite(available.Width)?Math.Max(0,available.Width):400;
  double gap=label.Visibility==Visibility.Collapsed?0:7;
  inline=TextWidth(label)+TextWidth(value)+gap<=width;
  label.TextWrapping=inline?TextWrapping.NoWrap:TextWrapping.Wrap;label.TextTrimming=TextTrimming.None;
  value.Measure(new Size(inline?Math.Max(0,width-TextWidth(label)-gap):width,double.PositiveInfinity));
  label.Measure(new Size(inline?Math.Max(0,width-value.DesiredSize.Width-gap):width,double.PositiveInfinity));
  return new Size(width,inline?Math.Max(label.DesiredSize.Height,value.DesiredSize.Height):label.DesiredSize.Height+value.DesiredSize.Height);
 }
 protected override Size ArrangeOverride(Size size)
 {
  double valueWidth=inline?Math.Min(size.Width,value.DesiredSize.Width):size.Width;
  double height=inline?Math.Max(label.DesiredSize.Height,value.DesiredSize.Height):label.DesiredSize.Height+value.DesiredSize.Height,top=Math.Max(0,(size.Height-height)/2);
  label.Arrange(new Rect(0,top,inline?Math.Max(0,size.Width-valueWidth-7):size.Width,label.DesiredSize.Height));
  value.Arrange(new Rect(size.Width-valueWidth,top+(inline?0:label.DesiredSize.Height),valueWidth,value.DesiredSize.Height));return size;
 }
}
