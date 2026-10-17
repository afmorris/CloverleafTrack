using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using Slugify;

namespace CloverleafTrack.Services;

public interface IAthleteSummaryService
{
    /// <summary>The exact JSON fact sheet a writer is given for this athlete (null when the athlete has no recorded performances).</summary>
    Task<string?> GetFactSheetAsync(int athleteId);

    /// <summary>Saves externally authored text (written in a Claude Code session) as a Draft with the current fact-sheet hash.</summary>
    Task SaveManualDraftAsync(int athleteId, string text, string model);
}

public class AthleteSummaryService(
    IAdminAthleteRepository athleteRepository,
    IAthleteService athleteService,
    IAthleteSummaryRepository summaryRepository) : IAthleteSummaryService
{
    public async Task<string?> GetFactSheetAsync(int athleteId)
    {
        var athlete = await athleteRepository.GetByIdAsync(athleteId)
                      ?? throw new ArgumentException($"Athlete {athleteId} not found.", nameof(athleteId));
        // The details page's slug rule is "first-last"; the class argument only affects a display label we don't use.
        var slug = new SlugHelper().GenerateSlug($"{athlete.FirstName}-{athlete.LastName}");
        var details = await athleteService.GetAthleteDetailsAsync(slug, athlete.GraduationYear);
        return details == null || details.Seasons.Count == 0 ? null : AthleteSummaryPrompt.BuildFactSheet(details);
    }

    public async Task SaveManualDraftAsync(int athleteId, string text, string model)
    {
        var factSheet = await GetFactSheetAsync(athleteId)
                        ?? throw new InvalidOperationException("Athlete has no performances to summarize.");
        await summaryRepository.SaveDraftAsync(athleteId, text.Trim(), AthleteSummaryPrompt.ComputeHash(factSheet), model);
    }
}
