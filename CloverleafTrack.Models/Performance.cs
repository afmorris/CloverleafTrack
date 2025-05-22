namespace CloverleafTrack.Models;

public class Performance
{
    public int Id { get; set; } 
    public double? DistanceInches { get; set; }
    public double? TimeSeconds { get; set; }
    public string? SortedAthleteHash { get; set; }
    public bool SchoolRecord { get; set; }
    public bool SeasonBest { get; set; }
    public bool PersonalBest { get; set; }
    
    public int? AthleteId { get; set; }
    public int EventId { get; set; }
    public int MeetId { get; set; }

    public Athlete Athlete { get; set; } = new();
    public Event Event { get; set; } = new();
    public Meet Meet { get; set; } = new();
}