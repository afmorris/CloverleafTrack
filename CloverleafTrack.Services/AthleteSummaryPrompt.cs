using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Athletes;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

/// <summary>
/// Builds the model input for an athlete career recap from the same view model the athlete page renders, so the
/// corrected rules apply (school record = all-time rank 1, never the stale SchoolRecord flag; rank only if top 10).
/// PersonalBest flags (and so TotalPRs / PRCount) are only populated for recent seasons, so PR counts are deliberately omitted.
/// </summary>
public static class AthleteSummaryPrompt
{
    /// <summary>Bump when the system prompt or fact-sheet shape changes so every summary is treated as stale.</summary>
    public const string Version = "1";

    public const string SystemPrompt = """
        You write short career recaps of high school track & field athletes for the Cloverleaf track & field results website.
        You are given a JSON fact sheet for ONE athlete. Write the recap from that fact sheet only.

        Rules:
        - Use only facts present in the JSON. Never invent marks, places, meets, dates, records, rankings, teammates, or history.
        - Write only about athletic results. Do not mention age, family, health, grades, personality, college plans, or the future,
          and do not praise character.
        - Do not use gendered pronouns. Use the athlete's name, or "they" sparingly.
        - Call a mark a school record only if its "sr" is true, and a top-10 mark only if "allTimeRank" is present on it.
          Never state a rank that is not in the JSON.
        - Use "totals.schoolRecords" for the career count of school records. Never state how many personal bests the athlete
          set (those counts are not recorded for older seasons). Do not count or add anything yourself.
        - Describe progress only with marks in the JSON: "first" and "best" in "careerArc", "seasonBest" in "seasons", and the
          provided "improvement". Use each season's "class" (Freshman, Sophomore, Junior, Senior); do not estimate or do arithmetic.
        - Relay entries list "relay" teammates. You may name teammates who appear in the JSON. Never claim a relay personal best.
        - Name every event exactly as its "event" value appears in the fact sheet ("800M", "100M Hurdles", "4x400M Relay",
          "Shot Put"), and put "indoor" or "outdoor" directly before it when the athlete has that event in both environments
          (for example "outdoor 1600M"). Name meets exactly as written in the JSON. The website turns these names into links.
        - Lead with school records and top-10 marks, then the arc of the career, then the athlete's signature events.
        - Plain, warm, factual tone like a good local sports page. Past tense. No emojis, no markdown, no headings, no bullet lists.
        - Write 80 to 140 words as a single paragraph. Output only the paragraph.
        """;

    public static string BuildFactSheet(AthleteDetailsViewModel a)
    {
        var seasonsChronological = a.Seasons.AsEnumerable().Reverse().ToList();

        var sheet = new Dictionary<string, object?>
        {
            ["athlete"] = a.FullName,
            ["team"] = a.Gender == Gender.Female ? "girls" : "boys",
            ["classOf"] = a.GraduationYear,
            ["totals"] = new Dictionary<string, object?>
            {
                ["schoolRecords"] = a.TotalSchoolRecords,
                ["seasons"] = a.Seasons.Count
            },
            ["personalRecords"] = a.PersonalRecords.Select(r => new Dictionary<string, object?>
            {
                ["event"] = r.EventName,
                ["environment"] = Env(r.Environment),
                ["mark"] = r.Performance,
                ["date"] = r.Date.ToString("MMMM d, yyyy"),
                ["meet"] = r.MeetName,
                ["sr"] = r.IsSchoolRecord ? true : null,
                ["allTimeRank"] = r.AllTimeRank is <= 10 ? r.AllTimeRank : null,
                ["relay"] = r.IsRelay ? r.RelayMembers : null
            }).ToList(),
            ["careerArc"] = a.CareerCharts.Where(c => c.Points.Count > 0).Select(c =>
            {
                var first = c.Points.OrderBy(p => p.Date).First();
                return new Dictionary<string, object?>
                {
                    ["event"] = c.EventName,
                    ["environment"] = Env(c.Environment),
                    ["relay"] = c.IsRelay ? true : null,
                    ["performances"] = c.Points.Count,
                    ["first"] = new Dictionary<string, object?>
                    {
                        ["mark"] = first.Formatted,
                        ["date"] = first.Date.ToString("MMMM yyyy"),
                        ["class"] = first.ClassAtTime
                    },
                    ["best"] = c.CareerBestFormatted,
                    ["improvement"] = c.CareerImprovementFormatted == null ? null : $"{c.CareerImprovementFormatted} ({(c.IsFieldEvent ? "farther" : "faster")})"
                };
            }).ToList(),
            ["seasons"] = seasonsChronological.Select(s => new Dictionary<string, object?>
            {
                ["season"] = s.SeasonName,
                ["class"] = ClassForSeason(a.GraduationYear, s.SeasonName),
                ["schoolRecords"] = s.SchoolRecordCount,
                ["events"] = s.EventGroups.Select(g => new Dictionary<string, object?>
                {
                    ["event"] = g.EventName,
                    ["environment"] = Env(g.Environment),
                    ["seasonBest"] = g.PersonalRecordPerformance,
                    ["performances"] = g.Performances.Count,
                    ["relay"] = g.Performances.Any(p => p.IsRelay) && g.Performances.All(p => p.IsRelay) ? true : null
                }).ToList()
            }).ToList(),
            ["highlights"] = Highlights(seasonsChronological)
        };

        return FactSheetJson.Serialize(sheet);
    }

    public static string BuildUserPrompt(string factSheet) => "Fact sheet:\n" + factSheet + "\n\nWrite the recap.";

    public static string ComputeHash(string factSheet) => FactSheetJson.Hash(Version, SystemPrompt, factSheet);

    /// <summary>Every school-record or top-10 performance across the career, records first, deduplicated.</summary>
    private static List<Dictionary<string, object?>> Highlights(List<SeasonPerformanceViewModel> seasons) =>
        seasons.SelectMany(s => s.EventGroups.SelectMany(g => g.Performances
                .Where(p => p.IsSchoolRecord || p.AllTimeRank is <= 10)
                .Select(p => (Season: s.SeasonName, Group: g, Perf: p))))
            .DistinctBy(x => (x.Group.EventId, x.Perf.Performance, x.Perf.MeetName))
            .OrderByDescending(x => x.Perf.IsSchoolRecord).ThenBy(x => x.Perf.AllTimeRank ?? int.MaxValue).ThenBy(x => x.Perf.Date)
            .Select(x => new Dictionary<string, object?>
            {
                ["event"] = x.Group.EventName,
                ["environment"] = Env(x.Group.Environment),
                ["mark"] = x.Perf.Performance,
                ["meet"] = x.Perf.MeetName,
                ["date"] = x.Perf.Date.ToString("MMMM d, yyyy"),
                ["sr"] = x.Perf.IsSchoolRecord ? true : null,
                ["allTimeRank"] = x.Perf.AllTimeRank is <= 10 ? x.Perf.AllTimeRank : null,
                ["relay"] = x.Perf.IsRelay ? x.Perf.RelayMembers : null
            }).ToList();

    private static string Env(Environment e) => e == Environment.Indoor ? "indoor" : "outdoor";

    /// <summary>Season names are "YYYY-YYYY"; the class is read at the second year's spring.</summary>
    private static string? ClassForSeason(int graduationYear, string seasonName)
    {
        var parts = seasonName.Split('-');
        return parts.Length == 2 && int.TryParse(parts[1], out var endYear)
            ? ClassYearCalculator.GetClassAtTimeOfPerformance(graduationYear, new DateTime(endYear, 3, 1))
            : null;
    }
}
