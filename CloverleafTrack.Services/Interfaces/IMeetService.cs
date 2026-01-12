using CloverleafTrack.ViewModels.Meets;

namespace CloverleafTrack.Services.Interfaces;

public interface IMeetService
{
    public Task<MeetsIndexViewModel> GetMeetsIndexAsync();
    public Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string slug);
}