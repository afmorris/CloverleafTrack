using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

public class MeetTeamResult
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public Gender Gender { get; set; }
    public int? OpponentMeetParticipantId { get; set; }
    public decimal? OurScore { get; set; }
    public decimal? OpponentScore { get; set; }
    public int? Place { get; set; }
    public int? FieldSize { get; set; }

    public MeetParticipant? OpponentMeetParticipant { get; set; }

    public bool Won => OurScore.HasValue && OpponentScore.HasValue && OurScore > OpponentScore;
    public string? OpponentName => OpponentMeetParticipant?.SchoolName;
}
