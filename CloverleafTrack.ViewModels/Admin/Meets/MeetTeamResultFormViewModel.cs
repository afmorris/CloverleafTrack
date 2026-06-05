using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Meets;

public class MeetTeamResultFormViewModel
{
    public int Id { get; set; }
    public Gender Gender { get; set; }
    public int? OpponentMeetParticipantId { get; set; }
    public string? OpponentName { get; set; }
    public decimal? OurScore { get; set; }
    public decimal? OpponentScore { get; set; }
    public int? Place { get; set; }
    public int? FieldSize { get; set; }
    public bool IsInvitational { get; set; }
}
