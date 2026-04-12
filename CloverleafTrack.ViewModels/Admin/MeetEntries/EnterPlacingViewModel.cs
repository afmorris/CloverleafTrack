using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.MeetEntries;

public class EnterPlacingViewModel
{
    public int EntryId { get; set; }
    public int MeetId { get; set; }
    public int PerformanceId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string AthleteDisplay { get; set; } = string.Empty;
    public string FormattedResult { get; set; } = string.Empty;
    public bool IsRelay { get; set; }
    public MeetType MeetType { get; set; }

    public List<MeetParticipant> Participants { get; set; } = new();
    public List<PlaceInputRow> PlaceInputs { get; set; } = new();
}
