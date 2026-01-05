using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IHomeRepository
{
    Task<HomePageStatsDto> GetHomePageStatsAsync(int currentSeasonId);
    Task<OnThisDayDto?> GetPerformanceOnThisDayAsync(int month, int day);
    Task<RecentHighlightDto?> GetRecentTopPerformanceAsync(int currentSeasonId, Environment environment);
    Task<ImprovementDto?> GetBiggestImprovementThisSeasonAsync(int currentSeasonId, Environment environment);
    Task<BreakoutAthleteDto?> GetBreakoutAthleteAsync(int currentSeasonId, Environment environment);
    Task<List<SeasonLeaderDto>> GetSeasonLeadersAsync(Gender gender, int currentSeasonId, Environment environment);
    Task<List<UpcomingMeetDto>> GetUpcomingMeetsAsync(int count = 5);
}