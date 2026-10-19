namespace CloverleafTrack.ViewModels.Shared;

/// <summary>One bar height for display. HeightInches is raw — views format it with PerformanceFormatHelper.FormatDistance.</summary>
public class PerformanceBarViewModel
{
    public double HeightInches { get; set; }

    /// <summary>Attempts at this height in order: X miss, O clear, P pass.</summary>
    public string Outcome { get; set; } = string.Empty;

    public bool IsCleared => Outcome.EndsWith('O');
    public bool IsPass => Outcome.EndsWith('P');
    public bool IsOut => Outcome == "XXX";

    /// <summary>True for the highest cleared bar (the performance's mark).</summary>
    public bool IsBest { get; set; }
}

/// <summary>The bar ladder for one high jump / pole vault performance. Empty (HasBars == false) unless bar rows exist.</summary>
public class PerformanceBarSeriesViewModel
{
    public List<PerformanceBarViewModel> Bars { get; set; } = new();

    public bool HasBars => Bars.Count > 0;
}
