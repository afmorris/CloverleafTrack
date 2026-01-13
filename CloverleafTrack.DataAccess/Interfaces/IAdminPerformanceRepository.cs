using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminPerformanceRepository
{
    Task<Performance?> GetByIdAsync(int id);
    Task<List<Performance>> GetAllWithDetailsAsync();
    Task<int> CreateAsync(Performance performance);
    Task<bool> UpdateAsync(Performance performance);
    Task<bool> DeleteAsync(int id);
    Task<List<Performance>> GetPerformancesForMeetAsync(int meetId);
    Task<Performance?> GetSimilarPerformanceAsync(int meetId, int eventId, int? athleteId);
    Task<Performance?> GetBestPerformanceForAthleteEventAsync(int athleteId, int eventId);
    Task<int> CreatePerformanceAthleteAsync(int performanceId, int athleteId);
    Task<bool> DeletePerformanceAthletesAsync(int performanceId);
    Task<List<int>> GetAthleteIdsForPerformanceAsync(int performanceId);
}