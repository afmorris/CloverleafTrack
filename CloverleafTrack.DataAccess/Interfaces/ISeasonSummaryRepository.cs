using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ISeasonSummaryRepository
{
    Task<SeasonSummary?> GetForSeasonAsync(int seasonId);
    Task<List<SeasonSummary>> GetAllAsync();

    /// <summary>The Approved text for a season, or null when there is none or it is only a Draft. This is the only read the public site uses.</summary>
    Task<string?> GetApprovedTextAsync(int seasonId);

    /// <summary>Inserts or replaces the summary for a season. Always resets Status to Draft (new text needs a new review).</summary>
    Task SaveDraftAsync(int seasonId, string text, string inputHash, string model);

    /// <summary>Reviewer edit: replaces the text but keeps InputHash/Model. Resets Status to Draft.</summary>
    Task UpdateTextAsync(int id, string text);
    Task SetApprovedAsync(int id, bool approved);
    Task DeleteAsync(int id);
}
