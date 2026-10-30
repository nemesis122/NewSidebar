using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora;

static class CpuClockPrecisionQA
{
    public static void Verify(Action<bool,string> check)
    {
        foreach(var layout in CardLayouts.All)
        {
            var prefs=new Preferences{Layout=layout.Key};
            var device=new Device("clock-test","CPU","CPU clock fixture",new(){new Metric("cpu/clocks-average","Takt Ø",4111,"MHz",6000){IsCpuClock=true},new("cpu/load","Auslastung",7,"%")});
            var card=CardLayouts.Create(device,prefs,new MetricHistory());
            var host=new Border{Width=300,Child=card.View};
            host.Measure(new Size(300,double.PositiveInfinity));host.Arrange(new Rect(0,0,300,host.DesiredSize.Height));host.UpdateLayout();card.Update(device,prefs);
            bool Has(string value)=>Texts(card.View).Any(t=>t.Text==value+" GHz")||Texts(card.View).Any(t=>t.Text==value)&&Texts(card.View).Any(t=>t.Text=="GHz");
            check(Has("4.11"),"Two-decimal CPU clock renders in the original design: "+layout.Key);
            var changed=device with{Metrics=device.Metrics.Select(m=>m.IsCpuClock?m with{Value=4234}:m).ToList()};card.Update(changed,prefs);
            check(Has("4.23"),"CPU clock changes remain visible without extra rows: "+layout.Key);
            card.Update(device with{Metrics=device.Metrics.Select(m=>m.IsCpuClock?m with{Value=800}:m).ToList()},prefs);
            check(Has("0.80"),"Sub-GHz CPU clocks retain precision in the design: "+layout.Key);
        }
    }
    static IEnumerable<TextBlock> Texts(DependencyObject root)
    {
        var seen=new HashSet<DependencyObject>();
        IEnumerable<TextBlock> Visit(DependencyObject node){if(!seen.Add(node))yield break;if(node is TextBlock text)yield return text;foreach(var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>())foreach(var value in Visit(child))yield return value;if(node is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)foreach(var value in Visit(VisualTreeHelper.GetChild(node,i)))yield return value;}
        return Visit(root);
    }
}
