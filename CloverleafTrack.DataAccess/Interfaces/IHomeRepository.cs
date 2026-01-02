using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IHomeRepository
{
    Task<HomePageStatsDto> GetHomePageStatsAsync(int currentSeasonId);
    Task<OnThisDayDto?> GetPerformanceOnThisDayAsync(int month, int day);
    Task<RecentHighlightDto?> GetRecentTopPerformanceAsync(int currentSeasonId);
    Task<ImprovementDto?> GetBiggestImprovementThisSeasonAsync(int currentSeasonId);
    Task<BreakoutAthleteDto?> GetBreakoutAthleteAsync(int currentSeasonId);
    Task<List<SeasonLeaderDto>> GetSeasonLeadersAsync(Gender gender, int currentSeasonId);
    Task<List<UpcomingMeetDto>> GetUpcomingMeetsAsync(int count = 5);
}
