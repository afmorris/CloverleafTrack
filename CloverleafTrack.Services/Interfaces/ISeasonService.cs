using CloverleafTrack.ViewModels;
using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Services.Interfaces;

public interface ISeasonService
{
    public Task<int> GetCurrentSeasonAsync();
    public Task<List<SeasonCardViewModel>> GetSeasonCardsAsync();
    public Task<SeasonDetailsViewModel?> GetSeasonDetailsAsync(string name);
}