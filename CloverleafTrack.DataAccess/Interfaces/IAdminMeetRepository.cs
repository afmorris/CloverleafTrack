using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetRepository
{
    Task<List<Meet>> GetAllAsync();
    Task<List<Meet>> GetFilteredAsync(string? searchName, int? seasonId, Environment? environment, MeetEntryStatus? entryStatus);
    Task<Meet?> GetByIdAsync(int id);
    Task<Meet?> GetByIdWithDetailsAsync(int id);
    Task<int> CreateAsync(Meet meet);
    Task<bool> UpdateAsync(Meet meet);
    Task<bool> DeleteAsync(int id);
    Task<int> GetPerformanceCountAsync(int meetId);
    Task<List<Meet>> GetRecentMeetsAsync(int count = 5);
}
