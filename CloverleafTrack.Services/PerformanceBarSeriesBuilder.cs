using CloverleafTrack.Models;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.Services;

/// <summary>Maps a flat batch of PerformanceBar rows to a PerformanceId → ladder lookup (mirrors PerformanceAttemptSeriesBuilder).</summary>
public static class PerformanceBarSeriesBuilder
{
    public static Dictionary<int, PerformanceBarSeriesViewModel> BuildLookup(IEnumerable<PerformanceBar> bars)
    {
        var lookup = new Dictionary<int, PerformanceBarSeriesViewModel>();

        foreach (var group in bars.GroupBy(b => b.PerformanceId))
        {
            var ordered = group
                .OrderBy(b => b.HeightInches)
                .Select(b => new PerformanceBarViewModel { HeightInches = b.HeightInches, Outcome = b.Outcome })
                .ToList();

            var best = PerformanceBarSeries.BestClearedHeight(group);
            if (best.HasValue)
            {
                ordered.First(b => b.IsCleared && b.HeightInches == best.Value).IsBest = true;
            }

            lookup[group.Key] = new PerformanceBarSeriesViewModel { Bars = ordered };
        }

        return lookup;
    }
}
