using CloverleafTrack.ViewModels.Meets;

namespace CloverleafTrack.Services.Interfaces;

public interface IMeetService
{
    public Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string name);
}