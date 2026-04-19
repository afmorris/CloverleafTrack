using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Leaderboard;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

public class LeaderboardService(ILeaderboardRepository leaderboardRepository) : ILeaderboardService
{
    private readonly SlugHelper _slugHelper = new();

    public async Task<LeaderboardViewModel> GetLeaderboardAsync()
    {
        var allPerformances = await leaderboardRepository.GetTopPerformancePerEventAsync();

        var viewModel = new LeaderboardViewModel
        {
            BoysOutdoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Male && p.Environment == Environment.Outdoor).ToList()
            ),
            BoysIndoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Male && p.Environment == Environment.Indoor).ToList()
            ),
            GirlsOutdoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Outdoor).ToList()
            ),
            GirlsIndoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Indoor).ToList()
            ),
            MixedOutdoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Mixed && p.Environment == Environment.Outdoor).ToList()
            ),
            MixedIndoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Mixed && p.Environment == Environment.Indoor).ToList()
            )
        };

        return viewModel;
    }

    public async Task<LeaderboardDetailsViewModel?> GetLeaderboardDetailsAsync(string eventKey)
    {
        var allPerformances = await leaderboardRepository.GetAllPerformancesForEventAsync(eventKey);

        if (!allPerformances.Any())
        {
            return null;
        }

        // Get event info from first performance
        var firstPerf = allPerformances.First();
        var isFieldEvent = allPerformances.Any(p => p.DistanceInches.HasValue);

        // Compute school record progression in C#:
        // Walk performances in chronological order, tracking the running best.
        // Each time a new best is seen, that performance set the record at that moment.
        var recordSettingIds = new HashSet<int>();
        var progression = new List<SchoolRecordMomentViewModel>();
        double? runningBest = null;

        var chronological = allPerformances
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

        // Build all performances list with rankings, flagging record-setting rows
        var allPerfsList = allPerformances.Select((perf, index) => new LeaderboardPerformanceViewModel
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
            ClassAtTimeOfPerformance = GetClassAtTimeOfPerformance(perf.GraduationYear, perf.MeetDate),
        }).ToList();

        // Build overall PRs list (best per athlete across all classes)
        var prsOnly = BuildPrViewModels(
            allPerformances.Where(p => p.AthleteId.HasValue).ToList(),
            recordSettingIds);

        // Build class-specific PRs: best per athlete within each class
        var classPrs = new Dictionary<string, List<LeaderboardPerformanceViewModel>>();
        foreach (var cls in new[] { "Freshman", "Sophomore", "Junior", "Senior" })
        {
            var classPerfs = allPerformances
                .Where(p => p.AthleteId.HasValue &&
                             GetClassAtTimeOfPerformance(p.GraduationYear, p.MeetDate) == cls)
                .ToList();
            classPrs[cls] = BuildPrViewModels(classPerfs, recordSettingIds);
        }

        return new LeaderboardDetailsViewModel
        {
            EventId = firstPerf.EventId,
            EventName = firstPerf.EventName,
            EventKey = firstPerf.EventKey,
            Gender = firstPerf.Gender,
            Environment = firstPerf.Environment,
            IsFieldEvent = isFieldEvent,
            IsRelayEvent = allPerformances.Any(p => p.AthleteId == null),
            AllPerformances = allPerfsList,
            PersonalRecordsOnly = prsOnly,
            ClassPersonalRecords = classPrs,
            SchoolRecordProgression = sortedProgression,
        };
    }

    private List<LeaderboardCategoryViewModel> BuildCategoryViewModels(List<LeaderboardDto> performances)
    {
        // Separate relays from non-relays
        var relays = performances.Where(p => IsRelay(p.EventName)).ToList();
        var nonRelays = performances.Where(p => !IsRelay(p.EventName)).ToList();

        // Group relays by their type
        var runningRelays = relays.Where(p => IsRunningRelay(p.EventType)).ToList();
        var jumpRelays = relays.Where(p => IsJumpRelay(p.EventType)).ToList();
        var throwRelays = relays.Where(p => IsThrowsRelay(p.EventType)).ToList();

        var categories = new List<LeaderboardCategoryViewModel>();

        // 1. Sprints
        AddCategoryIfExists(categories, nonRelays, EventCategory.Sprints, "Sprints");

        // 2. Distance
        AddCategoryIfExists(categories, nonRelays, EventCategory.Distance, "Distance");

        // 3. Hurdles
        AddCategoryIfExists(categories, nonRelays, EventCategory.Hurdles, "Hurdles");

        // 4. Running Relays
        if (runningRelays.Any())
        {
            categories.Add(new LeaderboardCategoryViewModel
            {
                Category = EventCategory.Relays,
                CategoryName = "Running Relays",
                Events = BuildEventViewModels(runningRelays)
            });
        }

        // 5. Jumps
        AddCategoryIfExists(categories, nonRelays, EventCategory.Jumps, "Jumps");

        // 6. Jump Relays
        if (jumpRelays.Any())
        {
            categories.Add(new LeaderboardCategoryViewModel
            {
                Category = EventCategory.Relays,
                CategoryName = "Jump Relays",
                Events = BuildEventViewModels(jumpRelays)
            });
        }

        // 7. Throws
        AddCategoryIfExists(categories, nonRelays, EventCategory.Throws, "Throws");

        // 8. Throw Relays
        if (throwRelays.Any())
        {
            categories.Add(new LeaderboardCategoryViewModel
            {
                Category = EventCategory.Relays,
                CategoryName = "Throw Relays",
                Events = BuildEventViewModels(throwRelays)
            });
        }

        return categories;
    }

    private void AddCategoryIfExists(
        List<LeaderboardCategoryViewModel> categories,
        List<LeaderboardDto> performances,
        EventCategory category,
        string categoryName)
    {
        var categoryPerformances = performances
            .Where(p => p.EventCategory == category)
            .OrderBy(p => p.EventSortOrder)
            .ToList();

        if (categoryPerformances.Any())
        {
            categories.Add(new LeaderboardCategoryViewModel
            {
                Category = category,
                CategoryName = categoryName,
                Events = BuildEventViewModels(categoryPerformances)
            });
        }
    }

    private List<LeaderboardEventViewModel> BuildEventViewModels(List<LeaderboardDto> performances)
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
                AthleteFirstName = p.AthleteFirstName ?? "",
                AthleteLastName = p.AthleteLastName ?? "",
                RelayName = p.RelayName ?? "",
                Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                MeetDate = p.PerformanceId > 0 ? p.MeetDate : null,
                MeetName = p.MeetName ?? ""
            })
            .ToList();
    }

    private static bool IsRelay(string eventName)
    {
        return eventName.Contains("Relay", StringComparison.OrdinalIgnoreCase) ||
               eventName.Contains("4x", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRunningRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.RunningRelay;
    }

    private static bool IsJumpRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.JumpRelay;
    }

    private static bool IsThrowsRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.ThrowsRelay;
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
        HashSet<int> recordSettingIds)
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
                ClassAtTimeOfPerformance = GetClassAtTimeOfPerformance(perf.GraduationYear, perf.MeetDate),
            })
            .ToList();
    }

    /// <summary>
    /// Returns the athlete's class (Freshman/Sophomore/Junior/Senior) at the time the performance was set,
    /// based on the athlete's graduation year and the meet date. Returns null for relays or unknown grad years.
    /// School year boundary: August or later means the school year that ends in meetDate.Year + 1.
    /// </summary>
    private static string? GetClassAtTimeOfPerformance(int? graduationYear, DateTime meetDate)
    {
        if (!graduationYear.HasValue) return null;
        // Meets in August or later belong to the school year that ends the following June
        var schoolYearEnd = meetDate.Month >= 8 ? meetDate.Year + 1 : meetDate.Year;
        return (graduationYear.Value - schoolYearEnd) switch
        {
            0 => "Senior",
            1 => "Junior",
            2 => "Sophomore",
            3 => "Freshman",
            _ => null
        };
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