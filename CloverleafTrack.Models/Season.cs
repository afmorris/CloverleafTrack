using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

public class Season
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrentSeason { get; set; }
    public string? Notes { get; set; }
    public SeasonStatus Status { get; set; }
    public bool ScoringEnabled { get; set; }
    public List<Meet> Meets { get; set; } = new();
}