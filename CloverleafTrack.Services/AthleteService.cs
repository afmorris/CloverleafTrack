using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using CloverleafTrack.ViewModels.Athletes;

namespace CloverleafTrack.Services;

public class AthleteService(IAthleteRepository repository) : IAthleteService
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
                EventCategorySortOrder = p.EventCategorySortOrder,
                EventSortOrder = p.EventSortOrder,
                IsSchoolRecord = p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1,
                RelayAthletes = p.RelayAthletes
            })
            .OrderBy(pr => pr.Environment)
            .ThenBy(pr => pr.EventCategorySortOrder)
            .ThenBy(pr => pr.EventSortOrder)
            .ToList();

        // Get top events for hero section (individual events only)
        var topSprintEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder <= 30) // Sprints, Distance, Hurdles
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .FirstOrDefault();

        var topFieldEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder >= 40) // Jumps, Throws
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .FirstOrDefault();

        // Group by season (ordered most recent first)
        var seasons = performances
            .GroupBy(p => new { p.SeasonName, p.SeasonStartDate })
            .OrderByDescending(g => g.Key.SeasonStartDate)
            .Select(seasonGroup => new SeasonPerformanceViewModel
            {
                SeasonName = seasonGroup.Key.SeasonName,
                PRCount = seasonGroup.Count(p => p.PersonalBest),
                SchoolRecordCount = seasonGroup.Count(p => p.SchoolRecord),
                EventGroups = seasonGroup
                    .GroupBy(p => new { p.EventId, p.EventName, p.EventCategorySortOrder, p.EventSortOrder, p.Environment })
                    .OrderBy(eg => eg.Key.EventCategorySortOrder)
                    .ThenBy(eg => eg.Key.EventSortOrder)
                    .Select(eventGroup =>
                    {
                        var prPerformance = eventGroup
                            .Where(p => p.PersonalBest)
                            .OrderByDescending(p => p.MeetDate)
                            .FirstOrDefault();

                        var isFieldEvent = eventGroup.First().DistanceInches.HasValue;
                        return new EventPerformanceGroupViewModel
                        {
                            EventId = eventGroup.Key.EventId,
                            EventName = eventGroup.Key.EventName,
                            Environment = eventGroup.Key.Environment,
                            IsFieldEvent = isFieldEvent,
                            PersonalRecordPerformance = prPerformance != null
                                ? FormatPerformance(prPerformance.TimeSeconds, prPerformance.DistanceInches)
                                : "",
                            PersonalRecordDate = prPerformance?.MeetDate ?? DateTime.MinValue,
                            Performances = eventGroup
                                .OrderByDescending(p => p.MeetDate)
                                .Select(p => new IndividualPerformanceViewModel
                                {
                                    Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                                    Date = p.MeetDate,
                                    MeetName = p.MeetName,
                                    IsPersonalBest = p.PersonalBest,
                                    IsSchoolRecord = p.SchoolRecord,
                                    IsSeasonBest = p.SeasonBest,
                                    AllTimeRank = p.AllTimeRank,
                                    RawValue = p.DistanceInches ?? p.TimeSeconds,
                                    RelayAthletes = p.RelayAthletes
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
            Seasons = seasons
        };
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