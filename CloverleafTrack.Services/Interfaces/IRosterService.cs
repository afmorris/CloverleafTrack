using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services.Interfaces;

public interface IRosterService
{
    Task<RosterViewModel> GetRosterAsync(int currentSeason);
}