namespace CloverleafTrack.ViewModels.Scoring;

public class SeasonScoringViewModel
{
    public int SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public string SeasonSlug { get; set; } = string.Empty;

    /// <summary>Boys athletes sorted by TotalFullPoints descending.</summary>
    public List<AthleteScoreRowViewModel> Boys { get; set; } = new();

    /// <summary>Girls athletes sorted by TotalFullPoints descending.</summary>
    public List<AthleteScoreRowViewModel> Girls { get; set; } = new();
}
