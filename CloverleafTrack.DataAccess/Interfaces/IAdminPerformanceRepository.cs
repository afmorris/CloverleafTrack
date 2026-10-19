using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminPerformanceRepository
{
    Task<Performance?> GetByIdAsync(int id);
    Task<List<Performance>> GetAllWithDetailsAsync();
    Task<int> CreateAsync(Performance performance);
    Task<bool> UpdateAsync(Performance performance);
    Task<bool> DeleteAsync(int id);
    Task<List<Performance>> GetPerformancesForMeetAsync(int meetId);
    Task<Performance?> GetSimilarPerformanceAsync(int meetId, int eventId, int? athleteId);
    Task<Performance?> GetBestPerformanceForAthleteEventAsync(int athleteId, int eventId);
    Task<int> CreatePerformanceAthleteAsync(int performanceId, int athleteId);
    Task<bool> DeletePerformanceAthletesAsync(int performanceId);
    Task<List<int>> GetAthleteIdsForPerformanceAsync(int performanceId);

    /// <summary>Loads the attempt series for a single performance, ordered by AttemptNumber. Empty list if none recorded.</summary>
    Task<List<PerformanceAttempt>> GetAttemptsForPerformanceAsync(int performanceId);

    /// <summary>
    /// Replaces the full attempt series for a performance, recomputes Performances.DistanceInches
    /// as the best (max) valid attempt, and calls sp_RebuildLeaderboards — mirroring every other
    /// performance write in this repository. If no attempt in the series is valid (all foul/pass),
    /// Performances.DistanceInches is left untouched: CK_Performances_DistanceOrTime requires a
    /// non-null distance for field events, and an all-foul/all-pass series gives no new value to
    /// replace it with. See BRAIN.md for the full reasoning.
    /// </summary>
    Task SaveAttemptSeriesAsync(int performanceId, List<PerformanceAttempt> attempts);

    /// <summary>Loads the round rows (prelim/semi/final) for a single performance, ordered by Round. Empty list if none.</summary>
    Task<List<PerformanceRound>> GetRoundsForPerformanceAsync(int performanceId);

    /// <summary>
    /// Replaces the full round set for a performance, sets Performances.TimeSeconds to the best (min)
    /// round time, and calls sp_RebuildLeaderboards. An empty list just clears the rounds and leaves
    /// TimeSeconds untouched.
    /// </summary>
    Task SaveRoundsAsync(int performanceId, List<PerformanceRound> rounds);

    /// <summary>Loads the bar ladder for a single high jump / pole vault performance, ordered by height. Empty list if none.</summary>
    Task<List<PerformanceBar>> GetBarsForPerformanceAsync(int performanceId);

    /// <summary>
    /// Replaces the full bar ladder for a performance, sets Performances.DistanceInches to the best cleared
    /// height (when any bar was cleared) and calls sp_RebuildLeaderboards. An empty list just clears the ladder.
    /// </summary>
    Task SaveBarsAsync(int performanceId, List<PerformanceBar> bars);
}
