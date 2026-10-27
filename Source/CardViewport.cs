using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Aurora;
public sealed class CardViewport:ScrollViewer
{
 public Grid Cards {get;}=new();
 public WidthPreservingFit Sizer {get;}=new(){VerticalAlignment=VerticalAlignment.Top};
 private bool fit;
 private (bool Fit,bool Fill,double Width,double Height,double MinimumWidth)? configured;
 public CardViewport(){HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled;HorizontalContentAlignment=HorizontalAlignment.Stretch;Sizer.Child=Cards;Content=Sizer;PreviewMouseWheel+=(_,e)=>{if(!fit){ScrollToVerticalOffset(VerticalOffset-e.Delta);e.Handled=true;}};MouseEnter+=(_,_)=>ShowScrollBar(true);MouseLeave+=(_,_)=>ShowScrollBar(false);Loaded+=(_,_)=>ShowScrollBar(IsMouseOver);}
 public override void OnApplyTemplate(){base.OnApplyTemplate();ShowScrollBar(IsMouseOver);}
 private void ShowScrollBar(bool visible)
 {
  if(GetTemplateChild("PART_VerticalScrollBar") is System.Windows.Controls.Primitives.ScrollBar bar)bar.Opacity=visible?1:0;
 }
 public void Configure(bool fitToView,double width,double height,bool fillHeight=false,double minimumContentWidth=0)
 {
  width=Math.Round(Math.Max(1,width),2);height=Math.Round(Math.Max(100,height),2);
  var next=(fitToView,fillHeight,width,height,minimumContentWidth);if(configured==next)return;configured=next;
  fit=fitToView;Sizer.Fit=fit;Sizer.FillHeight=fillHeight;Sizer.MinimumContentWidth=minimumContentWidth;
  VerticalScrollBarVisibility=fit?ScrollBarVisibility.Disabled:ScrollBarVisibility.Auto;
  Cards.Width=double.NaN;
  Sizer.Width=Math.Max(1,width-(fit?0:SystemParameters.VerticalScrollBarWidth));
  Sizer.Height=fit?Math.Max(100,height):double.NaN;
  Sizer.FitHeight=Math.Max(100,height);Sizer.InvalidateMeasure();
 }
}
public sealed class WidthPreservingFit:Decorator
{
 public bool Fit {get;set;}
 public double FitHeight {get;set;}=1000;
 public bool FillHeight {get;set;}
 public double MinimumContentWidth {get;set;}
 private double scale=1,logicalWidth,naturalHeight;
 protected override Size MeasureOverride(Size constraint)
 {
  if(Child==null)return new Size();
  double width=double.IsFinite(constraint.Width)?constraint.Width:double.IsFinite(Width)?Width:300;
  width=Math.Max(1,width);scale=MinimumContentWidth>width?width/MinimumContentWidth:1;logicalWidth=width/scale;
  Child.Measure(new Size(logicalWidth,double.PositiveInfinity));naturalHeight=Child.DesiredSize.Height;
  if(Fit&&naturalHeight*scale>FitHeight)
  {
   // Search a fitting scale; wrapping makes a fixed-point iteration oscillate.
   double low=0,high=scale;
   for(int pass=0;pass<16;pass++){double candidate=(low+high)/2;Child.Measure(new Size(width/candidate,double.PositiveInfinity));if(Child.DesiredSize.Height*candidate<=FitHeight)low=candidate;else high=candidate;}
   scale=Math.Max(.001,low);logicalWidth=width/scale;
   Child.Measure(new Size(logicalWidth,double.PositiveInfinity));naturalHeight=Child.DesiredSize.Height;
  }
  double desiredHeight=Fit?Math.Min(FitHeight,naturalHeight*scale):naturalHeight*scale;
  return new Size(width,FillHeight?Math.Max(FitHeight,desiredHeight):desiredHeight);
 }
 protected override Size ArrangeOverride(Size size)
 {
  if(Child!=null)
  {
   double width=Math.Max(1,size.Width);
   double actualScale=scale;
   Child.Arrange(new Rect(0,0,width/Math.Max(.01,actualScale),FillHeight?Math.Max(naturalHeight,FitHeight/Math.Max(.01,actualScale)):naturalHeight));
   Child.RenderTransform=actualScale==1?Transform.Identity:new ScaleTransform(actualScale,actualScale);
  }
  return size;
 }
}

// Measure natural rows first. Extra height grows card areas, never the gaps.
public sealed class HardwareCardGrid:Panel
{
 public int Columns{get;set;}=2;
 private readonly System.Collections.Generic.List<double> rows=new();
 protected override Size MeasureOverride(Size available)
 {
  rows.Clear();double width=double.IsFinite(available.Width)?Math.Max(1,available.Width):560;int columns=Math.Clamp(Columns,1,4);
  for(int start=0;start<InternalChildren.Count;start+=columns){int count=Math.Min(columns,InternalChildren.Count-start);double height=0;for(int i=0;i<count;i++){var child=InternalChildren[start+i];child.Measure(new Size(width/count,double.PositiveInfinity));height=Math.Max(height,child.DesiredSize.Height);}rows.Add(height);}
  double total=0;foreach(double row in rows)total+=row;return new Size(width,total);
 }
 protected override Size ArrangeOverride(Size finalSize)
 {
  double total=0;foreach(double row in rows)total+=row;double extra=rows.Count>0?Math.Max(0,finalSize.Height-total)/rows.Count:0;double top=0;int columns=Math.Clamp(Columns,1,4),rowIndex=0;
  for(int start=0;start<InternalChildren.Count;start+=columns){int count=Math.Min(columns,InternalChildren.Count-start);double height=(rowIndex<rows.Count?rows[rowIndex]:0)+extra;for(int i=0;i<count;i++)InternalChildren[start+i].Arrange(new Rect(i*finalSize.Width/count,top,finalSize.Width/count,height));top+=height;rowIndex++;}
  return finalSize;
 }
}
