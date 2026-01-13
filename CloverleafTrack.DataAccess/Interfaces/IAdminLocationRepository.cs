using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminLocationRepository
{
    Task<List<Location>> GetAllLocationsAsync();
    Task<Location?> GetLocationByIdAsync(int id);
    Task<int> CreateLocationAsync(Location location);
    Task<bool> UpdateLocationAsync(Location location);
    Task<bool> DeleteLocationAsync(int id);
    Task<List<Location>> GetRecentLocationsAsync(int count = 5);
}