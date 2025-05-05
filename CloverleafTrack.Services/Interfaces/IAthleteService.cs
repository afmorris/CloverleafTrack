using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services.Interfaces
{
    public interface IAthleteService
    {
        Task<List<AthleteViewModel>> GetActiveAthletesAsync(int currentSeason);
        Task<List<AthleteViewModel>> GetGraduatedAthletesAsync();
        Task<AthleteViewModel?> GetByIdAsync(int id);
        Task<int> CreateAsync(AthleteViewModel athlete);
        Task<bool> UpdateAsync(AthleteViewModel athlete);
        Task<bool> DeleteAsync(int id);
    }
}