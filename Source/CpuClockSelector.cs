using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aurora;

public sealed record CpuClockSelection(double? Value,string Source);

public static class CpuClockSelector
{
    private static readonly Regex CoreName=new(@"^(?:(?:CPU\s+)?Core(?:\s*#\d+)?|[PE]-Core\s*#\d+)$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
    private static readonly string[] AverageNames={"Cores (Average)","CPU Cores (Average)"};

    // Only live CPU core clocks belong here. In particular, a bus/reference clock
    // must never become a plausible-looking CPU frequency when cores are unavailable.
    public static CpuClockSelection Select(IEnumerable<(string Name,double? Value)> clocks)
    {
        var valid=clocks.Where(s=>s.Value is double v&&double.IsFinite(v)&&v>0).ToArray();
        foreach(string name in AverageNames)
        {
            var average=valid.FirstOrDefault(s=>string.Equals(s.Name,name,StringComparison.OrdinalIgnoreCase));
            if(average.Value.HasValue)return new(average.Value,average.Name);
        }
        var cores=valid.Where(s=>CoreName.IsMatch(s.Name.Trim())).ToArray();
        return cores.Length>0
            ? new(cores.Average(s=>s.Value!.Value),"CPU core clock average · "+string.Join(", ",cores.Select(s=>s.Name)))
            : new(null,"CPU core clock unavailable: no valid core clock sensors. Bus/reference and effective clocks are not substituted.");
    }
}
