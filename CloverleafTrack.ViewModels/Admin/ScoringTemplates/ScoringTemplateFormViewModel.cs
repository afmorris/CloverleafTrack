using System.ComponentModel.DataAnnotations;

namespace CloverleafTrack.ViewModels.Admin.ScoringTemplates;

public class ScoringTemplateFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Template Name")]
    public string Name { get; set; } = string.Empty;

    public bool IsBuiltIn { get; set; }

    /// <summary>Ordered list of place → points pairs, edited on the form.</summary>
    public List<ScoringTemplatePlaceFormRow> Places { get; set; } = new();
}

public class ScoringTemplatePlaceFormRow
{
    public int Id { get; set; }
    public int Place { get; set; }

    [Range(0, 9999.99)]
    public decimal Points { get; set; }
}
