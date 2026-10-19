namespace CloverleafTrack.Models.Enums;

/// <summary>
/// Outcome of a performance row. <see cref="Mark"/> is the normal case (a time or distance exists). The others are
/// "competed but no mark" results: both TimeSeconds and DistanceInches are NULL, and the row is excluded from
/// rankings, PRs, percentiles and scoring but still counts as an event the athlete competed in.
/// </summary>
public enum ResultStatus : byte
{
    Mark = 0,
    NoMark = 1,          // NM: fouled every attempt in a throw / horizontal jump
    NoHeight = 2,        // NH: cleared no height in the high jump / pole vault
    DidNotFinish = 3,    // DNF
    Disqualified = 4     // DQ
}

public static class ResultStatusExtensions
{
    public static string Abbreviation(this ResultStatus status) => status switch
    {
        ResultStatus.NoMark => "NM",
        ResultStatus.NoHeight => "NH",
        ResultStatus.DidNotFinish => "DNF",
        ResultStatus.Disqualified => "DQ",
        _ => ""
    };

    public static string Description(this ResultStatus status) => status switch
    {
        ResultStatus.NoMark => "No mark (fouled every attempt)",
        ResultStatus.NoHeight => "No height (cleared no bar)",
        ResultStatus.DidNotFinish => "Did not finish",
        ResultStatus.Disqualified => "Disqualified",
        _ => ""
    };

    /// <summary>The long phrase used in AI fact sheets and tooltips.</summary>
    public static string Phrase(this ResultStatus status) => status switch
    {
        ResultStatus.NoMark => "no mark (fouled out)",
        ResultStatus.NoHeight => "no height",
        ResultStatus.DidNotFinish => "did not finish",
        ResultStatus.Disqualified => "disqualified",
        _ => ""
    };
}
