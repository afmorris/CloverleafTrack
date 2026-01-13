using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Admin;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Admin;

public class MeetsIndexViewModel
{
    public List<MeetSeasonGroupViewModel> SeasonGroups { get; set; } = new();

    // Filters
    public string? SearchName { get; set; }
    public int? FilterSeasonId { get; set; }
    public Environment? FilterEnvironment { get; set; }
    public MeetEntryStatus? FilterEntryStatus { get; set; }

    // For filter dropdowns
    public List<SeasonOptionViewModel> Seasons { get; set; } = new();
}