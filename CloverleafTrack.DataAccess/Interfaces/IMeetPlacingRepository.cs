using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IMeetPlacingRepository
{
    Task<List<MeetPlacing>> GetForMeetAsync(int meetId);
}
