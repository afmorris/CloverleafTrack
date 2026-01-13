using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminAthleteRepository
{
    Task<List<Athlete>> GetAllAsync();
    Task<List<Athlete>> GetFilteredAsync(string? searchName, Gender? gender, bool? isActive, int? graduationYear);
    Task<Athlete?> GetByIdAsync(int id);
    Task<int> CreateAsync(Athlete athlete);
    Task<bool> UpdateAsync(Athlete athlete);
    Task<bool> DeleteAsync(int id);
    Task<List<Athlete>> GetSimilarAthletesAsync(string firstName, string lastName);
    Task<int> GetPerformanceCountAsync(int athleteId);
    Task<List<Athlete>> GetAthletesForMeetAsync(int meetId, Gender? gender);
}
