using CloverleafTrack.ViewModels.Performances;

namespace CloverleafTrack.ViewModels.Seasons;

public class SeasonDetailsViewModel
{
    public string Name { get; set; } = string.Empty;
    public int TotalPRs { get; set; }
    public int TotalAthletesWithPRs { get; set; }
    public int TotalSchoolRecordsBroken { get; set; }
    public int TotalMeets { get; set; }

    public List<TopPerformanceViewModel> IndoorTopPerformances { get; set; } = new();
    public List<TopPerformanceViewModel> OutdoorTopPerformances { get; set; } = new();
    public List<SeasonMeetViewModel> Meets { get; set; } = new();
}