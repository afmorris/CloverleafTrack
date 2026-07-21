using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CloverleafTrack.Web.Utilities;

public static class BuildMetadataHelper
{
    public static string GetInformationalVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attribute?.InformationalVersion ?? "unknown";
    }

    public static string GetShortCommitSha()
    {
        var version = GetInformationalVersion();

        // InformationalVersion often ends with '+commitSha' when SourceRevisionId is set.
        var plusIndex = version.IndexOf('+');
        if (plusIndex >= 0)
        {
            var sha = version.Substring(plusIndex + 1);
            if (!string.IsNullOrEmpty(sha))
                return sha[..Math.Min(sha.Length, 7)];
        }

        // Fallback: try to parse a plain SHA-ish suffix.
        if (!string.IsNullOrEmpty(version) && version != "unknown")
            return version[..Math.Min(version.Length, 7)];

        return "unknown";
    }
}
