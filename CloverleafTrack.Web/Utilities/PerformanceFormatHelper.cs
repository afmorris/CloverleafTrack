using System.Text.RegularExpressions;

namespace CloverleafTrack.Web.Utilities;

/// <summary>
/// Helper class for parsing and formatting track & field performance values
/// </summary>
public static class PerformanceFormatHelper
{
    /// <summary>
    /// Parse time input to seconds. Accepts formats like:
    /// - "11.24" (seconds)
    /// - "11.24s" (seconds with suffix)
    /// - "1:23.45" (minutes:seconds)
    /// - "1m23.45s" (minutes/seconds with suffixes)
    /// </summary>
    public static double? ParseTime(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        input = input.Trim().ToLower().Replace("s", "").Replace("m", ":");
        
        // Try simple decimal first (e.g., "11.24")
        if (double.TryParse(input, out var seconds))
        {
            return seconds;
        }
        
        // Try M:SS.ss format (e.g., "1:23.45")
        var colonMatch = Regex.Match(input, @"^(\d+):(\d+\.?\d*)$");
        if (colonMatch.Success)
        {
            var minutes = int.Parse(colonMatch.Groups[1].Value);
            var secs = double.Parse(colonMatch.Groups[2].Value);
            return (minutes * 60) + secs;
        }
        
        return null;
    }
    
    /// <summary>
    /// Parse distance input to inches. Accepts formats like:
    /// - "19'4" or "19'4.5" (feet and inches)
    /// - "19-04" (feet-inches)
    /// - "234.5" (total inches)
    /// - "19 feet 4 inches"
    /// </summary>
    public static double? ParseDistance(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        input = input.Trim().ToLower();
        
        // Remove common words
        input = input.Replace("feet", "'").Replace("foot", "'")
                     .Replace("inches", "\"").Replace("inch", "\"")
                     .Replace(" ", "");
        
        // Try feet'inches" format (e.g., "19'4" or "19'4.5")
        var feetInchMatch = Regex.Match(input, @"^(\d+)'(\d+\.?\d*)");
        if (feetInchMatch.Success)
        {
            var feet = int.Parse(feetInchMatch.Groups[1].Value);
            var inches = double.Parse(feetInchMatch.Groups[2].Value);
            return (feet * 12) + inches;
        }
        
        // Try feet-inches format (e.g., "19-04")
        var dashMatch = Regex.Match(input, @"^(\d+)-(\d+\.?\d*)$");
        if (dashMatch.Success)
        {
            var feet = int.Parse(dashMatch.Groups[1].Value);
            var inches = double.Parse(dashMatch.Groups[2].Value);
            return (feet * 12) + inches;
        }
        
        // Try just inches (e.g., "234.5")
        if (double.TryParse(input.Replace("\"", ""), out var totalInches))
        {
            return totalInches;
        }
        
        return null;
    }
    
    /// <summary>
    /// Format seconds to display string
    /// </summary>
    public static string FormatTime(double seconds)
    {
        if (seconds >= 60)
        {
            var minutes = (int)(seconds / 60);
            var remainder = seconds % 60;
            return $"{minutes}:{remainder:00.00}";
        }
        
        return $"{seconds:0.00}";
    }
    
    /// <summary>
    /// Format inches to display string (feet and inches)
    /// </summary>
    public static string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }
    
    /// <summary>
    /// Format performance based on whether it's time or distance
    /// </summary>
    public static string FormatPerformance(double? timeSeconds, double? distanceInches)
    {
        if (timeSeconds.HasValue)
        {
            return FormatTime(timeSeconds.Value);
        }
        
        if (distanceInches.HasValue)
        {
            return FormatDistance(distanceInches.Value);
        }
        
        return "N/A";
    }
    
    /// <summary>
    /// Calculate improvement between two performances (for PR preview)
    /// </summary>
    public static string FormatImprovement(double? currentTime, double? previousTime, 
                                          double? currentDistance, double? previousDistance)
    {
        if (currentTime.HasValue && previousTime.HasValue)
        {
            var diff = previousTime.Value - currentTime.Value;
            if (diff > 0)
            {
                return $"-{diff:0.00}s faster";
            }
            else
            {
                return $"+{Math.Abs(diff):0.00}s slower";
            }
        }
        
        if (currentDistance.HasValue && previousDistance.HasValue)
        {
            var diff = currentDistance.Value - previousDistance.Value;
            if (diff > 0)
            {
                var feet = Math.Floor(Math.Abs(diff) / 12);
                var inches = Math.Abs(diff) % 12;
                return $"+{feet:0}' {inches:0.##}\" farther";
            }
            else
            {
                var feet = Math.Floor(Math.Abs(diff) / 12);
                var inches = Math.Abs(diff) % 12;
                return $"-{feet:0}' {inches:0.##}\" shorter";
            }
        }
        
        return "";
    }
}
