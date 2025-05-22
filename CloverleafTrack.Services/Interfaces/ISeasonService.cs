using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services.Interfaces;

public interface ISeasonService
{
    public Task<int> GetCurrentSeasonAsync();
    public Task<List<SeasonCardViewModel>> GetSeasonCardsAsync();
}