using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAthleteSummaryRepository
{
    Task<AthleteSummary?> GetForAthleteAsync(int athleteId);
    Task<List<AthleteSummary>> GetForAthletesAsync(IEnumerable<int> athleteIds);

    /// <summary>Inserts or replaces the summary for an athlete. Always resets Status to Draft.</summary>
    Task SaveDraftAsync(int athleteId, string text, string inputHash, string model);
    Task UpdateTextAsync(int id, string text);
    Task SetApprovedAsync(int id, bool approved);
    Task DeleteAsync(int id);
}
