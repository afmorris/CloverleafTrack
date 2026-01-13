using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminSeasonRepository
{
    Task<List<Season>> GetAllSeasonsAsync();
    Task<Season?> GetSeasonByIdAsync(int id);
    Task<int> CreateSeasonAsync(Season season);
    Task<bool> UpdateSeasonAsync(Season season);
    Task<bool> DeleteSeasonAsync(int id);
    Task<Season?> GetCurrentSeasonAsync();
    Task<Season?> DetectSeasonFromDateAsync(DateTime date);
}