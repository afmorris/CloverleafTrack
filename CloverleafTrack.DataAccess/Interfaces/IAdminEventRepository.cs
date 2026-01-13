using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminEventRepository
{
    Task<List<Event>> GetAllEventsAsync();
    Task<List<Event>> GetEventsByEnvironmentAndGenderAsync(Environment environment, Gender? gender);
    Task<Event?> GetEventByIdAsync(int id);
    Task<int> CreateEventAsync(Event eventItem);
    Task<bool> UpdateEventAsync(Event eventItem);
    Task<bool> DeleteEventAsync(int id);
}