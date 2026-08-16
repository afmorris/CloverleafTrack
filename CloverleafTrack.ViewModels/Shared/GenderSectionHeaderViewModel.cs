using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Shared;

/// <summary>
/// Model for the shared, triple-encoded (glyph + word + color) gender section header partial
/// (issue #28). Scoped to Male/Female — individual athletes are never Gender.Mixed, unlike
/// relay events, so this deliberately does not need a Mixed case.
/// </summary>
public class GenderSectionHeaderViewModel
{
    public Gender Gender { get; set; }
    public int Count { get; set; }

    /// <summary>"lg" (default, page-level sections like the active roster) or "sm" (nested contexts, e.g. inside a per-class-year &lt;details&gt; on the former athletes list).</summary>
    public string Size { get; set; } = "lg";
}
