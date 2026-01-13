using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminEventRepository
{
    Task<List<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task<int> CreateAsync(Event evt);
    Task<bool> UpdateAsync(Event evt);
    Task<bool> DeleteAsync(int id);
    Task<List<Event>> GetByGenderAndEnvironmentAsync(Gender? gender, Environment environment);
}
