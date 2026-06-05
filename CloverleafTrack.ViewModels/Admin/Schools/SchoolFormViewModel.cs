using System.ComponentModel.DataAnnotations;

namespace CloverleafTrack.ViewModels.Admin.Schools;

public class SchoolFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "School Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Short Name / Abbreviation")]
    public string? ShortName { get; set; }
}
