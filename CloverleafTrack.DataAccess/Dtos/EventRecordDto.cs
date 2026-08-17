namespace CloverleafTrack.DataAccess.Dtos;

/// <summary>
/// The current all-time-best value for one event, sourced from Leaderboards.Rank = 1 —
/// never from the stale Performances.SchoolRecord flag. Used by the career chart (issue #26)
/// to draw the record-territory zone regardless of whether the athlete being viewed holds it.
/// </summary>
public class EventRecordDto
{
    public int EventId { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
}
