namespace CloverleafTrack.Services.Interfaces;

public enum SummaryGenerationOutcome
{
    Generated,
    /// <summary>Fact sheet unchanged since the stored summary was written; nothing was sent to the model.</summary>
    UpToDate,
    /// <summary>Meet has no recorded performances, so there is nothing to summarize.</summary>
    NoData
}

public interface IMeetSummaryService
{
    /// <summary>Generates (or regenerates) a Draft summary for one meet. Skips the model call when the fact sheet is unchanged unless <paramref name="force"/>.</summary>
    Task<SummaryGenerationOutcome> GenerateAsync(int meetId, bool force = false, CancellationToken cancellationToken = default);

    /// <summary>The exact JSON fact sheet the model would be given for this meet (null when the meet has no performances).</summary>
    Task<string?> GetFactSheetAsync(int meetId);

    /// <summary>Saves externally authored text (e.g. written by Claude Code in a session) as a Draft with the current fact-sheet hash, so staleness tracking still works.</summary>
    Task SaveManualDraftAsync(int meetId, string text, string model);
}
