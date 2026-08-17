using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Helpers;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAthleteRepository
{
    Task<List<Athlete>> GetAllAsync();
    Task<Athlete?> GetByIdAsync(int id);
    Task<List<AthleteEventParticipation>> GetAllWithPerformancesAsync();
    Task<int> CreateAsync(Athlete athlete);
    Task<bool> UpdateAsync(Athlete athlete);
    Task<bool> DeleteAsync(Athlete athlete);
    Task<Athlete?> GetBySlugWithBasicInfoAsync(string slug);
    Task<List<AthletePerformanceDto>> GetAllPerformancesForAthleteAsync(int athleteId);

    /// <summary>Current all-time-best value per event (Leaderboards.Rank = 1), for the given EventIds. Used by the career chart (issue #26) — never derive this from Performances.SchoolRecord, which is a stale snapshot.</summary>
    Task<List<EventRecordDto>> GetSchoolRecordsForEventsAsync(IEnumerable<int> eventIds);
}