using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin;

public class AthleteListViewModel
{
    public List<AthleteListItemViewModel> Athletes { get; set; } = new();
    public int TotalAthletes { get; set; }
    public int ActiveAthletes { get; set; }
    public int GraduatedAthletes { get; set; }

    // Filter state
    public string? SearchTerm { get; set; }
    public short? GenderFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public int? GraduationYearFilter { get; set; }
}

public class AthleteListItemViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public short Gender { get; set; }
    public int GraduationYear { get; set; }
    public bool IsActive { get; set; }
    public int PerformanceCount { get; set; }
}