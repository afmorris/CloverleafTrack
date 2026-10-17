using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Services;

public class MeetSummaryService(
    IAdminMeetRepository meetRepository,
    IMeetService meetService,
    IMeetSummaryRepository summaryRepository,
    ISummaryTextGenerator generator) : IMeetSummaryService
{
    public async Task<SummaryGenerationOutcome> GenerateAsync(int meetId, bool force = false, CancellationToken cancellationToken = default)
    {
        var meet = await meetRepository.GetByIdAsync(meetId)
                   ?? throw new ArgumentException($"Meet {meetId} not found.", nameof(meetId));

        var details = await meetService.GetMeetDetailsAsync(meet.Slug)
                      ?? throw new InvalidOperationException($"Meet '{meet.Name}' could not be loaded by slug '{meet.Slug}'.");
        if (details.TotalPerformances == 0)
        {
            return SummaryGenerationOutcome.NoData;
        }

        var factSheet = MeetSummaryPrompt.BuildFactSheet(details, meet.MeetLevel);
        var hash = MeetSummaryPrompt.ComputeHash(factSheet);

        if (!force)
        {
            var existing = await summaryRepository.GetForMeetAsync(meetId);
            if (existing != null && existing.InputHash == hash)
            {
                return SummaryGenerationOutcome.UpToDate;
            }
        }

        var generated = await generator.GenerateAsync(
            MeetSummaryPrompt.SystemPrompt, MeetSummaryPrompt.BuildUserPrompt(factSheet), cancellationToken);
        await summaryRepository.SaveDraftAsync(meetId, generated.Text, hash, generated.Model);
        return SummaryGenerationOutcome.Generated;
    }

    public async Task<string?> GetFactSheetAsync(int meetId)
    {
        var (_, factSheet) = await LoadFactSheetAsync(meetId);
        return factSheet;
    }

    public async Task SaveManualDraftAsync(int meetId, string text, string model)
    {
        var (_, factSheet) = await LoadFactSheetAsync(meetId);
        if (factSheet == null) throw new InvalidOperationException("Meet has no performances to summarize.");
        await summaryRepository.SaveDraftAsync(meetId, text.Trim(), MeetSummaryPrompt.ComputeHash(factSheet), model);
    }

    private async Task<(Models.Meet meet, string? factSheet)> LoadFactSheetAsync(int meetId)
    {
        var meet = await meetRepository.GetByIdAsync(meetId)
                   ?? throw new ArgumentException($"Meet {meetId} not found.", nameof(meetId));
        var details = await meetService.GetMeetDetailsAsync(meet.Slug)
                      ?? throw new InvalidOperationException($"Meet '{meet.Name}' could not be loaded by slug '{meet.Slug}'.");
        return (meet, details.TotalPerformances == 0 ? null : MeetSummaryPrompt.BuildFactSheet(details, meet.MeetLevel));
    }
}
