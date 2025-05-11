using CloverleafTrack.Models;
using CloverleafTrack.Models.Helpers;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAthleteRepository
{
    Task<List<Athlete>> GetAllAsync();
    Task<Athlete?> GetByIdAsync(int id);
    Task<List<AthleteEventParticipation>> GetAllWithPerformancesAsync();
    Task<int> CreateAsync(Athlete athlete);
    Task<bool> UpdateAsync(Athlete athlete);
    Task<bool> DeleteAsync(Athlete athlete);
}