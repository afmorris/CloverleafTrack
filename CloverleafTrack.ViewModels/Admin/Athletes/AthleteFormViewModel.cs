using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Athletes;

public class AthleteFormViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Gender")]
    public Gender Gender { get; set; }
    
    [Required]
    [Range(1980, 2035)]
    [Display(Name = "Graduation Year")]
    public int GraduationYear { get; set; }
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
    
    public List<AthleteListViewModel> SimilarAthletes { get; set; } = new();
}
