using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ISeasonRepository
{
    Task<List<Season>> GetAllAsync();
    Task<Season?> GetByIdAsync(int id);
    Task<Season?> GetByNameAsync(string name);
    Task<int> CreateAsync(Season season);
    Task<bool> UpdateAsync(Season season);
    Task<bool> DeleteAsync(Season season);
    Task<List<Season>> GetSeasonsWithMeetsAsync();
}