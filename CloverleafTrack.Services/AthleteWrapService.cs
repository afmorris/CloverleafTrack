using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Athletes;

namespace CloverleafTrack.Services;

public class AthleteWrapService(IAthleteService athleteService, ISeasonService seasonService) : IAthleteWrapService
{
    public async Task<AthleteWrapViewModel?> GetAthleteWrapAsync(string slug)
    {
        var currentSeason = await seasonService.GetCurrentSeasonAsync();
        var details = await athleteService.GetAthleteDetailsAsync(slug, currentSeason);
        return details == null ? null : AthleteWrapCalculator.Build(details, slug.ToLowerInvariant());
    }
}
