using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetEntryRepository
{
    Task<List<MeetEntryDto>> GetForMeetAsync(int meetId);
    Task<MeetEntryDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(MeetEntry entry);
    Task<bool> UpdatePerformanceIdAsync(int entryId, int performanceId);
    Task<bool> DeleteAsync(int id);
    Task AddRelayAthletesAsync(int meetEntryId, IEnumerable<int> athleteIds);
    Task RemoveRelayAthletesAsync(int meetEntryId);
    Task<List<int>> GetRelayAthleteIdsAsync(int meetEntryId);

    /// <summary>Returns the number of events (individual + relay) the athlete is entered in for this meet.</summary>
    Task<int> GetAthleteEventCountForMeetAsync(int meetId, int athleteId);
}
