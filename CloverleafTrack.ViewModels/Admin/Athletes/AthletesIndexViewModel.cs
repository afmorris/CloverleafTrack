using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Athletes;

public class AthletesIndexViewModel
{
    public List<AthleteListViewModel> Athletes { get; set; } = new();
    
    // Filters
    public string? SearchName { get; set; }
    public Gender? FilterGender { get; set; }
    public bool? FilterIsActive { get; set; }
    public int? FilterGraduationYear { get; set; }
    
    // Stats
    public int TotalAthletes { get; set; }
    public int ActiveAthletes { get; set; }
    public int GraduatedAthletes { get; set; }
}
