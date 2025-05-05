using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels;

public class AthleteViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }
}