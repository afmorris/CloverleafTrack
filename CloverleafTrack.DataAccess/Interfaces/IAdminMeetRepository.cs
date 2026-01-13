using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetRepository
{
    Task<List<Meet>> GetAllMeetsAsync();
    Task<List<Meet>> GetMeetsByFiltersAsync(string? searchTerm, int? seasonId, Environment? environment, MeetEntryStatus? entryStatus);
    Task<Meet?> GetMeetByIdAsync(int id);
    Task<Meet?> GetMeetWithDetailsAsync(int id);
    Task<int> CreateMeetAsync(Meet meet);
    Task<bool> UpdateMeetAsync(Meet meet);
    Task<bool> DeleteMeetAsync(int id);
    Task<List<Meet>> GetRecentMeetsAsync(int count = 5);
}