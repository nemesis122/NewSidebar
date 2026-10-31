using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Aurora;

// Portable copies in different locations must never activate each other.
public static class StartupIdentity
{
    public static string Normalize(string path) => Path.GetFullPath(path).ToUpperInvariant();
    public static bool SameExecutable(string? first, string? second)
    {
        if(string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) return false;
        try { return Normalize(first) == Normalize(second); }
        catch(ArgumentException) { return false; }
        catch(NotSupportedException) { return false; }
        catch(PathTooLongException) { return false; }
    }
    public static string InstanceKey(string executable) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(executable))));
    public static bool MatchesTaskTarget(string? target,string? current) => SameExecutable(target,current) && File.Exists(target);
    public static string MutexName(string executable) => "Local\\NemeSIS-Instance-" + InstanceKey(executable);
    public static string ShowEventName(string executable) => "Local\\NemeSIS-Show-" + InstanceKey(executable);
}
