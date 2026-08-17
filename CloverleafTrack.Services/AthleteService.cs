using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using CloverleafTrack.ViewModels.Athletes;
using CloverleafTrack.ViewModels.Shared;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

public class AthleteService(
    IAthleteRepository repository,
    IPerformanceAttemptRepository? attemptRepository = null) : IAthleteService
{
    public async Task<List<AthleteViewModel>> GetActiveAthletesAsync(int currentSeason)
    {
        var all = await repository.GetAllAsync();
        return all
            .Where(a => a.GraduationYear >= currentSeason)
            .Select(MapToViewModel)
            .ToList();
    }

    public async Task<List<AthleteViewModel>> GetGraduatedAthletesAsync(int currentSeason)
    {
        var all = await repository.GetAllAsync();
        return all
            .Where(a => a.GraduationYear < currentSeason)
            .Select(MapToViewModel)
            .ToList();
    }

    public async Task<AthleteViewModel?> GetByIdAsync(int id)
    {
        var athlete = await repository.GetByIdAsync(id);
        return athlete is null ? null : MapToViewModel(athlete);
    }

    public async Task<Dictionary<EventCategory, List<AthleteViewModel>>> GetActiveAthletesGroupedByEventCategoryAsync(int currentSeason)
    {
        var athletesWithPerformances = await repository.GetAllWithPerformancesAsync();
        var result = new Dictionary<EventCategory, List<AthleteViewModel>>();

        // Step 1: Build PR lookup using updated POCO
        var prLookup = athletesWithPerformances
            .GroupBy(p => (p.Athlete.Id, p.Event.Id))
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();

                    if (IsDistanceBasedEvent(first.Event))
                    {
                        var best = g
                            .Where(p => p.Performance.DistanceInches.HasValue)
                            .OrderByDescending(p => p.Performance.DistanceInches)
                            .FirstOrDefault();

                        return best != null
                            ? FormatDistance(best.Performance.DistanceInches!.Value)
                            : "N/A";
                    }
                    else
                    {
                        var best = g
                            .Where(p => p.Performance.TimeSeconds.HasValue)
                            .OrderBy(p => p.Performance.TimeSeconds)
                            .FirstOrDefault();

                        return best != null
                            ? FormatTime(best.Performance.TimeSeconds!.Value)
                            : "N/A";
                    }
                });

        // Step 2: Group by roster category (relay events mapped to their equivalent individual category)
        var groupedByCategory = athletesWithPerformances.GroupBy(x => GetRosterCategory(x.Event));
        foreach (var categoryGroup in groupedByCategory)
        {
            var eventCategory = categoryGroup.Key;

            var athletesInCategory = categoryGroup
                .Where(x => x.Athlete.IsActive)
                .GroupBy(x => x.Athlete.Id)
                .Select(x =>
                {
                    var first = x.First();

                    var events = x
                        .GroupBy(e => e.Event.Id)
                        .Select(g =>
                        {
                            var ev = g.First().Event;
                            var key = (first.Athlete.Id, ev.Id);

                            var pr = prLookup.GetValueOrDefault(key, "N/A");

                            return new EventParticipationViewModel
                            {
                                Id = ev.Id,
                                Name = ev.Name,
                                Environment = ev.Environment,
                                SortOrder = ev.SortOrder,
                                PersonalRecord = pr
                            };
                        })
                        .OrderBy(e => e.SortOrder)
                        .ToList();

                    return new AthleteViewModel
                    {
                        FirstName = first.Athlete.FirstName,
                        LastName = first.Athlete.LastName,
                        Class = GraduationYearToClass(first.Athlete.GraduationYear, currentSeason),
                        EventsInCategory = events,
                        Gender = first.Athlete.Gender,
                        GraduationYear = first.Athlete.GraduationYear
                    };
                })
                .OrderBy(x => x.FullName)
                .ToList();

            if (athletesInCategory.Any())
            {
                result[eventCategory!.Value] = athletesInCategory;
            }
        }

        return result;
    }

    public async Task<List<AthleteViewModel>> GetFlatActiveAthletesAsync(int currentSeason)
    {
        var athletesWithPerformances = await repository.GetAllWithPerformancesAsync();

        var prLookup = athletesWithPerformances
            .GroupBy(p => (p.Athlete.Id, p.Event.Id))
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();
                    if (IsDistanceBasedEvent(first.Event))
                    {
                        var best = g.Where(p => p.Performance.DistanceInches.HasValue)
                            .OrderByDescending(p => p.Performance.DistanceInches)
                            .FirstOrDefault();
                        return best != null ? FormatDistance(best.Performance.DistanceInches!.Value) : "N/A";
                    }
                    else
                    {
                        var best = g.Where(p => p.Performance.TimeSeconds.HasValue)
                            .OrderBy(p => p.Performance.TimeSeconds)
                            .FirstOrDefault();
                        return best != null ? FormatTime(best.Performance.TimeSeconds!.Value) : "N/A";
                    }
                });

        return athletesWithPerformances
            .Where(x => x.Athlete.IsActive)
            .GroupBy(x => x.Athlete.Id)
            .Select(g =>
            {
                var first = g.First();

                var events = g.GroupBy(e => e.Event.Id)
                    .Select(eg =>
                    {
                        var ev = eg.First().Event;
                        var key = (first.Athlete.Id, ev.Id);
                        return new EventParticipationViewModel
                        {
                            Id = ev.Id,
                            Name = ev.Name,
                            Environment = ev.Environment,
                            SortOrder = ev.SortOrder,
                            PersonalRecord = prLookup.GetValueOrDefault(key, "N/A")
                        };
                    })
                    .OrderBy(e => e.SortOrder)
                    .ToList();

                var categories = g.Select(x => GetRosterCategory(x.Event))
                    .Where(c => c.HasValue)
                    .Select(c => c!.Value)
                    .Distinct()
                    .ToList();

                return new AthleteViewModel
                {
                    FirstName = first.Athlete.FirstName,
                    LastName = first.Athlete.LastName,
                    Class = GraduationYearToClass(first.Athlete.GraduationYear, currentSeason),
                    EventsInCategory = events,
                    Categories = categories,
                    Gender = first.Athlete.Gender,
                    GraduationYear = first.Athlete.GraduationYear
                };
            })
            .OrderBy(a => a.FullName)
            .ToList();
    }

    public async Task<Dictionary<int, List<AthleteViewModel>>> GetFormerAthletesGroupedByGraduationYearAsync()
    {
        var athletesWithPerformances = await repository.GetAllWithPerformancesAsync();

        var inactiveAthletes = athletesWithPerformances.Where(x => !x.Athlete.IsActive).ToList();

        var prLookup = inactiveAthletes
            .GroupBy(x => (x.Athlete.Id, x.Event.Id))
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    var first = x.First();

                    if (first.Event.EventCategory is EventCategory.Throws or EventCategory.Jumps)
                    {
                        var bestDistance = x
                            .Where(p => p.Performance.DistanceInches.HasValue)
                            .OrderByDescending(p => p.Performance.DistanceInches)
                            .FirstOrDefault();

                        return bestDistance != null
                            ? FormatDistance(bestDistance.Performance.DistanceInches!.Value)
                            : "N/A";
                    }

                    var bestTime = x
                        .Where(p => p.Performance.TimeSeconds.HasValue)
                        .OrderBy(p => p.Performance.TimeSeconds)
                        .FirstOrDefault();

                    return bestTime != null ? FormatTime(bestTime.Performance.TimeSeconds!.Value) : "N/A";
                });

        var groupedByGradYear = inactiveAthletes
            .GroupBy(x => x.Athlete.GraduationYear)
            .OrderByDescending(x => x.Key)
            .ToDictionary(
                x => x.Key,
                x => x
                    .GroupBy(p => p.Athlete.Id)
                    .Select(athleteGroup =>
                    {
                        var first = athleteGroup.First();
                        var eventGroups = athleteGroup
                            .GroupBy(e => e.Event.Id)
                            .Select(eventGroup =>
                            {
                                var ev = eventGroup.First().Event;
                                var key = (first.Athlete.Id, ev.Id);
                                var pr = prLookup.GetValueOrDefault(key, "N/A");

                                return new EventParticipationViewModel
                                {
                                    Id = ev.Id,
                                    Name = ev.Name,
                                    Environment = ev.Environment,
                                    SortOrder = ev.SortOrder,
                                    PersonalRecord = pr
                                };
                            })
                            .OrderBy(e => e.SortOrder)
                            .ToList();

                        return new AthleteViewModel
                        {
                            FirstName = first.Athlete.FirstName,
                            LastName = first.Athlete.LastName,
                            Class = $"Class of {first.Athlete.GraduationYear}",
                            EventsInCategory = eventGroups,
                            Gender = first.Athlete.Gender,
                            GraduationYear = first.Athlete.GraduationYear
                        };
                    })
                    .OrderBy(a => a.FullName)
                    .ToList()
            );

        return groupedByGradYear;
    }


    public async Task<int> CreateAsync(AthleteViewModel viewModel)
    {
        var entity = MapToEntity(viewModel);
        return await repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(AthleteViewModel viewModel)
    {
        var entity = MapToEntity(viewModel);
        return await repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var athlete = await repository.GetByIdAsync(id);
        if (athlete is null) return false;
        return await repository.DeleteAsync(athlete);
    }

    public async Task<AthleteDetailsViewModel?> GetAthleteDetailsAsync(string slug, int currentSeason)
    {
        // Get athlete basic info
        var athlete = await repository.GetBySlugWithBasicInfoAsync(slug);
        if (athlete == null)
        {
            return null;
        }

        // Get all performances
        var performances = await repository.GetAllPerformancesForAthleteAsync(athlete.Id);

        if (!performances.Any())
        {
            // Return athlete with no performances
            return new AthleteDetailsViewModel
            {
                AthleteId = athlete.Id,
                FirstName = athlete.FirstName,
                LastName = athlete.LastName,
                GraduationYear = athlete.GraduationYear,
                Gender = athlete.Gender,
                Class = GraduationYearToClass(athlete.GraduationYear, currentSeason)
            };
        }

        // Individual PRs — rely on the PersonalBest flag
        var individualPRs = performances
            .Where(p => p.PersonalBest && p.RelayAthletes == null)
            .GroupBy(p => p.EventId)
            .Select(g => g.OrderByDescending(p => p.MeetDate).First());

        // Relay bests — compute best per relay event (fastest time / farthest distance)
        // since the PersonalBest flag is not reliably set on relay performances
        var relayBests = performances
            .Where(p => p.RelayAthletes != null)
            .GroupBy(p => p.EventId)
            .Select(g => g.First().TimeSeconds.HasValue
                ? g.OrderBy(p => p.TimeSeconds).First()
                : g.OrderByDescending(p => p.DistanceInches).First());

        var personalRecords = individualPRs
            .Concat(relayBests)
            .Select(p => new PersonalRecordViewModel
            {
                EventId = p.EventId,
                EventName = p.EventName,
                Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                Environment = p.Environment,
                Date = p.MeetDate,
                MeetName = p.MeetName,
                AllTimeRank = p.AllTimeRank,
                Percentile = p.Percentile,
                EventMarkCount = p.EventMarkCount,
                EventCategorySortOrder = p.EventCategorySortOrder,
                EventSortOrder = p.EventSortOrder,
                IsSchoolRecord = p.AllTimeRank == 1,
                RelayAthletes = p.RelayAthletes
            })
            .OrderBy(pr => pr.Environment)
            .ThenBy(pr => pr.EventCategorySortOrder)
            .ThenBy(pr => pr.EventSortOrder)
            .ToList();

        // Get top events for hero section (individual events only).
        // AllTimeRank only exists for the true top 10 all-time in an event, so it ties at the
        // "?? 999" fallback for the vast majority of athletes — Percentile (populated for nearly
        // every performance) is the tiebreaker that actually reflects which event is genuinely
        // their best, rather than falling back to personalRecords' structural sort order. See
        // BRAIN.md [C36] / issue #48.
        var topSprintEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder <= 30) // Sprints, Distance, Hurdles
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .ThenByDescending(pr => pr.Percentile ?? 0)
            .FirstOrDefault();

        var topFieldEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder >= 40) // Jumps, Throws
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .ThenByDescending(pr => pr.Percentile ?? 0)
            .FirstOrDefault();

        // Build a lookup: PerformanceId → attempt series (empty/absent for performances with no recorded series)
        var attempts = attemptRepository != null
            ? await attemptRepository.GetAttemptsForPerformancesAsync(performances.Select(p => p.PerformanceId))
            : new List<PerformanceAttempt>();
        var attemptLookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        // Group by season (ordered most recent first)
        var seasons = performances
            .GroupBy(p => new { p.SeasonName, p.SeasonStartDate })
            .OrderByDescending(g => g.Key.SeasonStartDate)
            .Select(seasonGroup => new SeasonPerformanceViewModel
            {
                SeasonName = seasonGroup.Key.SeasonName,
                PRCount = seasonGroup.Count(p => p.PersonalBest),
                SchoolRecordCount = seasonGroup.Count(p => p.AllTimeRank == 1),
                EventGroups = seasonGroup
                    .GroupBy(p => new { p.EventId, p.EventName, p.EventCategorySortOrder, p.EventSortOrder, p.Environment })
                    .OrderBy(eg => eg.Key.EventCategorySortOrder)
                    .ThenBy(eg => eg.Key.EventSortOrder)
                    .Select(eventGroup =>
                    {
                        var isFieldEvent = eventGroup.First().DistanceInches.HasValue;

                        // Season best (used for accordion summary label + Δ column baseline)
                        var seasonBest = isFieldEvent
                            ? eventGroup.Where(p => p.DistanceInches.HasValue).OrderByDescending(p => p.DistanceInches).FirstOrDefault()
                            : eventGroup.Where(p => p.TimeSeconds.HasValue).OrderBy(p => p.TimeSeconds).FirstOrDefault();

                        return new EventPerformanceGroupViewModel
                        {
                            EventId = eventGroup.Key.EventId,
                            EventName = eventGroup.Key.EventName,
                            Environment = eventGroup.Key.Environment,
                            IsFieldEvent = isFieldEvent,
                            PersonalRecordPerformance = seasonBest != null
                                ? FormatPerformance(seasonBest.TimeSeconds, seasonBest.DistanceInches)
                                : "",
                            PersonalRecordDate = seasonBest?.MeetDate ?? DateTime.MinValue,
                            PersonalRecordRawValue = isFieldEvent ? seasonBest?.DistanceInches : seasonBest?.TimeSeconds,
                            Performances = eventGroup
                                .OrderByDescending(p => p.MeetDate)
                                .Select(p => new IndividualPerformanceViewModel
                                {
                                    Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                                    Date = p.MeetDate,
                                    MeetName = p.MeetName,
                                    IsPersonalBest = p.PersonalBest,
                                    IsSchoolRecord = p.AllTimeRank == 1,
                                    IsSeasonBest = p.SeasonBest,
                                    AllTimeRank = p.AllTimeRank,
                                    RawValue = p.DistanceInches ?? p.TimeSeconds,
                                    RelayAthletes = p.RelayAthletes,
                                    AttemptSeries = attemptLookup.GetValueOrDefault(p.PerformanceId) ?? new PerformanceAttemptSeriesViewModel()
                                })
                                .ToList()
                        };
                    })
                    .ToList()
            })
            .ToList();

        return new AthleteDetailsViewModel
        {
            AthleteId = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            GraduationYear = athlete.GraduationYear,
            Gender = athlete.Gender,
            Class = GraduationYearToClass(athlete.GraduationYear, currentSeason),

            // Hero stats (individual events only)
            TopSprintEvent = topSprintEvent != null ? new AthleteTopEventViewModel
            {
                EventName = topSprintEvent.EventName,
                Performance = topSprintEvent.Performance,
                AllTimeRank = topSprintEvent.AllTimeRank,
                Environment = topSprintEvent.Environment
            } : null,
            TopFieldEvent = topFieldEvent != null ? new AthleteTopEventViewModel
            {
                EventName = topFieldEvent.EventName,
                Performance = topFieldEvent.Performance,
                AllTimeRank = topFieldEvent.AllTimeRank,
                Environment = topFieldEvent.Environment
            } : null,
            TotalPRs = performances.Count(p => p.PersonalBest && p.RelayAthletes == null),
            TotalSchoolRecords = performances
                    .Where(p => p.AllTimeRank == 1)
                    .Select(p => p.EventId)
                    .Distinct()
                    .Count(),

            PersonalRecords = personalRecords,
            Seasons = seasons,
            CareerCharts = await BuildCareerCharts(performances, athlete.GraduationYear)
        };
    }

    /// <summary>
    /// Builds one career progression chart per event the athlete has performances in (issue #26).
    /// Y-axis inversion and domain math live in CareerChartGeometry (unit-tested directly) —
    /// this method only gathers the values and calls it; it never reimplements the mapping.
    /// </summary>
    private async Task<List<CareerChartViewModel>> BuildCareerCharts(List<AthletePerformanceDto> performances, int graduationYear)
    {
        const double plotLeft = 68, plotRight = 580, plotTop = 20, plotBottom = 220;

        var eventIds = performances.Select(p => p.EventId).Distinct().ToList();
        var records = await repository.GetSchoolRecordsForEventsAsync(eventIds) ?? new List<EventRecordDto>();
        var recordsByEvent = records.ToDictionary(r => r.EventId);

        var charts = new List<CareerChartViewModel>();

        foreach (var group in performances.GroupBy(p => p.EventId))
        {
            var eventPerfs = group.OrderBy(p => p.MeetDate).ToList();
            var first = eventPerfs[0];
            var isFieldEvent = first.DistanceInches.HasValue;
            var isRelay = eventPerfs.Any(p => p.RelayAthletes != null);

            double RawValue(AthletePerformanceDto p) => (isFieldEvent ? p.DistanceInches : p.TimeSeconds) ?? 0;
            bool IsBetter(double a, double b) => isFieldEvent ? a > b : a < b;

            var careerBest = eventPerfs.OrderBy(p => isFieldEvent ? -RawValue(p) : RawValue(p)).First();
            var domainValues = eventPerfs.Select(RawValue).ToList();

            // Record territory — sourced from Leaderboards.Rank = 1 via a dedicated query, never
            // from the stale Performances.SchoolRecord flag. Suppressed if the athlete already
            // holds it (the "how much air is left" zone is meaningless once you ARE the record).
            recordsByEvent.TryGetValue(group.Key, out var recordDto);
            double? recordValue = recordDto == null ? null : (isFieldEvent ? recordDto.DistanceInches : recordDto.TimeSeconds);
            var athleteHoldsRecord = eventPerfs.Any(p => p.AllTimeRank == 1);
            var showRecordZone = recordValue.HasValue && !athleteHoldsRecord;
            if (showRecordZone) domainValues.Add(recordValue!.Value);

            // Program median/IQR — suppressed for relays (unstable population) and events with
            // fewer than ~10 program marks, matching EventStatistics' own NULL-below-10 rule.
            var markCount = first.EventMarkCount ?? 0;
            var showMedianBand = !isRelay && markCount >= 10 && first.MedianValue.HasValue && first.Q1Value.HasValue && first.Q3Value.HasValue;
            if (showMedianBand)
            {
                domainValues.Add(first.Q1Value!.Value);
                domainValues.Add(first.Q3Value!.Value);
            }

            var (domainMin, domainMax) = CareerChartGeometry.ComputeDomain(domainValues);

            double PixelY(double raw) => CareerChartGeometry.MapValueToPixelY(raw, domainMin, domainMax, plotTop, plotBottom, isFieldEvent);

            // X positions: evenly spaced by chronological order, NOT true calendar-date spacing.
            // Date-proportional spacing was the original design (see BRAIN.md [C39]) but in
            // practice competitions cluster into short in-season windows separated by months-long
            // off-seasons, so true date scaling crushed every meaningful point into a few dense
            // clusters and wasted most of the plot width on empty gaps — confirmed against a real
            // chart, not just reasoned about. Class-year ticks still land correctly because they're
            // derived from each point's already-computed PixelX below, not recomputed separately.
            double PixelX(int index) => eventPerfs.Count == 1
                ? (plotLeft + plotRight) / 2
                : plotLeft + (double)index / (eventPerfs.Count - 1) * (plotRight - plotLeft);

            var points = eventPerfs.Select((p, index) => new CareerChartPointViewModel
            {
                PixelX = PixelX(index),
                PixelY = PixelY(RawValue(p)),
                Formatted = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                Date = p.MeetDate,
                ClassAtTime = ClassYearCalculator.GetClassAtTimeOfPerformance(graduationYear, p.MeetDate),
                IsCareerBest = p.PerformanceId == careerBest.PerformanceId
            }).ToList();

            // Class-year ticks: position at the first performance (in plotted order) for each
            // class actually represented, rather than computing exact August-boundary dates —
            // every tick this way is anchored to a real data point, never extrapolated past the
            // plotted range. Abbreviation is an explicit map, not a [..2] substring — "Junior"
            // and "Senior" do NOT start with "Jr"/"Sr" (that bug shipped once already; verified
            // against a real screenshot showing "Ju"/"Se" before this fix).
            var classTicks = points
                .Where(p => p.ClassAtTime != null)
                .GroupBy(p => p.ClassAtTime)
                .Select(g => g.OrderBy(p => p.Date).First())
                .OrderBy(p => p.Date)
                .Select(p => new ClassYearTickViewModel { PixelX = p.PixelX, Label = ClassAbbreviation(p.ClassAtTime!) })
                .ToList();

            var yTicks = new List<CareerChartYTickViewModel>();
            for (var i = 0; i <= 4; i++)
            {
                var raw = domainMin + (domainMax - domainMin) * i / 4.0;
                // Gridline position uses the exact fractional value; the LABEL is rounded to a
                // shorter form (whole inch / one decimal second) so it fits in the left margin
                // without clipping — full-precision labels like `9' 4.82"` were wide enough to
                // overflow past x=0 and get clipped by the SVG viewport (confirmed via screenshot).
                var roundedForLabel = isFieldEvent ? Math.Round(raw) : Math.Round(raw, 1);
                yTicks.Add(new CareerChartYTickViewModel
                {
                    PixelY = PixelY(raw),
                    Label = FormatPerformance(isFieldEvent ? null : roundedForLabel, isFieldEvent ? roundedForLabel : null),
                    HiddenOnMobile = i == 1 || i == 3
                });
            }

            var bestRaw = RawValue(careerBest);
            var firstRaw = RawValue(eventPerfs[0]);
            string? improvementFormatted = eventPerfs.Count > 1 && IsBetter(bestRaw, firstRaw)
                ? FormatDelta(Math.Abs(bestRaw - firstRaw), isFieldEvent, improvement: true)
                : null;
            string? deltaOffRecordFormatted = showRecordZone
                ? FormatDelta(Math.Abs(recordValue!.Value - bestRaw), isFieldEvent, improvement: false)
                : null;

            charts.Add(new CareerChartViewModel
            {
                EventId = group.Key,
                EventName = first.EventName,
                Environment = first.Environment,
                IsFieldEvent = isFieldEvent,
                IsRelay = isRelay,
                Points = points,
                ClassTicks = classTicks,
                YTicks = yTicks,
                ShowRecordZone = showRecordZone,
                RecordFormatted = recordValue.HasValue ? FormatPerformance(isFieldEvent ? null : recordValue, isFieldEvent ? recordValue : null) : null,
                RecordLinePixelY = showRecordZone ? PixelY(recordValue!.Value) : null,
                RecordZoneTopPixelY = showRecordZone ? plotTop : null,
                RecordZoneBottomPixelY = showRecordZone ? PixelY(recordValue!.Value) : null,
                ShowMedianBand = showMedianBand,
                MedianFormatted = showMedianBand ? FormatPerformance(isFieldEvent ? null : first.MedianValue, isFieldEvent ? first.MedianValue : null) : null,
                MedianLinePixelY = showMedianBand ? PixelY(first.MedianValue!.Value) : null,
                IqrZoneTopPixelY = showMedianBand ? Math.Min(PixelY(first.Q1Value!.Value), PixelY(first.Q3Value!.Value)) : null,
                IqrZoneBottomPixelY = showMedianBand ? Math.Max(PixelY(first.Q1Value!.Value), PixelY(first.Q3Value!.Value)) : null,
                CareerBestFormatted = FormatPerformance(careerBest.TimeSeconds, careerBest.DistanceInches),
                CareerImprovementFormatted = improvementFormatted,
                BestPercentile = careerBest.Percentile,
                DeltaOffRecordFormatted = deltaOffRecordFormatted,
                PlotLeft = plotLeft,
                PlotRight = plotRight,
                PlotTop = plotTop,
                PlotBottom = plotBottom
            });
        }

        // Group same-named indoor/outdoor pairs adjacent (e.g. both "Shot Put" charts sit next
        // to each other in the tab bar, Outdoor first per the sitewide Outdoor-first convention),
        // rather than scattering them wherever raw point count happened to sort them — the whole
        // point of showing the Environment on the tab is to disambiguate the pair, which doesn't
        // help if they're nowhere near each other. Groups themselves are still ordered by total
        // significance (most active event group first).
        return charts
            .GroupBy(c => c.EventName)
            .OrderByDescending(g => g.Sum(c => c.Points.Count))
            .SelectMany(g => g.OrderBy(c => c.Environment == Environment.Outdoor ? 0 : 1))
            .ToList();
    }

    /// <summary>"Freshman"/"Sophomore"/"Junior"/"Senior" → "Fr"/"So"/"Jr"/"Sr". An explicit map, not a [..2] substring — "Junior"[..2] is "Ju", not "Jr".</summary>
    private static string ClassAbbreviation(string className) => className switch
    {
        "Freshman" => "Fr",
        "Sophomore" => "So",
        "Junior" => "Jr",
        "Senior" => "Sr",
        _ => className
    };

    /// <summary>Formats an unsigned magnitude delta with an explicit sign — "+2' 6.25&quot;"/"-0.43s" for an improvement, "2' 6.25&quot;"/"0.43s" (no sign) for a gap-to-record.</summary>
    private static string FormatDelta(double delta, bool isField, bool improvement)
    {
        var sign = improvement ? "+" : "";
        if (isField)
        {
            var feet = (int)(delta / 12);
            var inches = delta % 12;
            return feet > 0 ? $"{sign}{feet}' {inches:0.##}\"" : $"{sign}{inches:0.##}\"";
        }

        var ts = TimeSpan.FromSeconds(delta);
        return ts.TotalMinutes >= 1 ? $"{sign}{ts:m\\:ss\\.ff}" : $"{sign}{delta:0.00}s";
    }

    private string FormatPerformance(double? timeSeconds, double? distanceInches)
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
            return "N/A";
        }
    }

    private AthleteViewModel MapToViewModel(Athlete a) => new()
    {
        Id = a.Id,
        FirstName = a.FirstName,
        LastName = a.LastName,
        GraduationYear = a.GraduationYear,
        Gender = a.Gender
    };

    private Athlete MapToEntity(AthleteViewModel vm) => new()
    {
        Id = vm.Id,
        FirstName = vm.FirstName,
        LastName = vm.LastName,
        GraduationYear = vm.GraduationYear,
        Gender = vm.Gender
    };

    private string GraduationYearToClass(int gradYear, int currentSeason)
    {
        var diff = gradYear - currentSeason;

        return diff switch
        {
            >= 3 => "Freshman",
            2 => "Sophomore",
            1 => "Junior",
            0 => "Senior",
            _ => $"{gradYear} Graduate"
        };
    }

    /// <summary>
    /// Maps an event to the EventCategory used to bucket athletes on the roster page.
    /// Relay events are remapped to their individual-event equivalent so that, for example,
    /// a 4×100m participant appears in Sprints rather than Relays.
    /// </summary>
    private static EventCategory? GetRosterCategory(Event ev) => ev.EventType switch
    {
        EventType.JumpRelay   => EventCategory.Jumps,
        EventType.ThrowsRelay => EventCategory.Throws,
        EventType.FieldRelay  => EventCategory.Throws,
        EventType.RunningRelay => MapRunningRelayCategory(ev.Name),
        _                      => ev.EventCategory   // individual events: use stored category
    };

    private static EventCategory MapRunningRelayCategory(string eventName)
    {
        var name = eventName.ToLowerInvariant();
        return (name.Contains("distance medley") || name.Contains("dmr") ||
                name.Contains("800") || name.Contains("1500") || name.Contains("1600") ||
                name.Contains("mile") || name.Contains("2000") || name.Contains("3200"))
            ? EventCategory.Distance
            : EventCategory.Sprints;
    }

    /// <summary>
    /// Returns true when the event's PR is measured in distance (inches) rather than time.
    /// Handles relay event types explicitly because their EventCategory is Relays, not Throws/Jumps.
    /// </summary>
    private static bool IsDistanceBasedEvent(Event ev) => ev.EventType switch
    {
        EventType.Field        => ev.EventCategory is EventCategory.Throws or EventCategory.Jumps,
        EventType.FieldRelay   => true,
        EventType.JumpRelay    => true,
        EventType.ThrowsRelay  => true,
        _                      => false  // Running and RunningRelay use time
    };

    private string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }

    private string FormatTime(double seconds)
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