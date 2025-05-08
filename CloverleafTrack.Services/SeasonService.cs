using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.DataAccess.Interfaces;

namespace CloverleafTrack.Services;

public class SeasonService(ISeasonRepository seasonRepository) : ISeasonService
{
    public async Task<int> GetCurrentSeasonAsync()
    {
        var allSeasons = await seasonRepository.GetAllAsync();
        var currentSeason = allSeasons.FirstOrDefault(s => s.IsCurrentSeason);

        if (currentSeason == null)
        {
            throw new InvalidOperationException("No current season found.");
        }

        return currentSeason.EndDate.Year;
    }
}