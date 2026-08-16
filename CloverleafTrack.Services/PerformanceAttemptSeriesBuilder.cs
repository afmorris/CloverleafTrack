using CloverleafTrack.Models;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.Services;

/// <summary>
/// Shared mapping logic used by MeetService, AthleteService, and LeaderboardService to
/// turn a flat batch of PerformanceAttempt rows (from IPerformanceAttemptRepository) into
/// a PerformanceId → PerformanceAttemptSeriesViewModel lookup.
/// </summary>
public static class PerformanceAttemptSeriesBuilder
{
    /// <summary>
    /// Builds the lookup. Only performances that actually have attempt rows appear as keys —
    /// callers should fall back to a fresh, empty PerformanceAttemptSeriesViewModel (HasAttempts
    /// == false) for any PerformanceId not present, so displays stay silent for performances
    /// with no recorded series.
    /// </summary>
    public static Dictionary<int, PerformanceAttemptSeriesViewModel> BuildLookup(IEnumerable<PerformanceAttempt> attempts)
    {
        var lookup = new Dictionary<int, PerformanceAttemptSeriesViewModel>();

        foreach (var group in attempts.GroupBy(a => a.PerformanceId))
        {
            var attemptViewModels = group
                .OrderBy(a => a.AttemptNumber)
                .Select(a => new PerformanceAttemptViewModel
                {
                    AttemptNumber = a.AttemptNumber,
                    DistanceInches = a.DistanceInches,
                    IsFoul = a.IsFoul,
                    IsPass = a.IsPass
                })
                .ToList();

            var bestDistance = PerformanceAttemptSeries.BestValidDistance(group);
            if (bestDistance.HasValue)
            {
                var best = attemptViewModels.First(a => a.IsValid && a.DistanceInches!.Value == bestDistance.Value);
                best.IsBest = true;
            }

            lookup[group.Key] = new PerformanceAttemptSeriesViewModel { Attempts = attemptViewModels };
        }

        return lookup;
    }
}
