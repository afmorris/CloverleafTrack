using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Repositories;

public class AthleteRepository : IAthleteRepository
{
    public Task<List<Athlete>> GetActiveAthletesAsync(int currentYear)
    {
        throw new NotImplementedException();
    }

    public Task<List<Athlete>> GetGraduatedAthletesAsync(int currentYear)
    {
        throw new NotImplementedException();
    }

    public Task<Athlete?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateAsync(Athlete athlete)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Athlete athlete)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Athlete athlete)
    {
        throw new NotImplementedException();
    }
}