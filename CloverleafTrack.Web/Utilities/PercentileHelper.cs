namespace CloverleafTrack.Web.Utilities;

/// <summary>
/// Single source of truth for the diverging percentile color scale (anchored at the program
/// median, blue below / red above) used by the Personal Bests percentile column (issue #19)
/// and, later, the mark-cell tint on event pages and /search (issue #21). Bucket boundaries
/// and hex values must stay in sync with both issues' specs.
/// </summary>
public static class PercentileHelper
{
    private static readonly (int Min, int Max, string Fill, string Ink, string Text, string Label)[] Buckets =
    {
        (1,  14,  "#5aa3ea", "#d8eafc", "#8fc4f5", "far below median"),
        (15, 34,  "#3179c9", "#d3e6fa", "#6fa9e8", "below median"),
        (35, 47,  "#2c5f9e", "#d0e3f8", "#5f96d4", "slightly below median"),
        (48, 52,  "#4a4f5e", "#dfe3ea", "#a2aabb", "at median"),
        (53, 65,  "#a04a42", "#fadfda", "#d98d84", "slightly above median"),
        (66, 84,  "#cf594e", "#fce0da", "#e8897d", "above median"),
        (85, 100, "#ef8272", "#fde3dc", "#f09a8b", "far above median"),
    };

    private static (int Min, int Max, string Fill, string Ink, string Text, string Label) Bucket(int percentile)
    {
        var clamped = Math.Clamp(percentile, 1, 100);
        return Buckets.First(b => clamped >= b.Min && clamped <= b.Max);
    }

    /// <summary>Numeral color — clears 5.4:1 against the card surface. Used by the percentile column.</summary>
    public static string GetTextColor(int percentile) => Bucket(percentile).Text;

    /// <summary>Cell tint fill color (22% alpha applied by the caller) — for future mark-cell tint (issue #21).</summary>
    public static string GetFillColor(int percentile) => Bucket(percentile).Fill;

    /// <summary>Mark text color on a tinted cell — clears 9.4:1 (issue #21).</summary>
    public static string GetInkColor(int percentile) => Bucket(percentile).Ink;

    public static string GetBucketLabel(int percentile) => Bucket(percentile).Label;

    public static string OrdinalSuffix(int n)
    {
        var clamped = Math.Abs(n);
        if (clamped % 100 is >= 11 and <= 13) return "th";
        return (clamped % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    /// <summary>Plain-English reading for the title attribute and screen-reader span, e.g. "97th percentile of program history — far above median".</summary>
    public static string GetReading(int percentile)
    {
        var clamped = Math.Clamp(percentile, 1, 100);
        return $"{clamped}{OrdinalSuffix(clamped)} percentile of program history — {GetBucketLabel(clamped)}";
    }
}
