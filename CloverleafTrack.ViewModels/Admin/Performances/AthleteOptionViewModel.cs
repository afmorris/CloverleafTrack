using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Performances;

public class AthleteOptionViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int GraduationYear { get; set; }
    
    public string DisplayText => $"{LastName}, {FirstName} ({GraduationYear})";
}
