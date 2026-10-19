namespace CloverleafTrack.Models;

/// <summary>
/// One bar height in a high jump / pole vault ladder. Additive, display-only child data for
/// <see cref="Performance"/> (same contract as <see cref="PerformanceAttempt"/>): a Performance never
/// requires bar rows, and Performances.DistanceInches is always the best cleared height.
/// Outcome lists the attempts at this height in order: X = miss, O = clear, P = pass.
/// </summary>
public class PerformanceBar
{
    public int Id { get; set; }
    public int PerformanceId { get; set; }
    public double HeightInches { get; set; }
    public string Outcome { get; set; } = string.Empty;

    public bool IsCleared => Outcome.EndsWith('O');
    public bool IsPass => Outcome.EndsWith('P');
    public int Misses => Outcome.Count(c => c == 'X');
}

/// <summary>Pure, DB-free helpers over a bar ladder (unit-testable).</summary>
public static class PerformanceBarSeries
{
    public static readonly string[] ValidOutcomes = { "O", "XO", "XXO", "XXX", "P", "XP", "XXP" };

    public static bool IsValidOutcome(string? outcome) => outcome != null && ValidOutcomes.Contains(outcome);

    /// <summary>Highest bar with a clear, or null when no bar was cleared (a "no height").</summary>
    public static double? BestClearedHeight(IEnumerable<PerformanceBar> bars)
    {
        var cleared = bars.Where(b => b.IsCleared).Select(b => b.HeightInches).ToList();
        return cleared.Count == 0 ? null : cleared.Max();
    }
}
