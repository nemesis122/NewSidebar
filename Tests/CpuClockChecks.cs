using Aurora;

static class CpuClockChecks
{
    public static void Run(Action<bool,string> check)
    {
        foreach(string name in new[]{"CPU Core #1","CPU Core #8","Core #1","P-Core #1","E-Core #4","CPU Core","Core","cpu core #2"," P-Core #3 "})
        {
            var clock=CpuClockSelector.Select(new[]{(name,(double?)4200)});
            check(clock.Value==4200&&clock.Source.Contains(name),"CPU clock accepts a real core sensor name: "+name);
        }
        var intel=CpuClockSelector.Select(new[]{("CPU Core #1",(double?)4100),("CPU Core #2",(double?)4300),("Bus Speed",(double?)100)});
        check(intel.Value==4200,"Intel CPU Core sensors are averaged without including the bus clock");
        var hybrid=CpuClockSelector.Select(new[]{("P-Core #1",(double?)4500),("E-Core #1",(double?)3500)});
        check(hybrid.Value==4000,"Intel P- and E-core sensors participate in the core average");
        foreach(string average in new[]{"Cores (Average)","CPU Cores (Average)","cores (average)"})
            check(CpuClockSelector.Select(new[]{(average,(double?)3800),("Core #1",(double?)4200)}).Value==3800,"A valid reported average is preferred: "+average);
        check(CpuClockSelector.Select(new[]{("Cores (Average)",(double?)null),("CPU Core #1",(double?)4300)}).Value==4300,"Unavailable aggregate readings fall back to actual cores");
        foreach(string name in new[]{"Bus Speed","Bus Clock","Reference Clock","Uncore","Memory","CPU Core #1 Effective","Core #1 Effective Clock","GPU Core","CPU Total"})
            check(CpuClockSelector.Select(new[]{(name,(double?)100)}).Value==null,"Unrelated or effective clocks are not mislabeled as CPU core frequency: "+name);
        foreach(double? invalid in new double?[]{null,0,-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity})
            check(CpuClockSelector.Select(new[]{("CPU Core #1",invalid),("CPU Core #2",(double?)4000)}).Value==4000,"Unavailable and invalid core samples do not corrupt the average: "+invalid);
        var missing=CpuClockSelector.Select(Array.Empty<(string,double?)>());
        check(missing.Value==null&&missing.Source.Contains("unavailable"),"No reported CPU clock stays unavailable with an explicit source explanation");
        check(new Metric("cpu/clock","Clock",4111,"MHz"){IsCpuClock=true}.Display(false)=="4.11 GHz","CPU frequency displays two decimals in GHz");
        check(new Metric("cpu/clock","Clock",800,"MHz"){IsCpuClock=true}.Display(false)=="0.80 GHz","Idle CPU frequencies retain sub-GHz precision");
        check(new Metric("cpu/clock","Clock",null,"MHz"){IsCpuClock=true}.Display(false)=="—","Missing CPU clocks remain unavailable rather than becoming zero");
        check(new Metric("gpu/clock","Clock",4111,"MHz").Display(false)=="4 GHz","The CPU precision adjustment does not change GPU or generic clock formatting");
        var pinSnapshot=new Snapshot(new(){new("cpu","CPU","Any CPU",new())},new(){new("cpu/core/0","Any CPU","CPU Core #1","Clock",4111,"MHz","Cpu")},new(),DateTime.Now);
        var pin=PinPlacement.Compose(pinSnapshot,new Preferences{Pins=new(){"cpu/core/0"}}).Single();
        check(pin.Metrics.Single().IsCpuClock&&pin.Metrics.Single().Display(false)=="4.11 GHz","Individually pinned CPU core clocks preserve CPU-only precision");
    }
}
