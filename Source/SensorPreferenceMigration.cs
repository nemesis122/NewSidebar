using System.Linq;
namespace Aurora;

public static class SensorPreferenceMigration
{
    // Seed matched devices once. Later explicit changes in either source remain independent.
    public static bool Apply(Snapshot snapshot,Preferences prefs)
    {
        bool changed=false;
        if(snapshot.PreferenceSeeds==null)return false;
        foreach(var seed in snapshot.PreferenceSeeds)
        {
            if(seed.DeviceId==seed.ReferenceDeviceId||prefs.SeededSourceDevices.Contains(seed.DeviceId))continue;
            prefs.SeededSourceDevices.Add(seed.DeviceId);changed=true;
            if(prefs.HiddenDevices.Contains(seed.ReferenceDeviceId)&&!prefs.HiddenDevices.Contains(seed.DeviceId))prefs.HiddenDevices.Add(seed.DeviceId);
            if(!prefs.HardwareNames.ContainsKey(seed.DeviceId)&&prefs.HardwareNames.TryGetValue(seed.ReferenceDeviceId,out var name))prefs.HardwareNames[seed.DeviceId]=name;
            foreach(var metric in seed.MetricIds)
            {
                string reference=seed.ReferenceDeviceId+"|"+metric.Value;
                string target=seed.DeviceId+"|"+metric.Key;
                if(prefs.HiddenMetrics.Contains(reference)&&!prefs.HiddenMetrics.Contains(target))prefs.HiddenMetrics.Add(target);
            }
        }
        return changed;
    }
}
