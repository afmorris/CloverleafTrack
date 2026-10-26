using CloverleafTrack.ViewModels.Athletes;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

/// <summary>
/// Builds the athlete career wrap from the athlete detail view model, so the wrap and the athlete page can never disagree.
/// Pure: no I/O. PR counts use the at-the-time flag and only count genuine improvements (BRAIN.md [C118], [C126]).
/// </summary>
public static class AthleteWrapCalculator
{
    public const int MaxImprovements = 5;
    public const int MaxBests = 10;

    /// <summary>Seasons ending in or before this year only have scoring performances recorded (same cut-off as MeetScore).</summary>
    private const int LastPartialSeasonEndYear = 2020;

    public static AthleteWrapViewModel Build(AthleteDetailsViewModel d, string slug)
    {
        var vm = new AthleteWrapViewModel
        {
            AthleteId = d.AthleteId,
            Slug = slug,
            FullName = d.FullName,
            GraduationYear = d.GraduationYear,
            Gender = d.Gender,
            Class = d.Class,
            SummaryText = d.SummaryText,
            SummaryHtml = d.SummaryHtml,
            CareerCharts = d.CareerCharts
        };

        var chronological = d.Seasons.OrderBy(s => SeasonSortKey(s.SeasonName)).ToList();
        vm.Seasons = chronological.Count;
        vm.FirstSeason = chronological.FirstOrDefault()?.SeasonName ?? string.Empty;
        vm.LastSeason = chronological.LastOrDefault()?.SeasonName ?? string.Empty;
        vm.HasPartialSeasons = chronological.Any(s => SeasonEndYear(s.SeasonName) is { } y && y <= LastPartialSeasonEndYear);

        var all = chronological.SelectMany(s => s.EventGroups.SelectMany(g => g.Performances.Select(p => (Season: s, Group: g, Perf: p)))).ToList();
        vm.Meets = all.Select(x => (x.Perf.MeetName, x.Perf.Date.Date)).Distinct().Count();
        vm.Results = all.Count(x => x.Perf.HasMark);
        vm.PRs = all.Count(x => x.Perf.IsPersonalBest && x.Perf.PreviousBest != null);
        vm.SchoolRecords = d.TotalSchoolRecords;
        vm.TopTenEvents = d.PersonalRecords.Count(r => r.AllTimeRank is <= 10);

        vm.SeasonRows = chronological.Select(s => new AthleteWrapSeason
        {
            Name = s.SeasonName,
            Class = ClassForSeason(d.GraduationYear, s.SeasonName),
            Events = s.EventGroups.Count,
            Results = s.EventGroups.Sum(g => g.Performances.Count(p => p.HasMark)),
            PRs = s.EventGroups.Sum(g => g.Performances.Count(p => p.IsPersonalBest && p.PreviousBest != null)),
            SchoolRecords = s.SchoolRecordCount
        }).ToList();

        vm.Bests = d.PersonalRecords
            .OrderByDescending(r => r.Percentile ?? 0)
            .ThenBy(r => r.EventCategorySortOrder).ThenBy(r => r.EventSortOrder)
            .Take(MaxBests)
            .Select(r => new AthleteWrapBest
            {
                EventLabel = Label(r.EventName, r.Environment),
                Mark = r.Performance,
                MeetName = r.MeetName,
                Date = r.Date,
                Percentile = r.Percentile,
                AllTimeRank = r.AllTimeRank,
                IsSchoolRecord = r.IsSchoolRecord,
                IsRelay = r.IsRelay
            }).ToList();

        vm.Improvements = Improvements(d, all.Select(x => x.Group).Distinct().ToList(), all);
        return vm;
    }

    private static List<AthleteWrapImprovement> Improvements(
        AthleteDetailsViewModel d,
        List<EventPerformanceGroupViewModel> groups,
        List<(SeasonPerformanceViewModel Season, EventPerformanceGroupViewModel Group, IndividualPerformanceViewModel Perf)> all)
    {
        var changeByEvent = d.CareerCharts.ToDictionary(c => (c.EventId, c.Environment), c => c.CareerImprovementFormatted);
        var result = new List<AthleteWrapImprovement>();

        foreach (var byEvent in groups.GroupBy(g => (g.EventId, g.Environment)))
        {
            var marks = all
                .Where(x => x.Group.EventId == byEvent.Key.EventId && x.Group.Environment == byEvent.Key.Environment)
                .Where(x => !x.Perf.IsRelay && x.Perf.HasMark && x.Perf.RawValue.HasValue)
                .OrderBy(x => x.Perf.Date)
                .ToList();
            if (marks.Count < 2) continue;

            var isField = byEvent.First().IsFieldEvent;
            var first = marks.First();
            var best = isField ? marks.OrderByDescending(x => x.Perf.RawValue).First() : marks.OrderBy(x => x.Perf.RawValue).First();
            var gain = isField ? best.Perf.RawValue!.Value - first.Perf.RawValue!.Value : first.Perf.RawValue!.Value - best.Perf.RawValue!.Value;
            if (gain <= 0 || first.Perf.RawValue.Value <= 0) continue;

            changeByEvent.TryGetValue((byEvent.Key.EventId, byEvent.Key.Environment), out var change);
            result.Add(new AthleteWrapImprovement
            {
                EventLabel = Label(byEvent.First().EventName, byEvent.Key.Environment),
                First = first.Perf.Performance,
                Best = best.Perf.Performance,
                Change = change ?? string.Empty,
                Percent = gain / first.Perf.RawValue.Value * 100,
                Performances = marks.Count
            });
        }

        return result.OrderByDescending(i => i.Percent).Take(MaxImprovements).ToList();
    }

    /// <summary>Outdoor is the default; indoor events are marked so "800M" never means two things.</summary>
    public static string Label(string eventName, Environment environment) => environment == Environment.Indoor ? $"{eventName} (indoor)" : eventName;

    private static int? SeasonEndYear(string seasonName)
    {
        var parts = seasonName.Split('-');
        return parts.Length == 2 && int.TryParse(parts[1], out var y) ? y : null;
    }

    private static int SeasonSortKey(string seasonName) => SeasonEndYear(seasonName) ?? 0;

    private static string? ClassForSeason(int graduationYear, string seasonName) =>
        SeasonEndYear(seasonName) is { } y ? ClassYearCalculator.GetClassAtTimeOfPerformance(graduationYear, new DateTime(y, 3, 1)) : null;
}
