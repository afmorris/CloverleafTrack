using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Services.Interfaces;

public interface IMeetService
{
    public Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string name);
}