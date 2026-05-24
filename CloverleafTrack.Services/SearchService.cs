using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Services;

public class SearchService(
    IAthleteService athleteService,
    IMeetService meetService,
    ILeaderboardService leaderboardService,
    ISeasonService seasonService) : ISearchService
{
    public async Task<List<SearchRecord>> GetSearchIndexAsync()
    {
        var records = new List<SearchRecord>();
        var currentSeason = await seasonService.GetCurrentSeasonAsync();

        var activeAthletes = await athleteService.GetActiveAthletesAsync(currentSeason);
        var formerAthletes = await athleteService.GetGraduatedAthletesAsync(currentSeason);

        foreach (var athlete in activeAthletes.Concat(formerAthletes))
        {
            var genderText = athlete.Gender == Gender.Male ? "Boys" : "Girls";
            records.Add(new SearchRecord(
                "athlete",
                athlete.FullName,
                $"{athlete.Class} · {genderText}",
                $"/roster/{athlete.AthleteSlug}"));
        }

        var meetsIndex = await meetService.GetMeetsIndexAsync();
        foreach (var season in meetsIndex.Seasons)
        {
            foreach (var meet in season.Meets)
            {
                var sublabel = meet.Date.ToString("MMM d, yyyy");
                if (!string.IsNullOrEmpty(meet.LocationCity))
                    sublabel += $" · {meet.LocationCity}, {meet.LocationState}";
                else if (!string.IsNullOrEmpty(meet.LocationName))
                    sublabel += $" · {meet.LocationName}";

                records.Add(new SearchRecord("meet", meet.Name, sublabel, $"/meets/{meet.Slug}"));
            }
        }

        var leaderboard = await leaderboardService.GetLeaderboardAsync();
        var allSections = new[]
        {
            (leaderboard.BoysOutdoorCategories, "Boys", "Outdoor"),
            (leaderboard.BoysIndoorCategories, "Boys", "Indoor"),
            (leaderboard.GirlsOutdoorCategories, "Girls", "Outdoor"),
            (leaderboard.GirlsIndoorCategories, "Girls", "Indoor"),
            (leaderboard.MixedOutdoorCategories, "Mixed", "Outdoor"),
            (leaderboard.MixedIndoorCategories, "Mixed", "Indoor"),
        };

        foreach (var (categories, gender, env) in allSections)
        {
            foreach (var category in categories)
            {
                foreach (var evt in category.Events.Where(e => !string.IsNullOrEmpty(e.Performance)))
                {
                    var holder = string.IsNullOrWhiteSpace(evt.AthleteFullName?.Trim())
                        ? evt.Performance
                        : $"{evt.AthleteFullName} · {evt.Performance}";

                    records.Add(new SearchRecord(
                        "event",
                        $"{gender} {evt.EventName} ({env})",
                        holder,
                        $"/leaderboard/{evt.EventKey.ToLower()}"));
                }
            }
        }

        return records;
    }
}
