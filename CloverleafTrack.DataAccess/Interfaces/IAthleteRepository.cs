using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAthleteRepository
{
    Task<List<Athlete>> GetAllAsync();
    Task<Athlete?> GetByIdAsync(int id);
    Task<List<Athlete>> GetAllWithPerformancesAsync();
    Task<int> CreateAsync(Athlete athlete);
    Task<bool> UpdateAsync(Athlete athlete);
    Task<bool> DeleteAsync(Athlete athlete);
}