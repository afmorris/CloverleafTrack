using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminSchoolRepository
{
    Task<List<School>> GetAllAsync();
    Task<School?> GetByIdAsync(int id);
    Task<int> CreateAsync(School school);
    Task<bool> UpdateAsync(School school);
    Task<bool> DeleteAsync(int id);
}
