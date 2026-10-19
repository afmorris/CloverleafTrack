using CloverleafTrack.Models;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.Services;

/// <summary>Maps a flat batch of PerformanceRound rows to a PerformanceId → series lookup (mirrors PerformanceAttemptSeriesBuilder).</summary>
public static class PerformanceRoundSeriesBuilder
{
    /// <summary>Only performances with at least one round row appear as keys; callers fall back to an empty series.</summary>
    public static Dictionary<int, PerformanceRoundSeriesViewModel> BuildLookup(IEnumerable<PerformanceRound> rounds)
    {
        var lookup = new Dictionary<int, PerformanceRoundSeriesViewModel>();

        foreach (var group in rounds.GroupBy(r => r.PerformanceId))
        {
            var ordered = group
                .OrderBy(r => r.Round)
                .Select(r => new PerformanceRoundViewModel { Round = r.Round, TimeSeconds = r.TimeSeconds })
                .ToList();

            var best = PerformanceRoundSeries.BestTime(group);
            if (best.HasValue)
            {
                // Ties: the later round is the one displayed as best (the athlete matched it in the final).
                ordered.Last(r => r.TimeSeconds == best.Value).IsBest = true;
            }

            lookup[group.Key] = new PerformanceRoundSeriesViewModel { Rounds = ordered };
        }

        return lookup;
    }
}
