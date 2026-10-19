namespace CloverleafTrack.ViewModels.Shared;

/// <summary>One round (Prelim / Semifinal / Final) of a running or relay performance. TimeSeconds is raw — views format it.</summary>
public class PerformanceRoundViewModel
{
    public byte Round { get; set; }
    public double TimeSeconds { get; set; }

    /// <summary>True for the round that produced the performance's best (min) time.</summary>
    public bool IsBest { get; set; }

    public string Label => Round switch
    {
        1 => "Prelim",
        2 => "Semi",
        3 => "Final",
        _ => "Round"
    };

    public string ShortLabel => Round switch
    {
        1 => "P",
        2 => "S",
        3 => "F",
        _ => "R"
    };
}

/// <summary>
/// The prelim → semi → final chain for one performance. Attached to display ViewModels as an empty
/// instance by default. <see cref="HasRounds"/> needs at least two rounds: an athlete who ran only
/// one round (e.g. did not advance) shows no chain at all.
/// </summary>
public class PerformanceRoundSeriesViewModel
{
    public List<PerformanceRoundViewModel> Rounds { get; set; } = new();

    public bool HasRounds => Rounds.Count >= 2;
}
