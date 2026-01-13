using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin;

public class AthleteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    public short Gender { get; set; }

    [Required(ErrorMessage = "Graduation year is required")]
    [Range(1960, 2035, ErrorMessage = "Graduation year must be between 1960 and 2035")]
    public int GraduationYear { get; set; }

    public bool IsActive { get; set; } = true;

    // For duplicate detection
    public List<SimilarAthleteViewModel> SimilarAthletes { get; set; } = new();
}

public class SimilarAthleteViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }
}