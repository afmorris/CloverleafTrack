using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IMeetRepository
{
    public Task<List<Meet>> GetMeetsForSeasonAsync(int seasonId);
    Task<Meet?> GetMeetBasicInfoBySlugAsync(string slug);
    Task<List<MeetPerformanceDto>> GetPerformancesForMeetAsync(int meetId);
}