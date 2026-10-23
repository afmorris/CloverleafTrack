using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Services.Interfaces;

public interface ISeasonWrapService
{
    /// <summary>The season wrap for a season name such as "2025-2026"; null when no such season exists.</summary>
    Task<SeasonWrapViewModel?> GetSeasonWrapAsync(string seasonName);
}
