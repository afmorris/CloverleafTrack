namespace CloverleafTrack.Models.Enums;

/// <summary>Which round of a running/relay event a <see cref="PerformanceRound"/> mark came from.</summary>
public enum RoundType : byte
{
    Prelim = 1,
    Semifinal = 2,
    Final = 3
}
