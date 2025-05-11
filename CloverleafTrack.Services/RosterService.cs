using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services;

public class RosterService(IAthleteService athleteService) : IRosterService
{
    public async Task<RosterViewModel> GetRosterAsync(int currentSeason)
    {
        var active = await athleteService.GetActiveAthletesAsync(currentSeason);
        var graduated = await athleteService.GetGraduatedAthletesAsync(currentSeason);

        return new RosterViewModel
        {
            ActiveAthletes = active,
            GraduatedAthletes = graduated
        };
    }
}