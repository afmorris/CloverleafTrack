using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Leaderboard;
using CloverleafTrack.ViewModels.Shared;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

public class LeaderboardService(
    ILeaderboardRepository leaderboardRepository,
    ISeasonRepository seasonRepository,
    IPerformanceAttemptRepository? attemptRepository = null) : ILeaderboardService
{
    private static readonly string[] Classes = { "Freshman", "Sophomore", "Junior", "Senior" };

    private readonly SlugHelper _slugHelper = new();

    public async Task<LeaderboardViewModel> GetLeaderboardAsync()
    {
        var all = await leaderboardRepository.GetTopPerformancePerEventAsync();

        return new LeaderboardViewModel
        {
            BoysOutdoor   = BuildEventList(all.Where(p => p.Gender == Gender.Male   && p.Environment == Environment.Outdoor)),
            BoysIndoor    = BuildEventList(all.Where(p => p.Gender == Gender.Male   && p.Environment == Environment.Indoor)),
            GirlsOutdoor  = BuildEventList(all.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Outdoor)),
            GirlsIndoor   = BuildEventList(all.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Indoor)),
            MixedOutdoor  = BuildEventList(all.Where(p => p.Gender == Gender.Mixed  && p.Environment == Environment.Outdoor)),
            MixedIndoor   = BuildEventList(all.Where(p => p.Gender == Gender.Mixed  && p.Environment == Environment.Indoor)),
        };
    }

    public async Task<LeaderboardDetailsViewModel?> GetLeaderboardDetailsAsync(string eventKey, string? scope = "all-time", int depth = 25)
    {
        var allSeasons = await seasonRepository.GetAllAsync();
        var currentSeason = allSeasons.FirstOrDefault(s => s.IsCurrentSeason);

        // Resolve the scope string into a concrete SeasonId filter (null = all-time), and into a
        // normalized ScopeValue for the view. NOTE: ScopeValue is deliberately rebuilt from the
        // validated pieces below (never the raw `scope` argument) because the view interpolates
        // it directly into an inline <script> string literal — echoing the raw, attacker-controlled
        // query string there would be a reflected-XSS hole (Razor's default @-encoding is
        // HTML-safe, not JS-string-safe). Keep ScopeValue restricted to "all-time" | "season" |
        // "season-{digits}" for that reason — do not change it back to echoing `scope` verbatim.
        int? seasonId = null;
        string resolvedScopeValue = "all-time";
        if (string.Equals(scope, "season", StringComparison.OrdinalIgnoreCase) && currentSeason != null)
        {
            seasonId = currentSeason.Id;
            resolvedScopeValue = "season";
        }
        else if (scope != null &&
                 scope.StartsWith("season-", StringComparison.OrdinalIgnoreCase) &&
                 int.TryParse(scope["season-".Length..], out var parsedSeasonId))
        {
            seasonId = parsedSeasonId;
            resolvedScopeValue = $"season-{parsedSeasonId}";
        }

        var scopedPerformances = await leaderboardRepository.GetAllPerformancesForEventAsync(eventKey, seasonId);

        // A season scope can legitimately produce zero rows for an event that otherwise exists
        // (e.g. the event wasn't run that season). Only 404 when the event has never had ANY
        // performance recorded (all-time), not just none in the requested scope.
        var metadataSource = scopedPerformances;
        if (!metadataSource.Any())
        {
            metadataSource = seasonId == null
                ? scopedPerformances
                : await leaderboardRepository.GetAllPerformancesForEventAsync(eventKey, null);

            if (!metadataSource.Any())
            {
                return null;
            }
        }

        var firstPerf = metadataSource.First();
        var isFieldEvent = metadataSource.Any(p => p.DistanceInches.HasValue);
        var isRelayEvent = metadataSource.Any(p => p.AthleteId == null);

        // Compute school record progression in C#, from the scoped performance set:
        // Walk performances in chronological order, tracking the running best.
        // Each time a new best is seen, that performance set the record at that moment.
        var recordSettingIds = new HashSet<int>();
        var progression = new List<SchoolRecordMomentViewModel>();
        double? runningBest = null;

        var chronological = scopedPerformances
            .OrderBy(p => p.MeetDate)
            .ThenBy(p => isFieldEvent
                ? -(p.DistanceInches ?? 0)          // on same day, best distance first
                : (p.TimeSeconds ?? double.MaxValue)) // on same day, fastest time first
            .ToList();

        foreach (var perf in chronological)
        {
            var value = isFieldEvent ? perf.DistanceInches : perf.TimeSeconds;
            if (!value.HasValue) continue;

            var isNewRecord = runningBest == null ||
                              (isFieldEvent ? value > runningBest : value < runningBest);

            if (!isNewRecord) continue;

            var improvement = runningBest.HasValue
                ? FormatImprovement(Math.Abs(value.Value - runningBest.Value), isFieldEvent)
                : null;

            recordSettingIds.Add(perf.PerformanceId);
            progression.Add(new SchoolRecordMomentViewModel
            {
                AthleteName = perf.AthleteId.HasValue
                    ? $"{perf.AthleteFirstName} {perf.AthleteLastName}"
                    : perf.RelayName,
                AthleteSlug = perf.AthleteId.HasValue
                    ? _slugHelper.GenerateSlug($"{perf.AthleteFirstName}-{perf.AthleteLastName}")
                    : "",
                GraduationYear = perf.GraduationYear,
                Performance = FormatPerformance(perf.TimeSeconds, perf.DistanceInches),
                RawValue = value.Value,
                MeetName = perf.MeetName,
                MeetSlug = _slugHelper.GenerateSlug(perf.MeetName),
                MeetDate = perf.MeetDate,
                ImprovementFormatted = improvement,
            });

            runningBest = value;
        }

        if (progression.Any())
            progression.Last().IsCurrentRecord = true;

        // Sort for display: best mark first (distance desc / time asc)
        var sortedProgression = isFieldEvent
            ? progression.OrderByDescending(p => p.RawValue).ToList()
            : progression.OrderBy(p => p.RawValue).ToList();

        // Build a lookup: PerformanceId → attempt series (empty/absent for performances with no recorded series)
        var attempts = attemptRepository != null
            ? await attemptRepository.GetAttemptsForPerformancesAsync(scopedPerformances.Select(p => p.PerformanceId))
            : new List<PerformanceAttempt>();
        var attemptLookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        // Depth is applied AFTER scope (already applied via the SQL SeasonId filter above) and
        // AFTER class filtering (each class-specific list below is built from its own filtered
        // subset, then depth-limited independently) — see BRAIN.md "Event Page Depth" entry.
        var effectiveDepth = depth <= 0 ? int.MaxValue : depth;

        // Build the "all classes" performances list with rankings, flagging record-setting rows.
        var allPerfsListFull = BuildAllPerformanceViewModels(scopedPerformances, recordSettingIds, isFieldEvent, attemptLookup);
        var totalPerformanceCount = allPerfsListFull.Count;
        var allPerfsList = allPerfsListFull.Take(effectiveDepth).ToList();

        // Build class-specific "all performances" lists — each is ranked 1..N within its own
        // class subset (not sliced out of the overall top N), then depth-limited independently.
        var classAllPerfs = new Dictionary<string, List<LeaderboardPerformanceViewModel>>();
        foreach (var cls in Classes)
        {
            var classDtos = scopedPerformances
                .Where(p => p.AthleteId.HasValue && ClassYearCalculator.GetClassAtTimeOfPerformance(p.GraduationYear, p.MeetDate) == cls)
                .ToList();
            classAllPerfs[cls] = BuildAllPerformanceViewModels(classDtos, recordSettingIds, isFieldEvent, attemptLookup)
                .Take(effectiveDepth)
                .ToList();
        }

        // Build overall PRs list (best per athlete across all classes), depth-limited.
        var prsOnly = BuildPrViewModels(
                scopedPerformances.Where(p => p.AthleteId.HasValue).ToList(),
                recordSettingIds,
                isFieldEvent,
                attemptLookup)
            .Take(effectiveDepth)
            .ToList();

        // Build class-specific PRs: best per athlete within each class, depth-limited.
        var classPrs = new Dictionary<string, List<LeaderboardPerformanceViewModel>>();
        foreach (var cls in Classes)
        {
            var classPerfs = scopedPerformances
                .Where(p => p.AthleteId.HasValue &&
                             ClassYearCalculator.GetClassAtTimeOfPerformance(p.GraduationYear, p.MeetDate) == cls)
                .ToList();
            classPrs[cls] = BuildPrViewModels(classPerfs, recordSettingIds, isFieldEvent, attemptLookup)
                .Take(effectiveDepth)
                .ToList();
        }

        var seasonOptions = allSeasons
            .Where(s => !s.IsCurrentSeason)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SeasonFilterOptionViewModel { Id = s.Id, Name = s.Name })
            .ToList();

        return new LeaderboardDetailsViewModel
        {
            EventId = firstPerf.EventId,
            EventName = firstPerf.EventName,
            EventKey = firstPerf.EventKey,
            Gender = firstPerf.Gender,
            Environment = firstPerf.Environment,
            IsFieldEvent = isFieldEvent,
            IsRelayEvent = isRelayEvent,
            AllPerformances = allPerfsList,
            ClassAllPerformances = classAllPerfs,
            PersonalRecordsOnly = prsOnly,
            ClassPersonalRecords = classPrs,
            SchoolRecordProgression = sortedProgression,
            ScopeValue = resolvedScopeValue,
            DepthValue = depth <= 0 ? 0 : depth,
            TotalPerformanceCount = totalPerformanceCount,
            CurrentSeasonId = currentSeason?.Id,
            SeasonOptions = seasonOptions,
        };
    }

    /// <summary>
    /// Builds ranked (1..N) view models for a set of performances, in the order they were
    /// supplied (already best-first from the repository query). Used for the "All Performances"
    /// view — both the overall list and each class-specific subset.
    /// </summary>
    private List<LeaderboardPerformanceViewModel> BuildAllPerformanceViewModels(
        IEnumerable<LeaderboardPerformanceDto> perfs,
        HashSet<int> recordSettingIds,
        bool isFieldEvent,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        return perfs.Select((perf, index) => new LeaderboardPerformanceViewModel
        {
            Rank = index + 1,
            AthleteName = perf.AthleteId.HasValue
                ? $"{perf.AthleteFirstName} {perf.AthleteLastName}"
                : perf.RelayName,
            AthleteSlug = perf.AthleteId.HasValue
                ? _slugHelper.GenerateSlug($"{perf.AthleteFirstName}-{perf.AthleteLastName}")
                : "",
            Performance = FormatPerformance(perf.TimeSeconds, perf.DistanceInches),
            MeetName = perf.MeetName,
            MeetSlug = _slugHelper.GenerateSlug(perf.MeetName),
            MeetDate = perf.MeetDate,
            GraduationYear = perf.GraduationYear,
            IsSchoolRecord = perf.AllTimeRank == 1,
            WasRecordAtTime = recordSettingIds.Contains(perf.PerformanceId),
            ClassAtTimeOfPerformance = ClassYearCalculator.GetClassAtTimeOfPerformance(perf.GraduationYear, perf.MeetDate),
            RawValue = isFieldEvent ? perf.DistanceInches : perf.TimeSeconds,
            Percentile = perf.Percentile,
            AttemptSeries = attemptLookup.GetValueOrDefault(perf.PerformanceId) ?? new PerformanceAttemptSeriesViewModel()
        }).ToList();
    }

    private List<LeaderboardEventViewModel> BuildEventList(IEnumerable<LeaderboardDto> performances)
    {
        return performances
            .OrderBy(p => p.EventSortOrder)
            .Select(p => new LeaderboardEventViewModel
            {
                EventId = p.EventId,
                EventName = p.EventName,
                EventKey = p.EventKey,
                Gender = p.Gender,
                Environment = p.Environment,
                SortOrder = p.EventSortOrder,
                AthleteFirstName = p.AthleteFirstName ?? "",
                AthleteLastName = p.AthleteLastName ?? "",
                RelayName = p.RelayName ?? "",
                Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                MeetDate = p.PerformanceId > 0 ? p.MeetDate : null,
                MeetName = p.MeetName ?? ""
            })
            .ToList();
    }

    private static string FormatPerformance(double? timeSeconds, double? distanceInches)
    {
        if (timeSeconds.HasValue)
        {
            return FormatTime(timeSeconds.Value);
        }
        else if (distanceInches.HasValue)
        {
            return FormatDistance(distanceInches.Value);
        }
        else
        {
            return string.Empty;
        }
    }

    private List<LeaderboardPerformanceViewModel> BuildPrViewModels(
        List<LeaderboardPerformanceDto> individualPerfs,
        HashSet<int> recordSettingIds,
        bool isFieldEvent,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        return individualPerfs
            .GroupBy(p => p.AthleteId)
            .Select(g => g.First()) // already ordered best-first by the query
            .Select((perf, index) => new LeaderboardPerformanceViewModel
            {
                Rank = index + 1,
                AthleteName = $"{perf.AthleteFirstName} {perf.AthleteLastName}",
                AthleteSlug = _slugHelper.GenerateSlug($"{perf.AthleteFirstName}-{perf.AthleteLastName}"),
                Performance = FormatPerformance(perf.TimeSeconds, perf.DistanceInches),
                MeetName = perf.MeetName,
                MeetSlug = _slugHelper.GenerateSlug(perf.MeetName),
                MeetDate = perf.MeetDate,
                GraduationYear = perf.GraduationYear,
                IsSchoolRecord = perf.AllTimeRank == 1,
                WasRecordAtTime = recordSettingIds.Contains(perf.PerformanceId),
                ClassAtTimeOfPerformance = ClassYearCalculator.GetClassAtTimeOfPerformance(perf.GraduationYear, perf.MeetDate),
                RawValue = isFieldEvent ? perf.DistanceInches : perf.TimeSeconds,
                Percentile = perf.Percentile,
                AttemptSeries = attemptLookup.GetValueOrDefault(perf.PerformanceId) ?? new PerformanceAttemptSeriesViewModel()
            })
            .ToList();
    }

    private static string FormatImprovement(double delta, bool isField)
    {
        if (isField)
        {
            var feet = (int)(delta / 12);
            var inches = delta % 12;
            return feet > 0
                ? $"+{feet}' {inches:0.##}\""
                : $"+{inches:0.##}\"";
        }

        var ts = TimeSpan.FromSeconds(delta);
        return ts.TotalMinutes >= 1
            ? $"-{ts:m\\:ss\\.ff}"
            : $"-{delta:0.00}s";
    }

    private static string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }

    private static string FormatTime(double seconds)
    {
        if (seconds >= 60)
        {
            var minutes = (int)(seconds / 60);
            var remainder = seconds % 60;
            return $"{minutes}:{remainder:00.00}";
        }

        return $"{seconds:0.00}";
    }
}