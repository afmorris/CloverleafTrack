using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

/// <summary>
/// One recorded performance, flattened with the meet/season/percentile context MeetScore needs.
/// Not a table — populated by <c>IMeetRepository.GetMeetScoreMarksAsync</c>.
/// </summary>
public class MeetScoreMark
{
    public int PerformanceId { get; set; }
    public int MeetId { get; set; }
    public DateTime MeetDate { get; set; }
    public MeetLevel MeetLevel { get; set; }
    public DateTime SeasonStartDate { get; set; }
    public int EventId { get; set; }

    /// <summary>Null for relay performances.</summary>
    public int? AthleteId { get; set; }

    /// <summary>True when higher is better (distance); false when lower is better (time).</summary>
    public bool IsFieldEvent { get; set; }

    /// <summary>DistanceInches for field events, TimeSeconds for running events. Null if neither was recorded.</summary>
    public double? Value { get; set; }

    /// <summary>From PerformancePercentiles; null when the performance has no percentile row.</summary>
    public byte? Percentile { get; set; }

    /// <summary>From EventStatistics; 0 when the event has no statistics row.</summary>
    public int EventMarkCount { get; set; }
}
