using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Services;

/// <summary>
/// Builds the input for the written "season in words" on the season wrap. Same pattern as <see cref="MeetSummaryPrompt"/>: the writer
/// (an Opus subagent in chat, never the site) sees only this fact sheet — names, marks and counts from <see cref="SeasonWrapViewModel"/>
/// plus data-rule notes. BRAIN.md [C124].
/// </summary>
public static class SeasonSummaryPrompt
{
    /// <summary>Bump when the system prompt or fact-sheet shape changes so every summary is treated as stale.</summary>
    public const string Version = "1";

    public const string SystemPrompt = """
        You write the short "season in words" for the Cloverleaf track & field results website's season wrap page.
        You are given a JSON fact sheet for ONE season. Write the recap from that fact sheet only.

        Rules:
        - Use only facts present in the JSON. Never invent names, marks, places, scores, records, weather, or history.
        - Do not do arithmetic. Quote counts exactly as given. Do not compare to other seasons.
        - Only name athletes and meets that appear in the JSON. Read the "notes" list first and follow it: it explains what each count means.
        - Headline "personalRecords" counts a first-ever mark in an event as a personal record, so it is generous. Say "personal records"
          or "PRs" for that number only as given; do not call every one of them an improvement. "breakouts" and "biggestJumps" are
          the genuine improvements.
        - A school record is credited only to the people and marks listed under "schoolRecords". "stillRecord" false means a later mark
          has since broken it; say so or leave it out, never call it the current record.
        - If "inProgress" is true the season is still running: write in a "so far" voice and do not sum it up as finished.
        - Mention the dual-meet record only when "dualRecord" is present, exactly as given. Mention team scoring leaders only when
          "scoringLeaders" is present. Say nothing about team results otherwise.
        - Seniors may be thanked or noted by name only from "seniors". Do not guess anyone's plans.
        - Name events exactly as their "event" value appears in the fact sheet (for example "100M", "4x400M Relay", "Long Jump"),
          with "boys" or "girls" before it where the fact sheet gives a gender. Never write "800 meters" or "the 800".
        - Plain, warm, factual tone like a good local sports page. Past tense (present perfect when in progress). No emojis, no
          markdown, no headings, no bullet lists.
        - Write 120 to 220 words in two or three short paragraphs separated by a blank line. Output only the paragraphs.
        """;

    public static string BuildFactSheet(SeasonWrapViewModel wrap)
    {
        static string Date(DateTime d) => d.ToString("MMMM d, yyyy");
        static string Names(IEnumerable<WrapPerson> p) => string.Join(", ", p.Select(x => x.Name));

        var sheet = new Dictionary<string, object?>
        {
            ["season"] = wrap.Name,
            ["inProgress"] = wrap.InProgress ? true : null,
            ["throughDate"] = wrap.ThroughDate is { } t ? Date(t) : null,
            ["notes"] = Notes,
            ["totals"] = new Dictionary<string, object?>
            {
                ["meets"] = wrap.Meets,
                ["results"] = wrap.Results,
                ["athletesCompeted"] = wrap.AthletesCompeted,
                ["personalRecords"] = wrap.PRs,
                ["athletesWithPersonalRecords"] = wrap.AthletesWithPRs,
                ["schoolRecordsSet"] = wrap.SchoolRecordsSet,
                ["schoolRecordBreaks"] = wrap.SchoolRecordBreaks,
                ["topTenAllTimeMarks"] = wrap.TopTenMarks
            },
            ["schoolRecords"] = wrap.SchoolRecords.Select(r => new Dictionary<string, object?>
            {
                ["event"] = r.EventLabel,
                ["gender"] = r.Gender.ToString(),
                ["environment"] = r.Environment.ToString(),
                ["holders"] = r.Holders.Select(h => h.Name).ToList(),
                ["mark"] = r.Mark,
                ["meet"] = r.MeetName,
                ["date"] = Date(r.Date),
                ["timesBroken"] = r.TimesBroken,
                ["stillRecord"] = r.StillRecord
            }).ToList(),
            ["bestMeets"] = wrap.BestMeets.Select(m => new Dictionary<string, object?>
            {
                ["meet"] = m.Name, ["date"] = Date(m.Date), ["personalRecords"] = m.PRs
            }).ToList(),
            ["standouts"] = wrap.Standouts.Select(s => new Dictionary<string, object?>
            {
                ["athlete"] = s.Athlete.Name, ["event"] = s.EventLabel, ["mark"] = s.Mark,
                ["allTimeRank"] = s.AllTimeRank is <= 10 ? s.AllTimeRank : null,
                ["meet"] = s.MeetName, ["date"] = Date(s.Date)
            }).ToList(),
            ["breakouts"] = wrap.Breakouts.Select(b => new Dictionary<string, object?>
            {
                ["athlete"] = b.Athlete.Name, ["improvedMarks"] = b.PRs, ["events"] = b.Events, ["highlight"] = b.Highlight
            }).ToList(),
            ["biggestJumps"] = wrap.Improvements.Select(i => new Dictionary<string, object?>
            {
                ["athlete"] = i.Athlete.Name, ["event"] = i.EventLabel, ["before"] = i.Before, ["after"] = i.After, ["change"] = i.Change
            }).ToList(),
            ["seniors"] = wrap.Seniors.Select(s => new Dictionary<string, object?>
            {
                ["athlete"] = s.Athlete.Name, ["events"] = s.Events, ["bestEvent"] = s.BestEventLabel, ["bestMark"] = s.BestMark
            }).ToList(),
            ["dualRecord"] = wrap.DualRecord is { } d && (d.Boys.Any || d.Girls.Any)
                ? new Dictionary<string, object?>
                {
                    ["boys"] = d.Boys.Any ? d.Boys.Text : null,
                    ["girls"] = d.Girls.Any ? d.Girls.Text : null,
                    ["duals"] = d.Duals.Select(x => new Dictionary<string, object?>
                    {
                        ["date"] = Date(x.Date), ["opponent"] = x.Opponent,
                        ["boys"] = x.Boys.Length > 0 ? x.Boys : null, ["girls"] = x.Girls.Length > 0 ? x.Girls : null
                    }).ToList()
                }
                : null,
            ["scoringLeaders"] = wrap.ScoringEnabled && (wrap.BoysLeaders.Count > 0 || wrap.GirlsLeaders.Count > 0)
                ? new Dictionary<string, object?>
                {
                    ["boys"] = wrap.BoysLeaders.Select(l => new Dictionary<string, object?> { ["athlete"] = l.Athlete.Name, ["points"] = l.Points }).ToList(),
                    ["girls"] = wrap.GirlsLeaders.Select(l => new Dictionary<string, object?> { ["athlete"] = l.Athlete.Name, ["points"] = l.Points }).ToList()
                }
                : null
        };
        return FactSheetJson.Serialize(sheet);
    }

    private static readonly string[] Notes =
    [
        "personalRecords counts a first-ever mark in an event as a PR, so the total is generous; improvedMarks and biggestJumps count only marks that beat an earlier mark.",
        "schoolRecordsSet is events with a new school record this season; schoolRecordBreaks counts every mark that broke the record when run, so it can be larger.",
        "stillRecord false means the school record set this season has since been broken.",
        "topTenAllTimeMarks counts marks ranked in the all-time top 10 at the end of the season.",
        "Relay school records list all team members as holders.",
        "Seated (wheelchair) events never score for the team."
    ];

    public static string BuildUserPrompt(string factSheet) =>
        "Fact sheet:\n" + factSheet + "\n\nWrite the season in words.";

    public static string ComputeHash(string factSheet) => FactSheetJson.Hash(Version, SystemPrompt, factSheet);
}
