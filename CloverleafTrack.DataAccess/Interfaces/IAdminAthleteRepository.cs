using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminAthleteRepository
{
    Task<List<Athlete>> GetAllAthletesAsync();
    Task<List<Athlete>> GetAthletesByFiltersAsync(string? searchTerm, short? gender, bool? isActive, int? graduationYear);
    Task<List<Athlete>> GetAthletesEligibleForMeetAsync(DateTime meetDate, short? eventGender);
    Task<Athlete?> GetAthleteByIdAsync(int id);
    Task<int> CreateAthleteAsync(Athlete athlete);
    Task<bool> UpdateAthleteAsync(Athlete athlete);
    Task<bool> DeleteAthleteAsync(int id);
    Task<List<Athlete>> FindSimilarAthletesAsync(string firstName, string lastName);
    Task<int> GetPerformanceCountForAthleteAsync(int athleteId);
}