using System;
using System.Collections.Generic;
using System.Linq;
namespace Aurora;
public static class PinPlacement
{
    public static void SetPinned(Snapshot? snapshot,Preferences prefs,string id,bool added)
    {
        var previous=snapshot==null?null:Target(snapshot,prefs,id);prefs.SetPinned(id,added);
        if(added && previous!=null)prefs.SetMetricVisible(previous.Id,id,true);
        if(!added && snapshot!=null)
        {
            if(previous!=null)prefs.SetMetricVisible(previous.Id,id,false);
            foreach(var device in snapshot.Devices.Where(d=>d.Metrics.Any(m=>m.Id==id)))prefs.SetMetricVisible(device.Id,id,false);
        }
    }
    public static Device? Target(Snapshot snapshot,Preferences prefs,string sensorId)
    {
        if(prefs.PinTargets.TryGetValue(sensorId,out string? target)&&target.Length>0)return snapshot.Devices.FirstOrDefault(d=>d.Id==target);
        var owner=snapshot.Devices.Where(d=>sensorId.StartsWith(d.Id+"/",StringComparison.Ordinal)||d.Metrics.Any(m=>m.Id==sensorId)).OrderByDescending(d=>d.Id.Length).FirstOrDefault();
        if(owner!=null)return owner;
        var sensor=snapshot.Sensors.FirstOrDefault(s=>s.Id==sensorId);
        if(sensor?.HardwareKind=="Memory")return snapshot.Devices.FirstOrDefault(d=>d.Category=="RAM");
        return null;
    }
    public static List<Device> Compose(Snapshot snapshot,Preferences prefs)
    {
        var devices=snapshot.Devices.Select(d=>d with{Metrics=d.Metrics.ToList()}).ToList();var separate=new List<Metric>();
        foreach(var device in devices.Where(d=>d.Category=="Network")){
            prefs.NetworkAddresses.TryGetValue(device.Id,out var address);
            void Address(string suffix,string label,string? text,DateTimeOffset? updated,string route){if(!device.Metrics.Any(m=>m.Id==device.Id+suffix))device.Metrics.Add(new Metric(device.Id+suffix,label,null,"IP",Source:route+(updated.HasValue?" · "+updated.Value.ToLocalTime().ToString("g"):" · Not fetched yet"),TextValue:text));}
            Address("/internal-ip","Interne IP",address?.InternalIP,address?.InternalUpdated,"Saved adapter address · manual refresh");
            Address("/external-ip","Externe IP",address?.ExternalIP,address?.ExternalUpdated,"Saved internet IP · system default route · manual refresh via api64.ipify.org");
        }
        foreach(string id in prefs.Pins.Distinct())
        {
            var sensor=snapshot.Sensors.FirstOrDefault(s=>s.Id==id);var target=Target(snapshot,prefs,id);var destination=devices.FirstOrDefault(d=>d.Id==target?.Id);
            var metric=sensor==null?new Metric(id,"Nicht verfügbar · "+id,null,""):new Metric(sensor.Id,destination==null?sensor.Device+" · "+sensor.Name:sensor.Name,sensor.Value,sensor.Unit,Maximum(sensor.Unit),sensor.Device+" · "+sensor.Name);
            if(sensor?.Kind=="Clock"&&sensor.HardwareKind=="Cpu")metric=metric with{IsCpuClock=true};
            if(destination!=null){if(!destination.Metrics.Any(m=>m.Id==id))destination.Metrics.Add(metric);}else separate.Add(metric);
        }
        if(separate.Count>0)devices.Add(new("custom","Custom","Eigene Werte ohne Widget-Zuordnung",separate));
        return devices;
    }
    private static double Maximum(string unit)=>unit switch{"RPM"=>4000,"MHz"=>6000,"V"=>2,"W"=>450,_=>100};
}
