using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminLocationRepository
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task<int> CreateAsync(Location location);
    Task<bool> UpdateAsync(Location location);
    Task<bool> DeleteAsync(int id);
    Task<List<Location>> GetRecentlyUsedAsync(int count = 5);
}
