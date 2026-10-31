using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Aurora;

public enum SidebarActionIcon { Settings, Minimize }

public static class SidebarActionButton
{
    private static readonly Geometry SettingsGeometry=CreateGear();
    private static readonly Geometry MinimizeGeometry=Freeze(Geometry.Parse("M 4,13 L 4,19 L 20,19 L 20,13 M 8,10 L 12,14 L 16,10 M 12,4 L 12,14"));

    public static Button Create(SidebarActionIcon kind,Action action)
    {
        bool settings=kind==SidebarActionIcon.Settings;
        var icon=new Path
        {
            Data=settings?SettingsGeometry:MinimizeGeometry,Width=17,Height=17,
            Stretch=Stretch.Uniform,StrokeThickness=1.5,StrokeStartLineCap=PenLineCap.Round,
            StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round,
            HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,
            IsHitTestVisible=false
        };
        icon.SetResourceReference(Shape.StrokeProperty,"Text");
        System.Windows.Automation.AutomationProperties.SetAutomationId(icon,"SidebarActionIcon:"+kind);
        var button=new Button
        {
            Content=icon,Width=32,Height=32,Padding=new Thickness(6),
            Cursor=Cursors.Hand,HorizontalContentAlignment=HorizontalAlignment.Center,
            VerticalContentAlignment=VerticalAlignment.Center,UseLayoutRounding=true,
            SnapsToDevicePixels=true,ToolTip=settings?"Settings":"Minimize to tray"
        };
        System.Windows.Automation.AutomationProperties.SetAutomationId(button,settings?"OpenSettings":"HideToTray");
        System.Windows.Automation.AutomationProperties.SetName(button,settings?"Open settings":"Minimize sidebar to notification area");
        ToolTipService.SetInitialShowDelay(button,350);
        button.SetResourceReference(Control.BackgroundProperty,"Chrome");
        button.SetResourceReference(Control.BorderBrushProperty,"Edge");
        button.SetResourceReference(Control.ForegroundProperty,"Text");
        var border=new FrameworkElementFactory(typeof(Border),"Surface");
        border.SetValue(Border.CornerRadiusProperty,new CornerRadius(9));
        border.SetValue(Border.BorderThicknessProperty,new Thickness(1));
        border.SetValue(Border.BackgroundProperty,new TemplateBindingExtension(Control.BackgroundProperty));
        border.SetValue(Border.BorderBrushProperty,new TemplateBindingExtension(Control.BorderBrushProperty));
        border.SetValue(Border.PaddingProperty,new TemplateBindingExtension(Control.PaddingProperty));
        var presenter=new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty,HorizontalAlignment.Center);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty,VerticalAlignment.Center);
        presenter.SetValue(ContentPresenter.ContentProperty,new TemplateBindingExtension(ContentControl.ContentProperty));
        border.AppendChild(presenter);
        var template=new ControlTemplate(typeof(Button)){VisualTree=border};
        var hover=new Trigger{Property=UIElement.IsMouseOverProperty,Value=true};
        hover.Setters.Add(new Setter(Border.BorderBrushProperty,new DynamicResourceExtension("Accent"),"Surface"));
        hover.Setters.Add(new Setter(Border.BackgroundProperty,new DynamicResourceExtension("Edge"),"Surface"));template.Triggers.Add(hover);
        var focus=new Trigger{Property=UIElement.IsKeyboardFocusedProperty,Value=true};
        focus.Setters.Add(new Setter(Border.BorderBrushProperty,new DynamicResourceExtension("Accent"),"Surface"));template.Triggers.Add(focus);
        var pressed=new Trigger{Property=Button.IsPressedProperty,Value=true};
        pressed.Setters.Add(new Setter(UIElement.OpacityProperty,.7,"Surface"));template.Triggers.Add(pressed);
        var disabled=new Trigger{Property=UIElement.IsEnabledProperty,Value=false};
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty,.4,"Surface"));template.Triggers.Add(disabled);
        button.Template=template;button.Click+=(_,_)=>action();return button;
    }

    private static Geometry CreateGear()
    {
        var outline=new StreamGeometry();
        using(var context=outline.Open())
        {
            bool first=true;
            for(int tooth=0;tooth<8;tooth++)
                foreach(var point in new[]{(-17d,7.2),(-12d,9d),(12d,9d),(17d,7.2)})
                {
                    double angle=(tooth*45+point.Item1)*Math.PI/180;
                    var p=new Point(12+Math.Cos(angle)*point.Item2,12+Math.Sin(angle)*point.Item2);
                    if(first){context.BeginFigure(p,false,true);first=false;}else context.LineTo(p,true,false);
                }
        }
        var geometry=new GeometryGroup();geometry.Children.Add(outline);geometry.Children.Add(new EllipseGeometry(new Point(12,12),3.1,3.1));return Freeze(geometry);
    }
    private static Geometry Freeze(Geometry geometry){geometry.Freeze();return geometry;}
}
