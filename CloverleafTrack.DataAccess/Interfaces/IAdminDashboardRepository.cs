namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminDashboardRepository
{
    Task<int> GetTotalAthletesAsync();
    Task<int> GetTotalMeetsAsync();
    Task<int> GetTotalPerformancesAsync();
    Task<int> GetIncompleteMeetsCountAsync();
    Task<List<(string Action, DateTime Timestamp, int? RelatedId)>> GetRecentActivityAsync(int count = 20);
}
