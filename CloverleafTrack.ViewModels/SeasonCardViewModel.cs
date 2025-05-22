using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels;

public class SeasonCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public SeasonStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public int TotalMeets { get; set; }
    public int MeetsEntered { get; set; }
    
    public List<MeetSummaryViewModel> Meets { get; set; } = new();
    
    public bool HasSchoolRecords { get; set; }
    public List<string>? SchoolRecords { get; set; }
    
    public string StatusBadge => Status.ToString();
}