using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IScoringTemplateRepository
{
    Task<List<ScoringTemplate>> GetAllAsync();
    Task<ScoringTemplate?> GetByIdAsync(int id);
}
