namespace CloverleafTrack.Tests.TestSupport;

/// <summary>
/// Pure C# mirror of the T-SQL percentile / median / Q1 / Q3 algorithm implemented in
/// <c>sp_RebuildLeaderboards</c> (see <c>docs/schema.sql</c>, Steps 11-12).
///
/// This class is NOT part of the production app and is never called from
/// CloverleafTrack.Services or CloverleafTrack.DataAccess — CloverleafTrack computes and
/// stores Percentile / MedianValue / Q1Value / Q3Value in SQL Server via the stored
/// procedure, per the "single pipeline, not a second one" rule for this feature (see
/// BRAIN.md). It exists purely so the SQL algorithm's behavior can be exercised and
/// documented by xUnit tests in this sandbox, where no real SQL Server instance is
/// available to run the T-SQL directly. Keep this in lockstep with the SQL — if the SQL
/// changes, update this mirror and its tests too.
/// </summary>
public static class PercentileMath
{
    public sealed record Mark(int PerformanceId, double Value);

    /// <summary>
    /// Computes the 1-100 percentile for every mark in a single event's population,
    /// mirroring the SQL: COUNT(*) OVER (PARTITION BY EventId ORDER BY value
    /// RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) to count marks "not better
    /// than" the current one (ties included), then
    /// percentile = ROUND(100 * (total - countBetterOrEqual) / total), clamped 1..100.
    /// A population of exactly one mark is special-cased to 100 (see docs/schema.sql
    /// Step 11 comment for the rationale).
    /// </summary>
    /// <param name="marks">All marks sharing one EventId — the population must already
    /// be scoped to a single event by the caller; this method does not partition.</param>
    /// <param name="betterIsHigher">True for field events (higher DistanceInches is
    /// better); false for running events (lower TimeSeconds is better).</param>
    public static Dictionary<int, int> ComputePercentiles(IReadOnlyCollection<Mark> marks, bool betterIsHigher)
    {
        var result = new Dictionary<int, int>();
        var total = marks.Count;
        if (total == 0)
        {
            return result;
        }

        if (total == 1)
        {
            result[marks.Single().PerformanceId] = 100;
            return result;
        }

        foreach (var mark in marks)
        {
            var countBetterOrEqual = betterIsHigher
                ? marks.Count(m => m.Value >= mark.Value)
                : marks.Count(m => m.Value <= mark.Value);

            var countWorse = total - countBetterOrEqual;
            var raw = (int)Math.Round(100.0 * countWorse / total, MidpointRounding.AwayFromZero);
            result[mark.PerformanceId] = Math.Clamp(raw, 1, 100);
        }

        return result;
    }

    /// <summary>
    /// Computes (Median, Q1, Q3) using linear interpolation between order statistics —
    /// the same method as SQL Server's PERCENTILE_CONT. Returns (null, null, null) when
    /// the sample is smaller than <paramref name="minimumSampleSize"/> (default 10),
    /// mirroring the NULL-when-sparse rule in EventStatistics.
    /// </summary>
    public static (double? Median, double? Q1, double? Q3) ComputeQuartiles(
        IReadOnlyCollection<double> values,
        int minimumSampleSize = 10)
    {
        if (values.Count < minimumSampleSize)
        {
            return (null, null, null);
        }

        var sorted = values.OrderBy(v => v).ToArray();
        return (
            PercentileContinuous(sorted, 0.5),
            PercentileContinuous(sorted, 0.25),
            PercentileContinuous(sorted, 0.75));
    }

    private static double PercentileContinuous(IReadOnlyList<double> sorted, double p)
    {
        var n = sorted.Count;
        var rank = p * (n - 1);
        var lowerIndex = (int)Math.Floor(rank);
        var upperIndex = (int)Math.Ceiling(rank);

        if (lowerIndex == upperIndex)
        {
            return sorted[lowerIndex];
        }

        var fraction = rank - lowerIndex;
        return sorted[lowerIndex] + (sorted[upperIndex] - sorted[lowerIndex]) * fraction;
    }
}
