using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Services;

public class SeasonSummaryService(
    IAdminSeasonRepository seasonRepository,
    ISeasonWrapService wrapService,
    ISeasonSummaryRepository summaryRepository) : ISeasonSummaryService
{
    public async Task<string?> GetFactSheetAsync(int seasonId) => await LoadFactSheetAsync(seasonId);

    public async Task SaveManualDraftAsync(int seasonId, string text, string model)
    {
        var factSheet = await LoadFactSheetAsync(seasonId)
                        ?? throw new InvalidOperationException("Season has no results to summarize.");
        await summaryRepository.SaveDraftAsync(seasonId, text.Trim(), SeasonSummaryPrompt.ComputeHash(factSheet), model);
    }

    private async Task<string?> LoadFactSheetAsync(int seasonId)
    {
        var season = await seasonRepository.GetByIdAsync(seasonId)
                     ?? throw new ArgumentException($"Season {seasonId} not found.", nameof(seasonId));
        var wrap = await wrapService.GetSeasonWrapAsync(season.Name);
        return wrap is { HasData: true } ? SeasonSummaryPrompt.BuildFactSheet(wrap) : null;
    }
}
