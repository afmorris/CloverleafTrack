using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ISeasonWrapRepository
{
    /// <summary>Ids of seasons that have at least one recorded mark.</summary>
    Task<List<int>> GetSeasonIdsWithMarksAsync();

    /// <summary>Every recorded mark (individual and relay, no-mark rows excluded) at meets in the season.</summary>
    Task<List<SeasonWrapMarkDto>> GetMarksAsync(int seasonId);

    /// <summary>For athletes who competed in the season: their best per event before <paramref name="seasonStart"/>.</summary>
    Task<List<SeasonWrapPriorBestDto>> GetPriorBestsAsync(int seasonId, DateTime seasonStart);

    /// <summary>Dual / double dual team results with both scores recorded, one row per gender per opponent.</summary>
    Task<List<SeasonWrapTeamResultDto>> GetTeamResultsAsync(int seasonId);
}
