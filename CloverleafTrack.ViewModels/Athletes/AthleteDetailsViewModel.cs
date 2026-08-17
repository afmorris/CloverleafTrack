using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Athletes;

public class AthleteDetailsViewModel
{
    public int AthleteId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }
    public string Class { get; set; } = string.Empty;
    
    // Hero Stats
    public AthleteTopEventViewModel? TopSprintEvent { get; set; }
    public AthleteTopEventViewModel? TopFieldEvent { get; set; }
    public int TotalPRs { get; set; }
    public int TotalSchoolRecords { get; set; }
    
    // Personal Records
    public List<PersonalRecordViewModel> PersonalRecords { get; set; } = new();
    
    // Season Performance
    public List<SeasonPerformanceViewModel> Seasons { get; set; } = new();

    // Career Progression (issue #26) — one chart per event the athlete has performances in.
    // Never hidden by default; this is the page's "payoff artifact."
    public List<CareerChartViewModel> CareerCharts { get; set; } = new();
}