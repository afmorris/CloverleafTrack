namespace CloverleafTrack.Models;

public class MeetParticipant
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
