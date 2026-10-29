using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace Aurora;

// Arrange-only expansion keeps natural measurement and fit-to-view stable.
public sealed class ExpandedReadingsPanel : StackPanel
{
 public bool Expanded { get; }
 public int FixedChildren { get; }
 public ExpandedReadingsPanel(Preferences prefs,int fixedChildren=0)
 { Expanded=prefs.Columns>1;FixedChildren=fixedChildren; }
 protected override Size ArrangeOverride(Size size)
 {
  if(!Expanded)return base.ArrangeOverride(size);
  var children=InternalChildren.Cast<UIElement>().Where(c=>c.Visibility!=Visibility.Collapsed).ToArray();
  double natural=children.Sum(c=>c.DesiredSize.Height),extra=Math.Max(0,size.Height-natural);
  if(extra<.5)return base.ArrangeOverride(size);
  int fixedCount=InternalChildren.Cast<UIElement>().Take(FixedChildren).Count(c=>c.Visibility!=Visibility.Collapsed);
  // A nested readings grid gets the remaining height; its rows then spread inside it.
  int container=-1;
  for(int i=fixedCount;i<children.Length;i++)if(children[i] is ExpandedReadingsPanel or UniformGrid || children[i] is Grid grid && (children.Length-fixedCount==1 || grid.RowDefinitions.Any(row=>row.Height.IsStar)))container=i;
  double top=0;
  if(container>=0){for(int i=0;i<children.Length;i++){double height=children[i].DesiredSize.Height+(i==container?extra:0);children[i].Arrange(new Rect(0,top,size.Width,height));top+=height;}return size;}
  if(children.Length>fixedCount && children.Skip(fixedCount).All(child=>child is Border {Background:not null,Height:var h} && double.IsNaN(h))){for(int i=0;i<children.Length;i++){double height=children[i].DesiredSize.Height+(i>=fixedCount?extra/(children.Length-fixedCount):0);children[i].Arrange(new Rect(0,top,size.Width,height));top+=height;}return size;}
  bool IsTrack(int index)=>index<children.Length&&children[index] is Border{Child:Border,Height:>0} track&&double.IsFinite(track.Height);
  int groups=0;for(int i=fixedCount;i<children.Length;i++){groups++;if(IsTrack(i+1))i++;}
  double gap=groups>0?extra/(groups+1):0;
  for(int i=0;i<children.Length;i++){
   if(i==fixedCount)top+=gap;
   double height=children[i].DesiredSize.Height;children[i].Arrange(new Rect(0,top,size.Width,height));top+=height;
   if(i>=fixedCount&&!IsTrack(i+1))top+=gap;
  }
  return size;
 }
}
