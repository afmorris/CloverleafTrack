using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IPerformanceRepository
{
    public Task<int> CountPRsForSeasonAsync(int seasonId);
    public Task<int> CountAthletesWithPRsForSeasonAsync(int seasonId);
    public Task<int> CountSchoolRecordsBrokenForSeasonAsync(int seasonId);
    public Task<List<TopPerformanceDto>> GetTopPerformancesForSeasonAsync(int seasonId);
}