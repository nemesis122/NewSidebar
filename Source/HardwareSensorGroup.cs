using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora;

// Keep the production hardware cards and their data intact. Only their outer shells
// are replaced by one shared shell; instruments, fields and histories are untouched.
public sealed class HardwareSensorGroup : Border
{
    private readonly Dictionary<string,IHardwareCard> sections=new();
    private readonly Brush normalEdge;
    public IReadOnlyDictionary<string,IHardwareCard> Sections=>sections;

    public HardwareSensorGroup(IReadOnlyList<Device> devices,Preferences prefs,MetricHistory history)
    {
        var design=DesignCatalog.Resolve(prefs);
        var body=new StackPanel();
        var surface=new Grid();surface.Children.Add(body);Child=surface;
        var title=Ui.RoleText(SidebarTextRole.WidgetTitles,SensorCardComposition.Title,14*prefs.FontScale*Preferences.TextFactor(prefs.TitleTextScale));
        title.FontWeight=FontWeights.SemiBold;title.Margin=new Thickness(12,10,12,6);
        title.Visibility=prefs.ShowWidgetTitles?Visibility.Visible:Visibility.Collapsed;
        body.Children.Add(title);
        Panel layout=prefs.Columns>1?new HardwareCardGrid{Columns=prefs.Columns}:new StackPanel();
        body.Children.Add(layout);
        Margin=new Thickness(3,0,3,design.Gap);BorderThickness=new Thickness(1);CornerRadius=new CornerRadius(design.Radius);
        var background=Ui.Brush(design.Panel);background.Opacity=prefs.Opacity;Background=background;
        normalEdge=Ui.Brush(design.Edge);
        bool first=true;
        foreach(var device in devices)
        {
            var card=CardLayouts.Create(device,prefs,history);sections[device.Id]=card;
            if(card.View is not Border section)throw new InvalidOperationException("Hardware card has no outer surface.");
            Brush? sectionBackground=section.Background,sectionEdge=section.BorderBrush;
            // Fluent layouts have a nested native Card shell. Keep its original content
            // and padding rather than retaining a second rounded card inside the group.
            if(section.Child is Wpf.Ui.Controls.Card fluent)
            {
                sectionBackground=fluent.Background;sectionEdge=fluent.BorderBrush;
                var content=fluent.Content as UIElement;fluent.Content=null;section.Child=null;
                section.Padding=fluent.Padding;section.Child=content;
            }
            if(first)
            {
                if(sectionBackground!=null)Background=sectionBackground.CloneCurrentValue();
                if(sectionEdge!=null&&!WarningBrush(sectionEdge))normalEdge=sectionEdge.CloneCurrentValue();
                if(section.CornerRadius.TopLeft>0)CornerRadius=section.CornerRadius;
                System.Windows.Documents.TextElement.SetFontFamily(this,Ui.Font(prefs,design.Font));
                foreach(string key in new[]{"Text","Muted","HardwareName"})if(section.Resources.Contains(key))Resources[key]=section.Resources[key];
            }
            // Move HUD frame artwork to the common perimeter once, without altering
            // any of the instrument artwork inside the hardware section.
            if(section.Child is Grid canvas)
                foreach(var frame in canvas.Children.OfType<FrameworkElement>().Where(e=>e.GetType().Name is "FrameDecoration" or "PanelFrame").ToArray())
                {
                    canvas.Children.Remove(frame);if(first)surface.Children.Insert(0,frame);
                }
            section.Background=null;section.BorderThickness=new Thickness(0);section.CornerRadius=new CornerRadius(0);section.Margin=new Thickness(0);section.Effect=null;
            layout.Children.Add(section);first=false;
        }
        BorderBrush=normalEdge;
        System.Windows.Automation.AutomationProperties.SetAutomationId(this,"HardwareSensorGroup");
        Update(devices,prefs);
    }

    public void Update(IEnumerable<Device> devices,Preferences prefs)
    {
        bool warning=false;
        foreach(var device in devices)
        {
            if(sections.TryGetValue(device.Id,out var card))card.Update(device,prefs);
            warning|=device.Metrics.Any(m=>m.Value.HasValue&&((m.Unit=="°C"&&m.Value>=prefs.TempLimit)||(m.Label=="Auslastung"&&m.Value>=prefs.LoadLimit)));
        }
        BorderBrush=warning?Ui.Brush("#F47777"):normalEdge;
    }
    private static bool WarningBrush(Brush brush)=>brush is SolidColorBrush solid&&solid.Color==Color.FromRgb(244,119,119);
}
