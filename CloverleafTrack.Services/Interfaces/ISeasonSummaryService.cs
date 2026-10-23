namespace CloverleafTrack.Services.Interfaces;

public interface ISeasonSummaryService
{
    /// <summary>The exact JSON fact sheet the writer is given for a season (null when the season has no results).</summary>
    Task<string?> GetFactSheetAsync(int seasonId);

    /// <summary>Saves chat-authored text as a Draft with the current fact-sheet hash (no model call; AI never runs in the site).</summary>
    Task SaveManualDraftAsync(int seasonId, string text, string model);
}
