using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ISeasonScoringRepository
{
    /// <summary>
    /// Returns one row per athlete per placing for every scored meet in the season.
    /// Relay performances are expanded so each relay member has their own row.
    /// </summary>
    Task<List<ScoringDataDto>> GetScoringDataForSeasonAsync(int seasonId);
}
