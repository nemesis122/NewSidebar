using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora;

public sealed class ContemporaryHardwareCard : Border,IHardwareCard
{
    public FrameworkElement View=>this;
    private readonly SidebarDesign design;
    private readonly string layout,accent;
    private readonly Dictionary<string,TextBlock> values=new();
    private readonly TextBlock number,unit;
    private readonly string? primaryId;
    private readonly Instrument graphic;
    private readonly Brush normalEdge;

    public ContemporaryHardwareCard(Device device,Preferences prefs)
    {
        design=DesignCatalog.Resolve(prefs);layout=CardLayouts.BaseLayout(prefs.Layout);
        accent=DesignCatalog.CategoryColor(design,device.Category,prefs);
        System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(prefs,design.Font));
        double textScale=prefs.FontScale*CardLayouts.ReadingMultiplier(prefs.Layout)*CardLayouts.SpaceMultiplier(prefs);
        CornerRadius=new(layout switch{"Precision"=>3,"Bento"=>10,"OrbitStudio"=>22,_=>17});
        Padding=new Thickness(12,11,12,12);Margin=new Thickness(3,0,3,design.Gap);BorderThickness=new Thickness(1);
        SetResourceReference(BackgroundProperty,"Panel");
        normalEdge=layout=="NeonEdge"?Ui.Brush(accent):Ui.Brush(design.Edge);BorderBrush=normalEdge;
        if(layout=="Glass")
        {
            var top=(Color)ColorConverter.ConvertFromString(design.Panel);
            var bottom=(Color)ColorConverter.ConvertFromString(design.Back);
            Background=new LinearGradientBrush(Color.FromArgb((byte)(180*prefs.Opacity),top.R,top.G,top.B),Color.FromArgb((byte)(70*prefs.Opacity),bottom.R,bottom.G,bottom.B),new Point(0,0),new Point(1,1));
        }
        var body=new ExpandedReadingsPanel(prefs,2);Child=body;
        var header=new StackPanel{Orientation=Orientation.Horizontal};
        header.Children.Add(Ui.HardwareIcon(device.Category,14*textScale*Preferences.TextFactor(prefs.TitleTextScale)));
        var title=Ui.SidebarText(device.Category,12*textScale*Preferences.TextFactor(prefs.TitleTextScale));title.FontWeight=FontWeights.SemiBold;header.Children.Add(title);
        body.Children.Add(new Viewbox{Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=HorizontalAlignment.Left,Child=header});
        var name=Ui.HardwareTitle(device.Name,11*textScale*Preferences.TextFactor(prefs.HardwareTextScale),prefs.HardwareNames.ContainsKey(device.Id));
        name.TextWrapping=TextWrapping.Wrap;name.TextTrimming=TextTrimming.None;name.Margin=new Thickness(0,5,0,12);body.Children.Add(name);
        var content=new Grid();content.ColumnDefinitions.Add(new(){Width=new GridLength(112)});content.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});body.Children.Add(content);
        var instrument=new Grid{Width=102,Height=118,VerticalAlignment=VerticalAlignment.Top,HorizontalAlignment=HorizontalAlignment.Left};
        graphic=new Instrument(layout,accent,design.Track,DesignCatalog.GraphSize(prefs)){Width=102,Height=118};instrument.Children.Add(graphic);
        number=Ui.RoleText(SidebarTextRole.InstrumentNumbers,"—",24*prefs.FontScale*Preferences.TextFactor(prefs.InstrumentTextScale));number.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");number.Foreground=Ui.ValueBrush(prefs,accent);number.FontWeight=FontWeights.SemiBold;number.HorizontalAlignment=HorizontalAlignment.Center;number.TextTrimming=TextTrimming.None;
        System.Windows.Automation.AutomationProperties.SetAutomationId(number,"PrimaryMetric");
        unit=Ui.RoleText(SidebarTextRole.InstrumentUnits,"",9*prefs.FontScale*Preferences.TextFactor(prefs.UnitTextScale),"Muted");unit.HorizontalAlignment=HorizontalAlignment.Center;
        var digits=new StackPanel{VerticalAlignment=VerticalAlignment.Center};digits.Children.Add(number);digits.Children.Add(unit);
        var fitted=new Viewbox{Width=layout=="Precision"?66:82,Height=58,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,HorizontalAlignment=layout=="Precision"?HorizontalAlignment.Left:HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(layout=="Precision"?3:0,0,0,layout=="Fluent"?12:0),Child=digits};instrument.Children.Add(fitted);content.Children.Add(instrument);
        var readings=new ExpandedReadingsPanel(prefs){VerticalAlignment=CardLayouts.SpaceMultiplier(prefs)>1?VerticalAlignment.Stretch:VerticalAlignment.Top};Grid.SetColumn(readings,1);content.Children.Add(readings);
        foreach(var metric in device.Metrics)
        {
            var label=Ui.SidebarText(metric.Label,10*textScale*Preferences.TextFactor(prefs.LabelTextScale),"Muted");
            var value=Ui.RoleText(SidebarTextRole.Readings,"—",11*textScale*Preferences.TextFactor(prefs.ValueTextScale));value.FontFamily=Ui.Font(prefs,"Cascadia Mono, Consolas");value.Foreground=Ui.ValueBrush(prefs,design.Text);value.TextTrimming=TextTrimming.None;
            if(metric.Unit=="IP")value.TextWrapping=TextWrapping.Wrap;
            values[metric.Id]=value;
            var row=new RadialHardwareCard.ReadoutPanel(label,value,11*textScale*Preferences.TextFactor(prefs.ValueTextScale)){Margin=new Thickness(0,2,0,3)};
            if(layout=="Bento")
            {
                var tileBrush=Ui.Brush(design.Track);tileBrush.Opacity=.33*prefs.Opacity;
                bool featured=metric.Id==CardLayouts.MainMetric(device)?.Id;
                readings.Children.Add(new Border{CornerRadius=new CornerRadius(7),Background=tileBrush,BorderBrush=featured?Ui.Brush(accent):null,BorderThickness=new Thickness(featured?1:0),Padding=new Thickness(7,featured?10:6,7,featured?10:6),Margin=new Thickness(0,0,0,4),Child=row});
            }
            else
            {
                if(layout=="Precision")row.Margin=new Thickness(0,3,0,5);
                readings.Children.Add(row);
            }
        }
        primaryId=CardLayouts.MainMetric(device)?.Id;
        if(primaryId==null){instrument.Visibility=Visibility.Collapsed;content.ColumnDefinitions[0].Width=new GridLength(0);}
        CardLayouts.PositionInstrument(content,instrument,readings,prefs);
    }
    public void Update(Device device,Preferences prefs)
    {
        foreach(var metric in device.Metrics)if(values.TryGetValue(metric.Id,out var value))
        {value.Text=metric.Display(prefs.Fahrenheit);value.ToolTip=Ui.SidebarFullLabel(metric.Label)+" · "+value.Text+(metric.Source==null?"":"\n"+metric.Source);}
        var primary=device.Metrics.FirstOrDefault(m=>m.Id==primaryId);
        string text=primary?.Display(prefs.Fahrenheit)??"—";int split=text.LastIndexOf(' ');
        number.Text=split<0?text:text[..split];unit.Text=split<0?primary?.Unit??"":text[(split+1)..];number.ToolTip=primary==null?"Unavailable":Ui.SidebarFullLabel(primary.Label)+" · "+text;
        graphic.Ratio=primary?.Value is double raw&&double.IsFinite(raw)?Math.Clamp(raw/Math.Max(1,primary.Maximum),0,1):0;
        bool alert=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)));
        BorderBrush=alert?Ui.Brush("#F47777"):normalEdge;
    }
    private sealed class Instrument:FrameworkElement
    {
        private readonly string style;private readonly Brush ink,track;private readonly double thickness;private double ratio;
        public double Ratio{get=>ratio;set{if(Math.Abs(value-ratio)<.00001)return;ratio=value;InvalidateVisual();}}
        public Instrument(string style,string color,string background,double thickness){this.style=style;ink=Ui.Brush(color);track=Ui.Brush(background);this.thickness=thickness;}
        private static Point At(double angle,double radius,double cx=51,double cy=57)=>new(cx+Math.Cos(angle)*radius,cy+Math.Sin(angle)*radius);
        private void Arc(DrawingContext dc,double start,double sweep,double radius,Brush brush,double width)
        {
            if(sweep<=.001)return;
            var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(At(start,radius),false,false);c.ArcTo(At(start+sweep,radius),new Size(radius,radius),0,sweep>Math.PI,SweepDirection.Clockwise,true,false);}
            dc.DrawGeometry(null,new Pen(brush,width){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},path);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);double line=3*thickness;
            switch(style)
            {
                case "Fluent":
                    dc.DrawRoundedRectangle(null,new Pen(track,1),new Rect(2,12,98,78),16,16);
                    dc.DrawRoundedRectangle(track,null,new Rect(9,82,84,line),2,2);dc.DrawRoundedRectangle(ink,null,new Rect(9,82,84*ratio,line),2,2);
                    for(int i=0;i<5;i++)dc.DrawEllipse(i/5d<ratio?ink:track,null,new Point(31+i*10,103),2,2);
                    break;
                case "Glass":
                    dc.DrawRoundedRectangle(null,new Pen(track,1),new Rect(2,9,98,97),23,23);
                    Arc(dc,-Math.PI/2,Math.PI*1.98,40,track,line);Arc(dc,-Math.PI/2,Math.PI*1.98*ratio,40,ink,line);
                    break;
                case "Bento":
                    dc.DrawRoundedRectangle(null,new Pen(track,1),new Rect(2,15,98,76),10,10);
                    for(int i=0;i<4;i++)dc.DrawRoundedRectangle(i/4d<ratio?ink:track,null,new Rect(3+i*25,98,21,7*thickness),3,3);break;
                case "Precision":
                    dc.DrawLine(new Pen(track,2),new Point(84,10),new Point(84,106));
                    for(int i=0;i<=10;i++)dc.DrawLine(new Pen(track,1),new Point(i%5==0?70:77,10+i*9.6),new Point(88,10+i*9.6));
                    double y=106-96*ratio;dc.DrawLine(new Pen(ink,line),new Point(74,y),new Point(96,y));
                    dc.DrawEllipse(ink,null,new Point(84,y),3,3);break;
                case "NeonEdge":
                    var diamond=new StreamGeometry();using(var c=diamond.Open()){c.BeginFigure(new Point(51,5),false,true);c.LineTo(new Point(97,57),true,false);c.LineTo(new Point(51,109),true,false);c.LineTo(new Point(5,57),true,false);}
                    dc.DrawGeometry(null,new Pen(track,2),diamond);Arc(dc,-Math.PI/2,Math.PI*1.98*ratio,35,ink,line);
                    dc.DrawLine(new Pen(ink,1),new Point(25,112),new Point(77,112));break;
                default:
                    dc.DrawEllipse(null,new Pen(track,1),new Point(51,57),47,25);
                    dc.PushTransform(new RotateTransform(-35,51,57));dc.DrawEllipse(null,new Pen(track,1),new Point(51,57),47,25);dc.Pop();
                    Arc(dc,-Math.PI/2,Math.PI*1.98,36,track,line);Arc(dc,-Math.PI/2,Math.PI*1.98*ratio,36,ink,line);
                    dc.DrawEllipse(ink,null,At(-Math.PI/2+ratio*Math.PI*2,47),3,3);break;
            }
        }
    }
}
