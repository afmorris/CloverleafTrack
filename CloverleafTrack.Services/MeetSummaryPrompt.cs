using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Meets;

namespace CloverleafTrack.Services;

/// <summary>
/// Builds the model input for a meet recap. The model is only ever shown this fact sheet, never raw database rows,
/// so the flags it sees are the already-corrected ones (school record = AllTimeRank == 1; relays never get a PR flag,
/// because relay PersonalBest is unreliable — see CLAUDE.md "performance flags").
/// </summary>
public static class MeetSummaryPrompt
{
    /// <summary>Bump when the system prompt or fact-sheet shape changes so every summary is treated as stale.</summary>
    public const string Version = "2";

    public const string SystemPrompt = """
        You write short recaps of high school track & field meets for the Cloverleaf track & field results website.
        You are given a JSON fact sheet for ONE meet. Write the recap from that fact sheet only.

        Rules:
        - Use only facts present in the JSON. Never invent names, marks, places, scores, records, weather, or history.
        - Do not do arithmetic beyond simple counting of items you can see. Do not compare to other meets or seasons.
        - Only name athletes who appear in the JSON. Only describe an athlete as setting a personal best if "pr" is true,
          a school record if "sr" is true, and a top-10 all-time mark if "allTimeRank" is 10 or less.
        - Relay entries never carry a "pr" flag; do not claim a relay personal best.
        - "meetScore" is a 0-100 rating of the meet's overall performance level. Use it only as a loose hint for tone
          (75 or higher can be called "a strong day"). Never quote it, and never rank this meet against other meets.
        - Report team results exactly as given (scores, places, opponent names). If there are none, say nothing about team results.
        - Lead with what mattered most: school records, then team result, then standout wins and personal bests.
        - Name every event exactly as its "event" value appears in the fact sheet, with the same unit letter and capitalization
          (for example "800M", "100M Hurdles", "4x400M Relay", "Long Jump", "Pole Vault", "800M Sprint Medley"). Never write
          "800 meters", "the 800", "100 hurdles" or "4x400 relay". The website turns these names into links.
        - When a sentence mentions an event that exists for both boys and girls, put "boys" or "girls" directly before the first
          event name in that clause (for example "the girls 100M and 200M").
        - Plain, warm, factual tone like a good local sports page. Past tense. No emojis, no markdown, no headings, no bullet lists.
        - Write 70 to 120 words as a single paragraph. Output only the paragraph.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string BuildFactSheet(MeetDetailsViewModel meet, MeetLevel level)
    {
        var sheet = new Dictionary<string, object?>
        {
            ["meet"] = meet.Name,
            ["date"] = meet.Date.ToString("MMMM d, yyyy"),
            ["location"] = string.Join(", ", new[] { meet.LocationName, meet.LocationCity, meet.LocationState }
                .Where(s => !string.IsNullOrWhiteSpace(s))),
            ["season"] = meet.SeasonName,
            ["environment"] = meet.Environment.ToString(),
            ["level"] = level == MeetLevel.JV ? "JV" : "Varsity",
            ["format"] = DescribeFormat(meet),
            ["otherSchools"] = meet.Participants.Select(p => p.SchoolName).Where(n => n.Length > 0).ToList(),
            ["handTimed"] = meet.HandTimed ? true : null,
            ["teamResults"] = meet.TeamResult?.Entries.Select(e => new Dictionary<string, object?>
            {
                ["team"] = e.GenderLabel,
                ["label"] = e.Label,
                ["ourScore"] = e.OurScore,
                ["opponentScore"] = e.OpponentScore,
                ["opponent"] = e.OpponentName,
                ["place"] = e.Place,
                ["fieldSize"] = e.FieldSize
            }).ToList(),
            ["meetScore"] = meet.MeetScore is { } s ? Math.Round(s.Score) : null,
            ["totals"] = new Dictionary<string, object?>
            {
                ["athletes"] = meet.UniqueAthletes,
                ["performances"] = meet.TotalPerformances,
                ["schoolRecords"] = meet.TotalSchoolRecords,
                ["top10AllTimeMarks"] = meet.TopTenAllTimeCount
            },
            ["boys"] = BuildEvents(meet.BoysEvents),
            ["girls"] = BuildEvents(meet.GirlsEvents),
            ["mixed"] = meet.MixedEvents.Count > 0 ? BuildEvents(meet.MixedEvents) : null
        };

        // DefaultIgnoreCondition does not apply to dictionary entries, so strip nulls explicitly (saves tokens, avoids "pr":null noise).
        var node = JsonSerializer.SerializeToNode(sheet, JsonOptions)!;
        StripNulls(node);
        return node.ToJsonString(JsonOptions);
    }

    private static void StripNulls(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Where(kv => kv.Value is null).Select(kv => kv.Key).ToList()) obj.Remove(key);
                foreach (var value in obj.Select(kv => kv.Value)) StripNulls(value);
                break;
            case JsonArray arr:
                foreach (var item in arr) StripNulls(item);
                break;
        }
    }

    /// <summary>MeetType defaults to Dual, so a dual with no opponent and no team result is really an open/qualifier meet.</summary>
    private static string DescribeFormat(MeetDetailsViewModel meet) => meet.MeetType switch
    {
        MeetType.DoubleDual => "double dual (three teams, scored as two separate dual meets)",
        MeetType.Invitational => "invitational",
        _ when meet.Participants.Count == 0 && meet.TeamResult is null => "open meet / qualifier with no team scoring recorded",
        _ => "dual meet"
    };

    public static string BuildUserPrompt(string factSheet) =>
        "Fact sheet:\n" + factSheet + "\n\nWrite the recap.";

    public static string ComputeHash(string factSheet)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(Version + "\n" + SystemPrompt + "\n" + factSheet));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static List<Dictionary<string, object?>> BuildEvents(List<MeetEventGroupViewModel> groups) =>
        groups.Select(g => new Dictionary<string, object?>
        {
            ["event"] = g.EventName,
            ["results"] = g.Performances.Select(p =>
            {
                var isRelay = p.AthleteSlug == null; // individual performances always have a slug; relay rows don't
                return new Dictionary<string, object?>
                {
                    [isRelay ? "relayTeam" : "athlete"] = isRelay ? p.AthleteName.Split("|~|") : p.AthleteName,
                    ["mark"] = p.Performance,
                    ["pr"] = !isRelay && p.IsPersonalBest ? true : null,
                    ["sr"] = p.IsSchoolRecord ? true : null,
                    ["seasonBest"] = p.IsSeasonBest ? true : null,
                    ["allTimeRank"] = p.AllTimeRank is <= 10 ? p.AllTimeRank : null,
                    ["places"] = p.HasPlacing
                        ? p.Placings.Select(pl => pl.OpponentSchoolName != null
                            ? $"{pl.PlaceLabel} vs. {pl.OpponentSchoolName}"
                            : pl.PlaceLabel).ToList()
                        : null
                };
            }).ToList()
        }).ToList();
}
