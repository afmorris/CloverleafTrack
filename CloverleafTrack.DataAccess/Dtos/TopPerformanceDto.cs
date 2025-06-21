namespace CloverleafTrack.DataAccess.Dtos;

public class TopPerformanceDto
{
    public int AllTimeRank { get; set; }
    public string EventName { get; set; } = "";
    public string AthleteName { get; set; } = "";
    public double? DistanceInches { get; set; }
    public double? TimeSeconds { get; set; }
    public string MeetName { get; set; } = "";
    public DateTime MeetDate { get; set; }

    // Optional helper:
    public string FormattedPerformance =>
        DistanceInches.HasValue
            ? $"{DistanceInches.Value:F2}\""
            : $"{TimeSeconds.Value:F2} s";
}