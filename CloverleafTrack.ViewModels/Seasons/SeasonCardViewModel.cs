using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Seasons;

public class SeasonCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public SeasonStatus Status { get; set; }
    public string? Notes { get; set; }

    public int TotalMeets { get; set; }
    public int MeetsEntered { get; set; }
    public int TotalPRs { get; set; }  // NEW - for stats cards
    public bool IsCurrentSeason { get; set; }  // NEW - to highlight current season

    public List<MeetSummaryViewModel> Meets { get; set; } = new();

    public bool HasSchoolRecords { get; set; }
    public List<SchoolRecordViewModel>? IndoorSchoolRecords { get; set; }
    public List<SchoolRecordViewModel>? OutdoorSchoolRecords { get; set; }

    public string StatusBadge => Status.ToString();

    // Computed properties for display
    public int IndoorSchoolRecordCount => IndoorSchoolRecords?.Count ?? 0;
    public int OutdoorSchoolRecordCount => OutdoorSchoolRecords?.Count ?? 0;
}