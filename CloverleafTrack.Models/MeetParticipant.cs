using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

public class MeetParticipant
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public int SchoolId { get; set; }
    public Gender? Gender { get; set; }
    public int SortOrder { get; set; }

    public School School { get; set; } = null!;

    // Convenience so existing callers of .SchoolName continue to work
    public string SchoolName => School?.Name ?? string.Empty;
}
