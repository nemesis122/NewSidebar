using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora;

public static class SensorCardComposition
{
    public const string DeviceId = "hw-sensors";
    public const string Title = "HW Sensors";

    // Reuse the existing selection, pins and category order before combining anything.
    public static List<Device> Visible(Snapshot snapshot, Preferences prefs)
    {
        var devices = PinPlacement.Compose(snapshot,prefs)
            .Where(d => !prefs.HiddenCategories.Contains(d.Category) && !prefs.HiddenDevices.Contains(d.Id))
            .Select(d => d with { Name=prefs.HardwareName(d.Id,d.Name), Metrics=d.Metrics.Where(m=>prefs.IsMetricVisible(d.Id,m.Id)).ToList() })
            .Where(d => d.Metrics.Count>0)
            .OrderBy(d => { int index=prefs.Order.IndexOf(d.Category);return index<0?99:index; }).ToList();
        return devices;
    }
}
