using CloverleafTrack.ViewModels.Scoring;

namespace CloverleafTrack.Services.Interfaces;

public interface IScoringService
{
    Task<SeasonScoringViewModel?> GetSeasonScoringAsync(int seasonId);
}
