namespace CloverleafTrack.Models;

/// <summary>
/// One attempt (of up to 6) within a field-event performance's attempt series.
/// Additive, optional child data for <see cref="Performance"/> — a Performance
/// row never requires PerformanceAttempts rows to exist. See docs/schema.sql
/// "PerformanceAttempts" section for the table definition and CHECK constraint.
/// </summary>
public class PerformanceAttempt
{
    public int Id { get; set; }
    public int PerformanceId { get; set; }

    /// <summary>1..6 — which attempt in the series this is.</summary>
    public byte AttemptNumber { get; set; }

    /// <summary>NULL when IsFoul or IsPass is true; otherwise the valid mark for this attempt.</summary>
    public double? DistanceInches { get; set; }

    public bool IsFoul { get; set; }
    public bool IsPass { get; set; }

    /// <summary>True when this attempt has a recorded, valid (non-foul, non-pass) mark.</summary>
    public bool IsValid => !IsFoul && !IsPass && DistanceInches.HasValue;
}

/// <summary>
/// Pure, DB-free computation over an attempt series. Extracted so the
/// "what is the best mark" rule can be unit tested without a SQL Server
/// connection (see CloverleafTrack.Tests/Unit/Models/PerformanceAttemptTests.cs).
/// </summary>
public static class PerformanceAttemptSeries
{
    /// <summary>
    /// Returns the best (max) valid attempt distance in the series, or null when there is
    /// no valid attempt (e.g. every attempt was a foul or a pass). Order of attempts in the
    /// input does not matter — the best mark is not necessarily the last attempt taken.
    /// </summary>
    public static double? BestValidDistance(IEnumerable<PerformanceAttempt> attempts)
    {
        var validDistances = attempts
            .Where(a => a.IsValid)
            .Select(a => a.DistanceInches!.Value)
            .ToList();

        return validDistances.Count == 0 ? null : validDistances.Max();
    }
}
