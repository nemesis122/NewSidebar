using Aurora;
using System.Text.Json;

static class CombinedSensorsChecks
{
    public static void Run(Action<bool,string> check)
    {
        var snapshot=new Snapshot(new(){
            new("cpu","CPU","CPU A",new(){new("load","Auslastung",23,"%"),new("temp","Temperatur",52,"°C")}),
            new("gpu","GPU","GPU A",new(){new("load","Auslastung",76,"%"),new("temp","Temperatur",0,"°C")}),
            new("network/1","Network","Ethernet",new(){new("rx","Download",24,"MB/s",125)})
        },new(){new("fan","Board","Fan","Fan",1234,"RPM")},new(),DateTime.Now);
        var prefs=new Preferences{Monitor="existing-monitor",Columns=3,Layout="DashboardPro",Pins=new(){"fan"}};
        var separate=SensorCardComposition.Visible(snapshot,prefs);
        prefs.CombineSensorCards=true;var grouped=SensorCardComposition.Visible(snapshot,prefs);
        check(grouped.Select(d=>d.Id).SequenceEqual(separate.Select(d=>d.Id)),"Grouping preserves every original hardware ID and category order");
        check(grouped.Zip(separate).All(pair=>pair.First.Name==pair.Second.Name&&pair.First.Category==pair.Second.Category&&pair.First.Metrics.SequenceEqual(pair.Second.Metrics)),"Grouping does not merge devices, rename sensors, rewrite units or change values");
        check(grouped.Single(d=>d.Id=="gpu").Metrics.Single(m=>m.Id=="temp").Value==0,"A genuine zero remains available in its original device");
        check(grouped.Single(d=>d.Id=="custom").Metrics.Single().Id=="fan","Existing extra sensor pins retain their original hardware placement");
        prefs.HiddenDevices.Add("gpu");prefs.SetMetricVisible("cpu","load",false);
        grouped=SensorCardComposition.Visible(snapshot,prefs);
        check(grouped.Count==3&&grouped.All(d=>d.Id!="gpu")&&grouped.Single(d=>d.Id=="cpu").Metrics.Single().Id=="temp","Grouping honors the existing device and reading selection");
        prefs.HardwareNames["cpu"]="Gaming CPU";
        check(SensorCardComposition.Visible(snapshot,prefs).Single(d=>d.Id=="cpu").Name=="Gaming CPU","Original hardware names remain in their own section");
        prefs.NetworkAddresses["network/1"]=new(ExternalIP:"203.0.113.5");prefs.SetMetricVisible("network/1","network/1/external-ip",true);
        check(SensorCardComposition.Visible(snapshot,prefs).Single(d=>d.Category=="Network").Metrics.Single(m=>m.Unit=="IP").TextValue=="203.0.113.5","Saved selected IP values remain in the original network section");
        prefs.Save();var loaded=Preferences.Load();
        check(loaded.CombineSensorCards&&loaded.Monitor=="existing-monitor"&&loaded.Columns==3&&loaded.Layout=="DashboardPro","The shared frame persists without changing monitors, columns or designs");
        prefs.CombineSensorCards=false;
        check(SensorCardComposition.Visible(snapshot,prefs).Select(d=>d.Id).SequenceEqual(grouped.Select(d=>d.Id)),"Disabling the shared frame preserves the selected hardware sections");
        prefs.HiddenCategories=new(){"CPU","GPU","Network","Custom"};
        check(SensorCardComposition.Visible(snapshot,prefs).Count==0,"An empty hardware selection stays empty");
        check(!JsonSerializer.Deserialize<Preferences>("{}")!.CombineSensorCards,"Legacy settings keep separate cards by default");
    }
}
