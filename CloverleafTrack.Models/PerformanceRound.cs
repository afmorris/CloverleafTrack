using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

/// <summary>
/// One round's time (prelim / semifinal / final) for a running or relay performance.
/// Additive, optional child data for <see cref="Performance"/> — same contract as
/// <see cref="PerformanceAttempt"/>: a Performance row never requires round rows, and
/// Performances.TimeSeconds is always the best (min) round time. Field-event rounds are
/// stored as attempts instead. See docs/schema.sql "Performance Rounds".
/// </summary>
public class PerformanceRound
{
    public int Id { get; set; }
    public int PerformanceId { get; set; }
    public byte Round { get; set; }
    public double TimeSeconds { get; set; }

    public RoundType RoundType => (RoundType)Round;
}

/// <summary>Pure, DB-free computation over a set of round marks (unit-testable).</summary>
public static class PerformanceRoundSeries
{
    /// <summary>Best (min) time across the rounds, or null when there are none.</summary>
    public static double? BestTime(IEnumerable<PerformanceRound> rounds)
    {
        var times = rounds.Select(r => r.TimeSeconds).ToList();
        return times.Count == 0 ? null : times.Min();
    }
}
