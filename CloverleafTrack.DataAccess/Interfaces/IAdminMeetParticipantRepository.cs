using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetParticipantRepository
{
    Task<List<MeetParticipant>> GetForMeetAsync(int meetId);
    Task<MeetParticipant?> GetByIdAsync(int id);
    Task<int> CreateAsync(MeetParticipant participant);
    Task<bool> UpdateAsync(MeetParticipant participant);
    Task<bool> DeleteAsync(int id);
}
