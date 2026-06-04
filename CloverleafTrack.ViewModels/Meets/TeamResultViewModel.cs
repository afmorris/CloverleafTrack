using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Meets;

public class TeamResultViewModel
{
    public MeetType MeetType { get; set; }
    public string? OpponentName { get; set; }
    public int? BoysScore { get; set; }
    public int? BoysOpponentScore { get; set; }
    public int? GirlsScore { get; set; }
    public int? GirlsOpponentScore { get; set; }
    public int? BoysPlace { get; set; }
    public int? GirlsPlace { get; set; }
    public int? FieldSize { get; set; }

    public bool HasResult => BoysScore.HasValue || GirlsScore.HasValue ||
                             BoysPlace.HasValue || GirlsPlace.HasValue;

    private bool IsDualStyle => MeetType == MeetType.Dual || MeetType == MeetType.DoubleDual;

    public bool BoysWon => IsDualStyle && BoysScore.HasValue && BoysScore > BoysOpponentScore;
    public bool GirlsWon => IsDualStyle && GirlsScore.HasValue && GirlsScore > GirlsOpponentScore;

    public string? BoysResultLabel
    {
        get
        {
            if (IsDualStyle && BoysScore.HasValue)
                return $"Boys {(BoysWon ? "W" : "L")} {BoysScore}-{BoysOpponentScore}";
            if (!IsDualStyle && BoysPlace.HasValue)
                return $"Boys {BoysPlace}{Ordinal(BoysPlace.Value)}{(FieldSize.HasValue ? $" of {FieldSize}" : "")}";
            return null;
        }
    }

    public string? GirlsResultLabel
    {
        get
        {
            if (IsDualStyle && GirlsScore.HasValue)
                return $"Girls {(GirlsWon ? "W" : "L")} {GirlsScore}-{GirlsOpponentScore}";
            if (!IsDualStyle && GirlsPlace.HasValue)
                return $"Girls {GirlsPlace}{Ordinal(GirlsPlace.Value)}{(FieldSize.HasValue ? $" of {FieldSize}" : "")}";
            return null;
        }
    }

    private static string Ordinal(int n) => (n % 100) switch
    {
        11 or 12 or 13 => "th",
        _ => (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" }
    };
}
