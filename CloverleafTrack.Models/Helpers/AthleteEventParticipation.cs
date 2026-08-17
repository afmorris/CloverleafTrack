namespace CloverleafTrack.Models.Helpers;

public class AthleteEventParticipation
{
    public Athlete Athlete { get; set; }
    public Event Event { get; set; }
    public Performance Performance { get; set; }

    /// <summary>Season the performance was set in — used to build the roster's per-season trend sparkline (issue #23). Not the athlete's current class; see ClassYearCalculator for that distinct concept.</summary>
    public DateTime SeasonStartDate { get; set; }
    public string SeasonName { get; set; } = string.Empty;
}