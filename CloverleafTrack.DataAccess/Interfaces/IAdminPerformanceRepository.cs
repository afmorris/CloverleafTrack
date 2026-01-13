using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminPerformanceRepository
{
    Task<List<Performance>> GetAllPerformancesAsync();
    Task<List<Performance>> GetPerformancesByMeetAsync(int meetId);
    Task<Performance?> GetPerformanceByIdAsync(int id);
    Task<int> CreatePerformanceAsync(Performance performance);
    Task<bool> UpdatePerformanceAsync(Performance performance);
    Task<bool> DeletePerformanceAsync(int id);
    Task<Performance?> CheckDuplicatePerformanceAsync(int meetId, int eventId, int? athleteId);
    Task<Performance?> GetAthleteCurrentPRAsync(int athleteId, int eventId);
    Task<List<int>> GetRelayAthleteIdsAsync(int performanceId);
    Task<bool> AddRelayAthleteAsync(int performanceId, int athleteId);
    Task<bool> RemoveAllRelayAthletesAsync(int performanceId);
}