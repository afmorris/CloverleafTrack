using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ISchoolRepository
{
    Task<List<School>> GetAllAsync();
    Task<School?> GetByIdAsync(int id);
}
