namespace CloverleafTrack.Models;

public class MeetEntryAthlete
{
    public int Id { get; set; }
    public int MeetEntryId { get; set; }
    public int AthleteId { get; set; }

    public Athlete Athlete { get; set; } = new();
}
