using CloverleafTrack.ViewModels.Athletes;

namespace CloverleafTrack.Services.Interfaces;

public interface IAthleteWrapService
{
    /// <summary>The career wrap for an athlete slug; null when no such athlete exists.</summary>
    Task<AthleteWrapViewModel?> GetAthleteWrapAsync(string slug);
}
