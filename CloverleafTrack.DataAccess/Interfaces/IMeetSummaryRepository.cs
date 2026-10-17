using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IMeetSummaryRepository
{
    Task<MeetSummary?> GetForMeetAsync(int meetId);
    Task<List<MeetSummary>> GetForMeetsAsync(IEnumerable<int> meetIds);

    /// <summary>Inserts or replaces the summary for a meet. Always resets Status to Draft (new text needs a new review).</summary>
    Task SaveDraftAsync(int meetId, string text, string inputHash, string model);

    /// <summary>Reviewer edit: replaces the text but keeps the existing InputHash/Model. Resets Status to Draft.</summary>
    Task UpdateTextAsync(int id, string text);
    Task SetApprovedAsync(int id, bool approved);
    Task DeleteAsync(int id);
}
