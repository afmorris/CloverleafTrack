using CloverleafTrack.ViewModels.Meets;

namespace CloverleafTrack.Services.Interfaces;

public interface IMeetService
{
    public Task<MeetsIndexViewModel> GetMeetsIndexAsync();
    public Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string slug);

    /// <summary>MeetScore for every meet, keyed by meet id (always computed for all meets together; see MeetScoreCalculator).</summary>
    public Task<Dictionary<int, MeetScoreViewModel>> GetMeetScoresAsync();
}