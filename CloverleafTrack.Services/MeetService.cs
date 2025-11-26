using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services;

public class MeetService(IMeetRepository meetRepository) : IMeetService
{
    public async Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string name)
    {
        return null;
    }
}