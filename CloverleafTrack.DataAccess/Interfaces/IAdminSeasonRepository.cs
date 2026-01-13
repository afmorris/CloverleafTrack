using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminSeasonRepository
{
    Task<List<Season>> GetAllAsync();
    Task<Season?> GetByIdAsync(int id);
    Task<Season?> GetCurrentSeasonAsync();
    Task<int> CreateAsync(Season season);
    Task<bool> UpdateAsync(Season season);
    Task<bool> DeleteAsync(int id);
    Task<Season?> GetSeasonForDateAsync(DateTime date);
}
