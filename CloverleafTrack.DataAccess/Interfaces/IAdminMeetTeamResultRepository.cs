using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetTeamResultRepository
{
    Task<List<MeetTeamResult>> GetForMeetAsync(int meetId);
    Task<int> CreateAsync(MeetTeamResult result);
    Task<bool> UpdateAsync(MeetTeamResult result);
    Task<bool> DeleteAsync(int id);
    Task DeleteAllForMeetAsync(int meetId);
}
