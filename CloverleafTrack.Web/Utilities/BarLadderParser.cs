using CloverleafTrack.Models;

namespace CloverleafTrack.Web.Utilities;

/// <summary>
/// Parses/formats the admin "bars" text for high jump / pole vault, e.g. <c>4'0" O; 4'2" XO; 4'4" XXX</c>.
/// Entries are separated by new lines, semicolons or commas; each is a height (anything
/// <see cref="PerformanceFormatHelper.ParseDistance"/> accepts) followed by an outcome (O, XO, XXO, XXX, P, XP, XXP).
/// </summary>
public static class BarLadderParser
{
    public static List<PerformanceBar> Parse(string? input, out List<string> errors)
    {
        errors = new List<string>();
        var bars = new List<PerformanceBar>();
        if (string.IsNullOrWhiteSpace(input)) return bars;

        foreach (var raw in input.Split(new[] { '\n', '\r', ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var entry = raw.Trim();
            if (entry.Length == 0) continue;

            var split = entry.LastIndexOfAny(new[] { ' ', '\t' });
            if (split < 0)
            {
                errors.Add($"'{entry}': expected a height and an outcome (e.g. 4'4\" XXO)");
                continue;
            }

            var heightText = entry[..split].Trim();
            var outcome = entry[(split + 1)..].Trim().ToUpperInvariant();
            var height = PerformanceFormatHelper.ParseDistance(heightText);

            if (height == null) { errors.Add($"'{entry}': could not read the height"); continue; }
            if (!PerformanceBarSeries.IsValidOutcome(outcome)) { errors.Add($"'{entry}': outcome must be one of {string.Join(", ", PerformanceBarSeries.ValidOutcomes)}"); continue; }
            if (bars.Any(b => b.HeightInches == height.Value)) { errors.Add($"'{entry}': duplicate height"); continue; }

            bars.Add(new PerformanceBar { HeightInches = height.Value, Outcome = outcome });
        }

        return bars.OrderBy(b => b.HeightInches).ToList();
    }

    public static string Format(IEnumerable<PerformanceBar> bars) =>
        string.Join("\n", bars.OrderBy(b => b.HeightInches).Select(b => $"{PerformanceFormatHelper.FormatDistance(b.HeightInches)} {b.Outcome}"));

    /// <summary>Only individual high jump and pole vault events take a bar ladder.</summary>
    public static bool IsVerticalJump(string? eventName) =>
        eventName != null && !eventName.Contains("Relay", StringComparison.OrdinalIgnoreCase) &&
        (eventName.Contains("High Jump", StringComparison.OrdinalIgnoreCase) || eventName.Contains("Pole Vault", StringComparison.OrdinalIgnoreCase));
}
