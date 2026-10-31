using System.Reflection;
namespace Aurora;
// CLR versions normalize leading zeros; display the exact published product version.
public static class ReleaseIdentity
{
 public static string Version=>typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]??typeof(App).Assembly.GetName().Version?.ToString()??"Unknown";
}
