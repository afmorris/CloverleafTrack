using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Athletes;

public class AthleteListViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int GraduationYear { get; set; }
    public bool IsActive { get; set; }
    public int PerformanceCount { get; set; }
}
