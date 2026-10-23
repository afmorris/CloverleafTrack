using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Meets;
using CloverleafTrack.ViewModels.Scoring;
using CloverleafTrack.ViewModels.Seasons;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

/// <summary>
/// Pure: turns a season's marks, prior bests, team results, MeetScores and scoring totals into the season wrap. No database, deterministic.
/// PR / school-record counts use the "at the time" flags (BRAIN.md [C118]) so a PR later beaten still counts; "breakouts" and "improvements"
/// only count marks that beat an earlier mark, never an athlete's first-ever mark in an event. BRAIN.md [C123].
/// </summary>
public static class SeasonWrapCalculator
{
    public const int ListSize = 5;
    private static readonly SlugHelper Slugs = new();

    public static SeasonWrapViewModel Build(
        Season season,
        IReadOnlyList<SeasonWrapMarkDto> marks,
        IReadOnlyList<SeasonWrapPriorBestDto> priorBests,
        IReadOnlyList<SeasonWrapTeamResultDto> teamResults,
        IReadOnlyDictionary<int, MeetScoreViewModel> meetScores,
        SeasonScoringViewModel? scoring)
    {
        var vm = new SeasonWrapViewModel
        {
            SeasonId = season.Id,
            Name = season.Name,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            InProgress = season.IsCurrentSeason,
            ThroughDate = marks.Count == 0 ? null : marks.Max(m => m.MeetDate),
            ScoringEnabled = season.ScoringEnabled,
            SeniorClassYear = season.EndDate.Year
        };
        if (marks.Count == 0) return vm;

        var individual = marks.Where(m => m.AthleteId.HasValue).ToList();

        vm.Meets = marks.Select(m => m.MeetId).Distinct().Count();
        vm.Results = marks.Count;
        vm.AthletesCompeted = individual.Select(m => m.AthleteId!.Value).Distinct().Count();
        vm.PRs = individual.Count(m => m.PRAtTime);
        vm.AthletesWithPRs = individual.Where(m => m.PRAtTime).Select(m => m.AthleteId!.Value).Distinct().Count();
        vm.SchoolRecordBreaks = marks.Count(m => m.SRAtTime);
        vm.TopTenMarks = marks.Count(m => m.AllTimeRank is <= 10);

        vm.SchoolRecords = SchoolRecords(marks);
        vm.SchoolRecordsSet = vm.SchoolRecords.Count;
        vm.BestMeets = BestMeets(marks, meetScores);
        vm.Standouts = Standouts(individual);
        vm.Breakouts = Breakouts(individual);
        vm.Improvements = Improvements(individual, priorBests);
        vm.Seniors = Seniors(individual, season.EndDate.Year);
        vm.DualRecord = DualRecord(teamResults);
        if (scoring != null)
        {
            vm.BoysLeaders = Leaders(scoring.Boys);
            vm.GirlsLeaders = Leaders(scoring.Girls);
        }
        vm.Progression = Progression(marks);
        return vm;
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    private static bool IsField(EventType t) => t is EventType.Field or EventType.FieldRelay or EventType.JumpRelay or EventType.ThrowsRelay;

    /// <summary>Higher is always better (field = inches, running = negated seconds).</summary>
    private static double Score(SeasonWrapMarkDto m) => IsField(m.EventType) ? m.DistanceInches ?? double.MinValue : -(m.TimeSeconds ?? double.MaxValue);

    private static string Mark(SeasonWrapMarkDto m) => Format(m.TimeSeconds, m.DistanceInches);

    private static string Format(double? time, double? distance) => MeetService.FormatPerformance(time, distance);

    private static string EventLabel(SeasonWrapMarkDto m) => EventLabel(m.EventGender, m.EventName, m.Environment);

    private static string EventLabel(Gender gender, string name, Environment env) =>
        $"{(gender == Gender.Male ? "Boys" : gender == Gender.Female ? "Girls" : "Mixed")} {name}{(env == Environment.Indoor ? " (indoor)" : "")}";

    private static string Name(SeasonWrapMarkDto m) => $"{m.AthleteFirstName} {m.AthleteLastName}".Trim();

    private static WrapPerson Person(string name) => new() { Name = name, Slug = Slugs.GenerateSlug(name) };

    private static WrapPerson Person(SeasonWrapMarkDto m) => Person(Name(m));

    private static string MeetSlug(string meetName) => Slugs.GenerateSlug(meetName);

    // ---- sections ---------------------------------------------------------------------------------------------

    private static List<WrapSchoolRecord> SchoolRecords(IReadOnlyList<SeasonWrapMarkDto> marks) =>
        marks.Where(m => m.SRAtTime)
            .GroupBy(m => m.EventId)
            .Select(g =>
            {
                var best = g.OrderByDescending(Score).ThenBy(m => m.MeetDate).First();
                var holders = best.AthleteId.HasValue
                    ? new List<WrapPerson> { Person(best) }
                    : (best.RelayAthletes ?? "").Split("|~|", StringSplitOptions.RemoveEmptyEntries).Select(Person).ToList();
                return new WrapSchoolRecord
                {
                    EventLabel = EventLabel(best),
                    Gender = best.EventGender,
                    Environment = best.Environment,
                    Holders = holders,
                    Mark = Mark(best),
                    MeetName = best.MeetName,
                    MeetSlug = MeetSlug(best.MeetName),
                    Date = best.MeetDate,
                    TimesBroken = g.Count(),
                    StillRecord = best.AllTimeRank == 1
                };
            })
            .OrderBy(r => r.Gender).ThenBy(r => r.Environment).ThenBy(r => r.EventLabel)
            .ToList();

    private static List<WrapMeetScore> BestMeets(IReadOnlyList<SeasonWrapMarkDto> marks, IReadOnlyDictionary<int, MeetScoreViewModel> scores) =>
        marks.GroupBy(m => m.MeetId)
            .Select(g => (Meet: g.First(), Score: scores.GetValueOrDefault(g.Key)))
            .Where(x => x.Score is { Basis: MeetScoreBasis.Full })
            .OrderByDescending(x => x.Score!.Score).ThenBy(x => x.Meet.MeetDate)
            .Take(3)
            .Select(x => new WrapMeetScore
            {
                Name = x.Meet.MeetName,
                Slug = MeetSlug(x.Meet.MeetName),
                Date = x.Meet.MeetDate,
                Score = x.Score!.Score,
                PRs = x.Score.PRs
            })
            .ToList();

    private static List<WrapStandout> Standouts(IReadOnlyList<SeasonWrapMarkDto> individual) =>
        individual.Where(m => m.Percentile.HasValue)
            .GroupBy(m => m.AthleteId!.Value)
            .Select(g => g.OrderByDescending(m => m.Percentile).ThenBy(m => m.AllTimeRank ?? int.MaxValue).ThenByDescending(m => m.MeetDate).First())
            .OrderByDescending(m => m.Percentile).ThenBy(m => m.AllTimeRank ?? int.MaxValue).ThenBy(m => Name(m))
            .Take(ListSize)
            .Select(m => new WrapStandout
            {
                Athlete = Person(m),
                EventLabel = EventLabel(m),
                Mark = Mark(m),
                Percentile = m.Percentile!.Value,
                AllTimeRank = m.AllTimeRank,
                MeetName = m.MeetName,
                MeetSlug = MeetSlug(m.MeetName),
                Date = m.MeetDate
            })
            .ToList();

    /// <summary>Relative gain of a PR over the previous best: 0.03 = 3% better. Needs a previous best.</summary>
    private static double Gain(SeasonWrapMarkDto m)
    {
        if (m.PreviousBest is not > 0) return 0;
        return IsField(m.EventType)
            ? ((m.DistanceInches ?? 0) - m.PreviousBest.Value) / m.PreviousBest.Value
            : (m.PreviousBest.Value - (m.TimeSeconds ?? m.PreviousBest.Value)) / m.PreviousBest.Value;
    }

    private static List<WrapBreakout> Breakouts(IReadOnlyList<SeasonWrapMarkDto> individual) =>
        individual.Where(m => m.PRAtTime && m.PreviousBest.HasValue)
            .GroupBy(m => m.AthleteId!.Value)
            .Select(g =>
            {
                var top = g.OrderByDescending(Gain).First();
                var prev = Format(IsField(top.EventType) ? null : top.PreviousBest, IsField(top.EventType) ? top.PreviousBest : null);
                return (Count: g.Count(), Total: g.Sum(Gain), Row: new WrapBreakout
                {
                    Athlete = Person(top),
                    PRs = g.Count(),
                    Events = g.Select(m => m.EventId).Distinct().Count(),
                    Highlight = $"{top.EventName}{(top.Environment == Environment.Indoor ? " (indoor)" : "")}: {prev} → {Mark(top)}"
                });
            })
            .OrderByDescending(x => x.Count).ThenByDescending(x => x.Total).ThenBy(x => x.Row.Athlete.Name)
            .Take(ListSize)
            .Select(x => x.Row)
            .ToList();

    private static List<WrapImprovement> Improvements(IReadOnlyList<SeasonWrapMarkDto> individual, IReadOnlyList<SeasonWrapPriorBestDto> priors)
    {
        var prior = priors.ToDictionary(p => (p.AthleteId, p.EventId));
        var result = new List<(double Percent, WrapImprovement Row)>();
        foreach (var g in individual.GroupBy(m => (Athlete: m.AthleteId!.Value, m.EventId)))
        {
            if (!prior.TryGetValue((g.Key.Athlete, g.Key.EventId), out var p)) continue;
            var best = g.OrderByDescending(Score).First();
            var field = IsField(best.EventType);
            var before = field ? p.BestDistance : p.BestTime;
            var after = field ? best.DistanceInches : best.TimeSeconds;
            if (before is not > 0 || after is not > 0) continue;

            var diff = field ? after.Value - before.Value : before.Value - after.Value;
            if (diff <= 0) continue;
            var percent = diff / before.Value * 100;
            result.Add((percent, new WrapImprovement
            {
                Athlete = Person(best),
                EventLabel = EventLabel(best),
                Before = Format(field ? null : before, field ? before : null),
                After = Mark(best),
                Change = field ? $"{Format(null, diff)} farther" : $"{diff:0.00}s faster",
                Percent = Math.Round(percent, 1)
            }));
        }
        return result.OrderByDescending(x => x.Percent).ThenBy(x => x.Row.Athlete.Name).Take(ListSize).Select(x => x.Row).ToList();
    }

    private static List<WrapSenior> Seniors(IReadOnlyList<SeasonWrapMarkDto> individual, int classYear) =>
        individual.Where(m => m.GraduationYear == classYear)
            .GroupBy(m => m.AthleteId!.Value)
            .Select(g =>
            {
                var best = g.Where(m => m.Percentile.HasValue).OrderByDescending(m => m.Percentile).ThenBy(m => m.AllTimeRank ?? int.MaxValue).FirstOrDefault() ?? g.First();
                return new WrapSenior
                {
                    Athlete = Person(best),
                    Events = g.Select(m => m.EventId).Distinct().Count(),
                    PRs = g.Count(m => m.PRAtTime && m.PreviousBest.HasValue),
                    BestEventLabel = EventLabel(best),
                    BestMark = Mark(best),
                    BestPercentile = best.Percentile ?? 0
                };
            })
            .OrderByDescending(s => s.BestPercentile).ThenBy(s => s.Athlete.Name)
            .ToList();

    private static WrapDualRecord? DualRecord(IReadOnlyList<SeasonWrapTeamResultDto> teamResults)
    {
        if (teamResults.Count == 0) return null;
        var record = new WrapDualRecord();

        string Cell(decimal ours, decimal theirs) => ours > theirs ? $"W {ours:0.##}-{theirs:0.##}" : ours < theirs ? $"L {ours:0.##}-{theirs:0.##}" : $"T {ours:0.##}-{theirs:0.##}";
        void Tally(WrapRecord r, decimal ours, decimal theirs)
        {
            if (ours > theirs) r.Wins++;
            else if (ours < theirs) r.Losses++;
            else r.Ties++;
        }

        foreach (var g in teamResults.GroupBy(t => (t.MeetId, Opponent: t.OpponentName ?? "")).OrderBy(g => g.First().MeetDate).ThenBy(g => g.Key.Opponent))
        {
            var row = new WrapDualRow
            {
                Date = g.First().MeetDate,
                MeetName = g.First().MeetName,
                MeetSlug = MeetSlug(g.First().MeetName),
                Opponent = g.Key.Opponent
            };
            foreach (var t in g)
            {
                if (t.Gender == Gender.Male) { row.Boys = Cell(t.OurScore!.Value, t.OpponentScore!.Value); Tally(record.Boys, t.OurScore.Value, t.OpponentScore.Value); }
                else if (t.Gender == Gender.Female) { row.Girls = Cell(t.OurScore!.Value, t.OpponentScore!.Value); Tally(record.Girls, t.OurScore.Value, t.OpponentScore.Value); }
            }
            record.Duals.Add(row);
        }
        return record;
    }

    private static List<WrapScoringLeader> Leaders(IEnumerable<AthleteScoreRowViewModel> rows) =>
        rows.Where(r => r.TotalSplitPoints > 0)
            .OrderByDescending(r => r.TotalSplitPoints).ThenBy(r => r.AthleteLastName).ThenBy(r => r.AthleteFirstName)
            .Take(ListSize)
            .Select(r => new WrapScoringLeader { Athlete = new WrapPerson { Name = r.AthleteFullName, Slug = r.AthleteSlug }, Points = r.TotalSplitPoints, Events = r.EventCount })
            .ToList();

    private static List<WrapMeetPoint> Progression(IReadOnlyList<SeasonWrapMarkDto> marks) =>
        marks.GroupBy(m => m.MeetId)
            .Select(g =>
            {
                var withPct = g.Where(m => m.Percentile.HasValue).ToList();
                return new WrapMeetPoint
                {
                    Name = g.First().MeetName,
                    Slug = MeetSlug(g.First().MeetName),
                    Date = g.First().MeetDate,
                    Results = g.Count(),
                    PRs = g.Count(m => m.AthleteId.HasValue && m.PRAtTime),
                    SchoolRecords = g.Count(m => m.SRAtTime),
                    AveragePercentile = withPct.Count == 0 ? 0 : Math.Round(withPct.Average(m => (double)m.Percentile!.Value), 1)
                };
            })
            .OrderBy(p => p.Date).ThenBy(p => p.Name)
            .ToList();
}
