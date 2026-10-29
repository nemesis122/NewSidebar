using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Aurora;

internal static class SourceSelectionQA
{
    public static void Verify(Action<bool,string> require)
    {
        var original=new[]{new Device("cpu","CPU","Demo CPU",new(){new("cpu-load","Auslastung",24,"%"),new("cpu-temp","Temperatur",52,"°C"),new("cpu-clock","Takt Ø",4000,"MHz")}),new Device("gpu","GPU","Demo GPU",new(){new("gpu-load","Auslastung",30,"%"),new("gpu-temp","Temperatur",48,"°C"),new("gpu-volt","Spannung",.73,"V")}),new Device("ram","RAM","32 GB",new(){new("ram-used","Belegt",12,"GB"),new("ram-free","Verfügbar",20,"GB")}),new Device("board","Mainboard","Demo board",new(){new("board-temp","Temperatur",40,"°C")}),new Device("controller","Controller","Demo controller",new(){new("controller-temp","Temperatur",41,"°C")})};
        string Other(string id)=>"ohm/pawn/"+id;
        var next=original.Select(d=>d with{Id=Other(d.Id),Metrics=d.Metrics.Select(m=>m with{Id=Other(m.Id)}).ToList()}).ToList();
        var seeds=original.Select(d=>new DevicePreferenceSeed(Other(d.Id),d.Id,d.Metrics.ToDictionary(m=>Other(m.Id),m=>m.Id))).ToList();
        foreach(var layout in new[]{"Original","FluentGlass","MinimalPro"})
        {
            var prefs=new Preferences{Layout=layout,FontScale=1.3,Fit=true,Columns=2,HiddenCategories=new(),HiddenDevices=new(){"board","controller"},HiddenMetrics=new(){"gpu|gpu-volt"}};
            double before=Scale(original,prefs);
            SensorPreferenceMigration.Apply(new(next,new(),new(),DateTime.Now,SensorSources.OpenPawn,seeds),prefs);
            require(next.Count(d=>!prefs.HiddenDevices.Contains(d.Id))==3,"Hidden bridge cards stay hidden before fitting: "+layout);
            double after=Scale(next,prefs);
            require(Math.Abs(before-after)<.0001,"Matching source selections keep the same whole-sidebar text scale: "+layout);
        }
    }
    private static double Scale(IEnumerable<Device> devices,Preferences prefs)
    {
        var view=new CardViewport();var panel=new HardwareCardGrid{Columns=2};view.Cards.Children.Add(panel);
        foreach(var device in devices.Where(d=>!prefs.HiddenDevices.Contains(d.Id)))
        {
            var filtered=device with{Metrics=device.Metrics.Where(m=>prefs.IsMetricVisible(device.Id,m.Id)).ToList()};
            var card=CardLayouts.Create(filtered,prefs);card.Update(filtered,prefs);panel.Children.Add(card.View);
        }
        view.Configure(true,560,300,true);view.Measure(new Size(560,300));view.Arrange(new Rect(0,0,560,300));view.UpdateLayout();
        return view.Sizer.Child.RenderTransform.Value.M11;
    }
}
