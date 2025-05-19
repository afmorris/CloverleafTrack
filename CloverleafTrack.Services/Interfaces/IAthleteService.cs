using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services.Interfaces
{
    public interface IAthleteService
    {
        Task<List<AthleteViewModel>> GetActiveAthletesAsync(int currentSeason);
        Task<List<AthleteViewModel>> GetGraduatedAthletesAsync(int currentSeason);
        Task<AthleteViewModel?> GetByIdAsync(int id);
        Task<Dictionary<EventCategory, List<AthleteViewModel>>> GetActiveAthletesGroupedByEventCategoryAsync(int currentSeason);
        Task<Dictionary<int, List<AthleteViewModel>>> GetFormerAthletesGroupedByGraduationYearAsync();
        Task<int> CreateAsync(AthleteViewModel athlete);
        Task<bool> UpdateAsync(AthleteViewModel athlete);
        Task<bool> DeleteAsync(int id);
    }
}