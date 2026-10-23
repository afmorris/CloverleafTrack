using CloverleafTrack.Models;

namespace CloverleafTrack.ViewModels.Admin.Performances;

public class PerformancesIndexViewModel
{
    public List<Performance> Performances { get; set; } = new();

    /// <summary>True when more rows matched than <see cref="Limit"/>; the list was cut off.</summary>
    public bool Truncated { get; set; }
    public int Limit { get; set; }

    public int? SeasonId { get; set; }
    public int? MeetId { get; set; }
    public string? Search { get; set; }

    public List<(int Id, string Name)> Seasons { get; set; } = new();
    public List<(int Id, string Name, DateTime Date)> Meets { get; set; } = new();
}
