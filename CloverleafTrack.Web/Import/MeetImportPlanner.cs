using System.Text.RegularExpressions;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Web.Utilities;

namespace CloverleafTrack.Web.Import;

/// <summary>
/// Turns a <see cref="ResultsFile"/> into reviewable rows: which event, which athlete(s), what mark, which places, and whether it is new,
/// already stored, different from what is stored, or unusable. Pure: no database, no writes, deterministic. The admin picks which rows
/// to apply; <see cref="MeetImportService"/> re-plans from the same file at apply time. BRAIN.md [C122].
/// </summary>
public static class MeetImportPlanner
{
    public const int EventLimit = 4;

    public static ImportPlan Plan(ResultsFile file, MeetImportContext ctx, IReadOnlyDictionary<int, int>? athleteOverrides = null)
    {
        var plan = new ImportPlan { Source = file.Source, Notes = file.Notes };

        if (file.Meet?.Id is int fileMeetId && fileMeetId != ctx.Meet.Id)
            plan.FileProblems.Add($"This file is for meet {fileMeetId}{(string.IsNullOrWhiteSpace(file.Meet.Name) ? "" : $" ({file.Meet.Name})")}, not {ctx.Meet.Name} (meet {ctx.Meet.Id}).");
        if (file.Version != 1)
            plan.FileProblems.Add($"Unsupported file version {file.Version}; this importer reads version 1.");
        if (file.Results.Count == 0 && file.Entries.Count == 0)
            plan.FileProblems.Add("The file has no results and no entries.");

        var index = 0;
        foreach (var r in file.Results)
            plan.Rows.Add(PlanRow(index++, ImportRowKind.Result, r.Gender, r.Event, r.Athlete, r.Athletes, r, ctx, athleteOverrides));
        foreach (var e in file.Entries)
            plan.Rows.Add(PlanRow(index++, ImportRowKind.Entry, e.Gender, e.Event, e.Athlete, e.Athletes, null, ctx, athleteOverrides));

        FlagDuplicatesInFile(plan);
        FlagEventLimit(plan, ctx);
        return plan;
    }

    private static ImportRow PlanRow(int index, ImportRowKind kind, string? genderText, string? eventText, string? athleteText,
        List<string>? relayTexts, ResultsFileResult? result, MeetImportContext ctx, IReadOnlyDictionary<int, int>? overrides)
    {
        var row = new ImportRow { Index = index, Kind = kind, Note = result?.Note };

        var gender = ParseGender(genderText);
        row.Gender = gender;
        row.EventLabel = $"{genderText} {eventText}".Trim();
        var ev = ResolveEvent(gender, eventText, ctx.Events);
        if (ev == null)
        {
            row.Problems.Add(gender == null
                ? $"Gender \"{genderText}\" is not Boys, Girls or Mixed."
                : $"No {gender} event called \"{eventText}\" for a {ctx.Meet.Environment.ToString().ToLowerInvariant()} meet.");
            row.State = ImportRowState.Problem;
            row.AthleteText = relayTexts is { Count: > 0 } ? string.Join(" / ", relayTexts) : athleteText ?? "";
            row.AthleteDisplay = row.AthleteText;
            return row;
        }

        row.EventId = ev.Id;
        row.EventType = ev.EventType;
        row.AthleteCount = ev.AthleteCount;
        row.IsRelay = ev.AthleteCount > 1;
        row.EventLabel = $"{GenderLabel(ev.Gender)} {ev.Name}";

        ResolveAthletes(row, ev, athleteText, relayTexts, ctx, overrides);
        if (kind == ImportRowKind.Result)
        {
            ParseMark(row, ev, result!);
            ResolvePlacings(row, result!, ctx);
        }

        if (row.Problems.Count > 0)
        {
            row.State = ImportRowState.Problem;
            return row;
        }

        row.MatchesEntry = HasEntry(row, ctx);
        row.State = kind == ImportRowKind.Entry ? EntryState(row, ctx) : ResultState(row, ctx);
        return row;
    }

    // ---- athletes ---------------------------------------------------------------------------------------------

    private static void ResolveAthletes(ImportRow row, Event ev, string? athleteText, List<string>? relayTexts,
        MeetImportContext ctx, IReadOnlyDictionary<int, int>? overrides)
    {
        if (row.IsRelay)
        {
            var names = relayTexts ?? new List<string>();
            row.AthleteText = string.Join(" / ", names);
            if (names.Count != ev.AthleteCount)
                row.Problems.Add($"{ev.Name} needs {ev.AthleteCount} athletes; the file lists {names.Count}.");
            var display = new List<string>();
            foreach (var name in names)
            {
                var id = ResolveAthlete(name, ev.Gender, ctx, out var candidates);
                if (id == null)
                {
                    row.Problems.Add(candidates.Count > 0
                        ? $"\"{name}\" is not an exact match (did you mean {string.Join(", ", candidates.Take(3).Select(c => c.Name))}?); fix the name in the file."
                        : $"No athlete named \"{name}\".");
                    display.Add(name);
                    continue;
                }
                if (row.RelayAthleteIds.Contains(id.Value)) row.Problems.Add($"\"{name}\" is listed twice.");
                row.RelayAthleteIds.Add(id.Value);
                display.Add(AthleteName(ctx, id.Value));
            }
            row.AthleteDisplay = string.Join(" / ", display);
            return;
        }

        row.AthleteText = athleteText ?? "";
        if (string.IsNullOrWhiteSpace(athleteText))
        {
            row.Problems.Add("No athlete given.");
            return;
        }

        int? athleteId;
        if (overrides != null && overrides.TryGetValue(row.Index, out var chosen) && ctx.Athletes.Any(a => a.Id == chosen))
        {
            athleteId = chosen;
            row.Candidates = Candidates(athleteText, ev.Gender, ctx);
        }
        else
        {
            athleteId = ResolveAthlete(athleteText, ev.Gender, ctx, out var candidates);
            row.Candidates = candidates;
        }

        if (athleteId == null)
        {
            row.AthleteDisplay = athleteText;
            row.Problems.Add(row.Candidates.Count > 0
                ? $"\"{athleteText}\" is not an exact, unique match. Pick the athlete below."
                : $"No athlete named \"{athleteText}\".");
            return;
        }
        row.AthleteId = athleteId;
        row.AthleteDisplay = AthleteName(ctx, athleteId.Value);
    }

    /// <summary>Exact, unique full-name match (preferring athletes of the right era); otherwise null with the near matches in <paramref name="candidates"/>.</summary>
    public static int? ResolveAthlete(string text, Gender? eventGender, MeetImportContext ctx, out List<AthleteCandidate> candidates)
    {
        var (first, last) = SplitName(text);
        var key = Norm(first) + Norm(last);
        var pool = ctx.Athletes.Where(a => GenderFits(a.Gender, eventGender)).ToList();
        var exact = pool.Where(a => Norm(a.FirstName) + Norm(a.LastName) == key).ToList();

        if (exact.Count > 1)
        {
            var inEra = exact.Where(a => InEra(a, ctx.Meet)).ToList();
            if (inEra.Count == 1) exact = inEra;
        }

        if (exact.Count == 1)
        {
            candidates = new List<AthleteCandidate>();
            return exact[0].Id;
        }

        candidates = exact.Count > 1 ? exact.Select(ToCandidate).ToList() : Candidates(text, eventGender, ctx);
        return null;
    }

    private static List<AthleteCandidate> Candidates(string text, Gender? eventGender, MeetImportContext ctx)
    {
        var (first, last) = SplitName(text);
        var f = Norm(first);
        var l = Norm(last);
        var full = f + l;
        return ctx.Athletes
            .Where(a => GenderFits(a.Gender, eventGender))
            .Select(a => new { a, af = Norm(a.FirstName), al = Norm(a.LastName) })
            .Where(x => (x.al == l && (x.af.StartsWith(f) || f.StartsWith(x.af) || Distance(x.af, f) <= 2))
                        || (x.af == f && Distance(x.al, l) <= 1)
                        || Distance(x.af + x.al, full) <= 2)
            .OrderByDescending(x => InEra(x.a, ctx.Meet))
            .ThenBy(x => Distance(x.af + x.al, full))
            .Take(6)
            .Select(x => ToCandidate(x.a))
            .ToList();
    }

    private static AthleteCandidate ToCandidate(Athlete a) => new() { Id = a.Id, Name = $"{a.FirstName} {a.LastName}", GraduationYear = a.GraduationYear };

    private static bool InEra(Athlete a, Meet meet) => a.GraduationYear >= meet.Date.Year && a.GraduationYear <= meet.Date.Year + 4;

    private static bool GenderFits(Gender athlete, Gender? eventGender) => eventGender is null or Gender.Mixed || athlete == eventGender;

    private static string AthleteName(MeetImportContext ctx, int id)
    {
        var a = ctx.Athletes.FirstOrDefault(x => x.Id == id);
        return a == null ? $"#{id}" : $"{a.FirstName} {a.LastName}";
    }

    /// <summary>"First Last" or "Last, First"; the last word is the surname otherwise.</summary>
    public static (string First, string Last) SplitName(string text)
    {
        var t = text.Trim();
        var comma = t.IndexOf(',');
        if (comma > 0) return (t[(comma + 1)..].Trim(), t[..comma].Trim());
        var space = t.LastIndexOf(' ');
        return space < 0 ? (t, "") : (t[..space].Trim(), t[(space + 1)..].Trim());
    }

    private static string Norm(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static int Distance(string a, string b)
    {
        if (a == b) return 0;
        var prev = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var cur = new int[b.Length + 1];
            cur[0] = i;
            for (var j = 1; j <= b.Length; j++)
                cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            prev = cur;
        }
        return prev[b.Length];
    }

    // ---- events -----------------------------------------------------------------------------------------------

    public static Gender? ParseGender(string? text) => (text ?? "").Trim().ToLowerInvariant() switch
    {
        "boys" or "boy" or "b" or "male" or "m" or "men" or "man" => Gender.Male,
        "girls" or "girl" or "g" or "female" or "f" or "women" or "woman" => Gender.Female,
        "mixed" or "mix" or "coed" or "x" => Gender.Mixed,
        _ => null
    };

    private static string GenderLabel(Gender? g) => g switch { Gender.Male => "Boys", Gender.Female => "Girls", Gender.Mixed => "Mixed", _ => "" };

    private static readonly Dictionary<string, string> EventAliases = new()
    {
        ["shot"] = "shotput", ["shotputthrow"] = "shotput", ["discusthrow"] = "discus", ["hammer"] = "hammerthrow",
        ["javelin"] = "javelinthrow", ["steeplechase"] = "2ksteeplechase", ["steeple"] = "2ksteeplechase",
        ["pv"] = "polevault", ["hj"] = "highjump", ["lj"] = "longjump", ["tj"] = "triplejump"
    };

    /// <summary>Lower-case, letters and digits only, and the "m" in "100m" dropped so "100", "100m" and "100 M" meet.</summary>
    public static string NormEvent(string s)
    {
        var n = Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9]", "");
        n = Regex.Replace(n, @"(?<=\d)(?:meters?|metres?|m)(?![a-z]*ile)", "");
        return EventAliases.TryGetValue(n, out var alias) ? alias : n;
    }

    public static Event? ResolveEvent(Gender? gender, string? name, IEnumerable<Event> events)
    {
        if (gender == null || string.IsNullOrWhiteSpace(name)) return null;
        var pool = events.Where(e => e.Gender == gender).ToList();
        var key = NormEvent(name);
        return pool.FirstOrDefault(e => NormEvent(e.Name) == key)
               ?? pool.FirstOrDefault(e => NormEvent(e.Name) == key + "relay");
    }

    // ---- marks ------------------------------------------------------------------------------------------------

    private static bool IsFieldType(EventType t) => t is EventType.Field or EventType.FieldRelay or EventType.JumpRelay or EventType.ThrowsRelay;

    private static void ParseMark(ImportRow row, Event ev, ResultsFileResult r)
    {
        var statusText = (r.Status ?? "").Trim().ToUpperInvariant();
        if (statusText.Length > 0)
        {
            row.Status = statusText switch
            {
                "NM" => ResultStatus.NoMark,
                "NH" => ResultStatus.NoHeight,
                "DNF" => ResultStatus.DidNotFinish,
                "DQ" => ResultStatus.Disqualified,
                _ => ResultStatus.Mark
            };
            if (row.Status == ResultStatus.Mark) row.Problems.Add($"Unknown status \"{r.Status}\" (use NM, NH, DNF or DQ).");
            else if (!string.IsNullOrWhiteSpace(r.Mark)) row.Problems.Add("Give a mark or a status, not both.");
            row.MarkText = statusText;
            return;
        }

        if (string.IsNullOrWhiteSpace(r.Mark))
        {
            row.Problems.Add("No mark or status.");
            return;
        }

        if (IsFieldType(ev.EventType))
        {
            row.DistanceInches = PerformanceFormatHelper.ParseDistance(r.Mark);
            if (row.DistanceInches is null or <= 0) row.Problems.Add($"Can't read \"{r.Mark}\" as a distance (use 15' 10.25\" or 15-10.25).");
            else row.MarkText = PerformanceFormatHelper.FormatDistance(row.DistanceInches.Value);
        }
        else
        {
            row.TimeSeconds = PerformanceFormatHelper.ParseTime(r.Mark);
            if (row.TimeSeconds is null or <= 0) row.Problems.Add($"Can't read \"{r.Mark}\" as a time (use 11.23 or 2:05.40).");
            else row.MarkText = PerformanceFormatHelper.FormatTime(row.TimeSeconds.Value);
        }
    }

    // ---- placings ---------------------------------------------------------------------------------------------

    private static void ResolvePlacings(ImportRow row, ResultsFileResult r, MeetImportContext ctx)
    {
        var given = new List<(string? Opponent, int Place)>();
        if (r.Place.HasValue) given.Add((null, r.Place.Value));
        if (r.Placings != null) given.AddRange(r.Placings.Select(p => (p.Opponent, p.Place)));
        if (given.Count == 0) return;

        if (given.Any(g => g.Place < 1))
        {
            row.Problems.Add("Places start at 1.");
            return;
        }

        if (ctx.Meet.MeetType == MeetType.Invitational)
        {
            var overall = given.Where(g => g.Opponent == null).Select(g => g.Place).Distinct().ToList();
            if (given.Any(g => g.Opponent != null)) row.Warnings.Add("An invitational has one overall place; per-opponent places were ignored.");
            if (overall.Count == 1) row.Placings.Add(new PlanPlacing { OpponentLabel = "Overall", Place = overall[0] });
            else if (overall.Count > 1) row.Problems.Add("Several overall places given.");
            return;
        }

        foreach (var (opponent, place) in given)
        {
            MeetParticipant? participant;
            if (opponent == null)
            {
                if (ctx.Participants.Count != 1)
                {
                    row.Problems.Add($"Give the opponent for each place ({string.Join(", ", ctx.Participants.Select(p => p.SchoolName))}).");
                    continue;
                }
                participant = ctx.Participants[0];
            }
            else
            {
                participant = ResolveOpponent(opponent, ctx.Participants);
                if (participant == null)
                {
                    row.Problems.Add($"Opponent \"{opponent}\" is not in this meet ({string.Join(", ", ctx.Participants.Select(p => p.SchoolName))}).");
                    continue;
                }
            }

            if (row.Placings.Any(p => p.MeetParticipantId == participant.Id))
                row.Problems.Add($"Two places given against {participant.SchoolName}.");
            else
                row.Placings.Add(new PlanPlacing { MeetParticipantId = participant.Id, OpponentLabel = $"vs. {participant.SchoolName}", Place = place });
        }
    }

    private static MeetParticipant? ResolveOpponent(string text, List<MeetParticipant> participants)
    {
        var key = Norm(text);
        if (key.Length == 0) return null;
        var matches = participants.Where(p => Norm(p.SchoolName) == key).ToList();
        if (matches.Count == 0)
            matches = participants.Where(p => Norm(p.SchoolName).StartsWith(key) || key.StartsWith(Norm(p.SchoolName))).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    // ---- comparison with what is stored -----------------------------------------------------------------------

    private static bool HasEntry(ImportRow row, MeetImportContext ctx)
    {
        if (row.EventId == null) return false;
        if (!row.IsRelay)
            return row.AthleteId != null && ctx.Entries.Any(e => e.EventId == row.EventId && e.AthleteId == row.AthleteId);

        var names = row.RelayAthleteIds.Select(id => Norm(AthleteName(ctx, id))).OrderBy(x => x).ToList();
        return ctx.Entries.Any(e => e.EventId == row.EventId && e.AthleteId == null && e.RelayAthleteNames != null
            && e.RelayAthleteNames.Split("|~|").Select(Norm).OrderBy(x => x).SequenceEqual(names));
    }

    private static ImportRowState EntryState(ImportRow row, MeetImportContext ctx) =>
        row.MatchesEntry || ExistingPerformanceFor(row, ctx) != null ? ImportRowState.AlreadyThere : ImportRowState.New;

    private static ExistingPerformance? ExistingPerformanceFor(ImportRow row, MeetImportContext ctx)
    {
        var sameEvent = ctx.Performances.Where(p => p.EventId == row.EventId);
        if (!row.IsRelay) return sameEvent.FirstOrDefault(p => p.AthleteId == row.AthleteId);
        var ids = row.RelayAthleteIds.OrderBy(x => x).ToList();
        return sameEvent.FirstOrDefault(p => p.AthleteId == null && p.RelayAthleteIds.OrderBy(x => x).SequenceEqual(ids));
    }

    private static ImportRowState ResultState(ImportRow row, MeetImportContext ctx)
    {
        var existing = ExistingPerformanceFor(row, ctx);
        if (existing == null) return ImportRowState.New;

        row.ExistingPerformanceId = existing.Id;
        row.ExistingMarkText = existing.Status != ResultStatus.Mark ? existing.Status.Abbreviation()
            : existing.TimeSeconds.HasValue ? PerformanceFormatHelper.FormatTime(existing.TimeSeconds.Value)
            : existing.DistanceInches.HasValue ? PerformanceFormatHelper.FormatDistance(existing.DistanceInches.Value) : "";

        var sameMark = existing.Status == row.Status
                       && Math.Abs((existing.TimeSeconds ?? 0) - (row.TimeSeconds ?? 0)) < 0.005
                       && Math.Abs((existing.DistanceInches ?? 0) - (row.DistanceInches ?? 0)) < 0.01;

        var stored = ctx.Placings.Where(p => p.PerformanceId == existing.Id).ToList();
        var placingDiffers = row.Placings.Any(p => stored.Any(s => s.MeetParticipantId == p.MeetParticipantId && s.Place != p.Place));
        var placingsToAdd = row.Placings.Any(p => stored.All(s => s.MeetParticipantId != p.MeetParticipantId));

        if (!sameMark || placingDiffers) return ImportRowState.Differs;
        return placingsToAdd ? ImportRowState.AddsPlacings : ImportRowState.AlreadyThere;
    }

    // ---- whole-file checks ------------------------------------------------------------------------------------

    private static void FlagDuplicatesInFile(ImportPlan plan)
    {
        var seen = new Dictionary<string, int>();
        foreach (var row in plan.Rows.Where(r => r.EventId != null && r.State != ImportRowState.Problem))
        {
            var who = row.IsRelay ? string.Join(",", row.RelayAthleteIds.OrderBy(x => x)) : row.AthleteId?.ToString() ?? "";
            var key = $"{row.Kind}|{row.EventId}|{who}";
            if (seen.TryGetValue(key, out var first))
            {
                row.Problems.Add($"Same event and athlete as row {first + 1} of the file.");
                row.State = ImportRowState.Problem;
            }
            else seen[key] = row.Index;
        }
    }

    /// <summary>Warns (never blocks) when an athlete would be in more than four events at this meet, counting stored entries and results.</summary>
    private static void FlagEventLimit(ImportPlan plan, MeetImportContext ctx)
    {
        var events = new Dictionary<int, HashSet<int>>();
        void Add(int athleteId, int eventId)
        {
            if (!events.TryGetValue(athleteId, out var set)) events[athleteId] = set = new HashSet<int>();
            set.Add(eventId);
        }

        foreach (var e in ctx.Entries.Where(e => e.AthleteId.HasValue)) Add(e.AthleteId!.Value, e.EventId);
        foreach (var p in ctx.Performances)
        {
            if (p.AthleteId.HasValue) Add(p.AthleteId.Value, p.EventId);
            foreach (var id in p.RelayAthleteIds) Add(id, p.EventId);
        }
        foreach (var row in plan.Rows.Where(r => r.EventId != null && r.State != ImportRowState.Problem))
        {
            if (row.AthleteId.HasValue) Add(row.AthleteId.Value, row.EventId!.Value);
            foreach (var id in row.RelayAthleteIds) Add(id, row.EventId!.Value);
        }

        foreach (var row in plan.Rows.Where(r => r.EventId != null && r.State != ImportRowState.Problem))
        {
            var ids = row.AthleteId.HasValue ? new List<int> { row.AthleteId.Value } : row.RelayAthleteIds;
            foreach (var id in ids.Where(id => events.TryGetValue(id, out var set) && set.Count > EventLimit))
                row.Warnings.Add($"{AthleteName(ctx, id)} is in {events[id].Count} events at this meet (limit {EventLimit}).");
        }
    }
}
