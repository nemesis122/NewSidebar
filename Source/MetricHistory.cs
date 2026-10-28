using System;
using System.Collections.Generic;
using System.Linq;
namespace Aurora;

public readonly record struct HistorySample(DateTime Time,double? Value);
// Memory-only, bounded history. Settings rebuilds cannot add duplicate snapshots.
public sealed class MetricHistory
{
 public const int MaximumSeries=512,MaximumSamples=256;
 public static readonly TimeSpan Window=TimeSpan.FromSeconds(60);
 private readonly Dictionary<(string Device,string Metric),List<HistorySample>> series=new();
 public DateTime LatestTime {get;private set;}
 public int SeriesCount=>series.Count;
 public void Record(IEnumerable<Device> devices,DateTime time)
 {
  if(time<LatestTime){if(LatestTime-time<TimeSpan.FromSeconds(5))return;series.Clear();}LatestTime=time;
  foreach(var device in devices)foreach(var metric in device.Metrics.Where(m=>m.Unit!="IP"))
  {
   var key=(device.Id,metric.Id);
   if(!series.TryGetValue(key,out var samples))
   {
    if(series.Count>=MaximumSeries){var oldest=series.OrderBy(p=>p.Value[^1].Time).First().Key;series.Remove(oldest);}
    series[key]=samples=new();
   }
   if(samples.Count>0&&time<=samples[^1].Time)continue;
   samples.Add(new(time,metric.Value is double value&&double.IsFinite(value)?value:null));
   samples.RemoveAll(p=>p.Time<time-Window);
   if(samples.Count>MaximumSamples)samples.RemoveRange(0,samples.Count-MaximumSamples);
  }
  foreach(var key in series.Where(p=>p.Value[^1].Time<time-Window).Select(p=>p.Key).ToArray())series.Remove(key);
 }
 public IReadOnlyList<HistorySample> Read(string device,string metric)=>series.TryGetValue((device,metric),out var samples)?samples.Where(p=>p.Time>=LatestTime-Window).ToArray():Array.Empty<HistorySample>();
}

