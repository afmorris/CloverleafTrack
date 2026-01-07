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

                    if (first.Event.EventCategory is EventCategory.Throws or EventCategory.Jumps)
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

        // Step 2: Group by EventCategory
        var groupedByCategory = athletesWithPerformances.GroupBy(x => x.Event.EventCategory);
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

        // Get personal records (best performance per event)
        var personalRecords = performances
            .Where(p => p.PersonalBest)
            .GroupBy(p => p.EventId)
            .Select(g => g.OrderByDescending(p => p.MeetDate).First())
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
                EventSortOrder = p.EventSortOrder
            })
            .OrderBy(pr => pr.Environment)
            .ThenBy(pr => pr.EventCategorySortOrder)
            .ThenBy(pr => pr.EventSortOrder)
            .ToList();

        // Get top events for hero section
        var topSprintEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder <= 30) // Sprints, Distance, Hurdles
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .FirstOrDefault();

        var topFieldEvent = personalRecords
            .Where(pr => pr.EventCategorySortOrder >= 40) // Jumps, Throws
            .OrderBy(pr => pr.AllTimeRank ?? 999)
            .FirstOrDefault();

        // Group by season
        var seasons = performances
            .GroupBy(p => p.SeasonName)
            .Select(seasonGroup => new SeasonPerformanceViewModel
            {
                SeasonName = seasonGroup.Key,
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

                        return new EventPerformanceGroupViewModel
                        {
                            EventName = eventGroup.Key.EventName,
                            Environment = eventGroup.Key.Environment,
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
                                    AllTimeRank = p.AllTimeRank
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

            // Hero stats
            TopSprintEvent = topSprintEvent != null ? new AthleteTopEventViewModel
            {
                EventName = topSprintEvent.EventName,
                Performance = topSprintEvent.Performance,
                AllTimeRank = topSprintEvent.AllTimeRank
            } : null,
            TopFieldEvent = topFieldEvent != null ? new AthleteTopEventViewModel
            {
                EventName = topFieldEvent.EventName,
                Performance = topFieldEvent.Performance,
                AllTimeRank = topFieldEvent.AllTimeRank
            } : null,
            TotalPRs = performances.Count(p => p.PersonalBest),
            TotalSchoolRecords = performances.Count(p => p.SchoolRecord),

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