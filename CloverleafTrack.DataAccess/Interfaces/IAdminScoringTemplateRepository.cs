using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminScoringTemplateRepository
{
    Task<List<ScoringTemplate>> GetAllAsync();
    Task<ScoringTemplate?> GetByIdAsync(int id);
    Task<int> CreateAsync(ScoringTemplate template);
    Task<bool> UpdateAsync(ScoringTemplate template);
    Task<bool> DeleteAsync(int id);
    Task<int> AddPlaceAsync(ScoringTemplatePlace place);
    Task<bool> UpdatePlaceAsync(ScoringTemplatePlace place);
    Task<bool> DeletePlaceAsync(int id);
    Task<bool> DeleteAllPlacesAsync(int templateId);
}
