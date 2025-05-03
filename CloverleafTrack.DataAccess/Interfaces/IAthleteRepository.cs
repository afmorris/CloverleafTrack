using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAthleteRepository
{
    Task<List<Athlete>> GetActiveAthletesAsync(int currentYear);
    Task<List<Athlete>> GetGraduatedAthletesAsync(int currentYear);
    
    Task<Athlete?> GetByIdAsync(int id);
    Task<int> CreateAsync(Athlete athlete);
    Task<bool> UpdateAsync(Athlete athlete);
    Task<bool> DeleteAsync(Athlete athlete);
}