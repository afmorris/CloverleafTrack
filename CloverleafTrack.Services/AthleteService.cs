using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;

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
        var participations = await repository.GetAllWithPerformancesAsync();
        var result = new Dictionary<EventCategory, List<AthleteViewModel>>();
        
        // Step 1: Build PR lookup using updated POCO
        var prLookup = participations
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
                            ? FormatDistance(best.Performance.DistanceInches.Value)
                            : "N/A";
                    }
                    else
                    {
                        var best = g
                            .Where(p => p.Performance.TimeSeconds.HasValue)
                            .OrderBy(p => p.Performance.TimeSeconds)
                            .FirstOrDefault();

                        return best != null
                            ? FormatTime(best.Performance.TimeSeconds.Value)
                            : "N/A";
                    }
                });

        // Step 2: Group by EventCategory
        var groupedByCategory = participations.GroupBy(x => x.Event.EventCategory);
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
                            
                            var pr = prLookup.TryGetValue(key, out var value)
                                ? value
                                : "N/A";

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
                        EventsInCategory = events
                    };
                })
                .OrderBy(x => x.FullName)
                .ToList();
            
            result[eventCategory.Value] = athletesInCategory;
        }

        return result;
    }
    
    public async Task<Dictionary<EventCategory, List<AthleteViewModel>>> GetFormerAthletesGroupedByEventCategoryAsync(int currentSeason)
    {
        var participations = await repository.GetAllWithPerformancesAsync();
        var result = new Dictionary<EventCategory, List<AthleteViewModel>>();
        
        // Step 1: Build PR lookup using updated POCO
        var prLookup = participations
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
                            ? FormatDistance(best.Performance.DistanceInches.Value)
                            : "N/A";
                    }
                    else
                    {
                        var best = g
                            .Where(p => p.Performance.TimeSeconds.HasValue)
                            .OrderBy(p => p.Performance.TimeSeconds)
                            .FirstOrDefault();

                        return best != null
                            ? FormatTime(best.Performance.TimeSeconds.Value)
                            : "N/A";
                    }
                });

        // Step 2: Group by EventCategory
        var groupedByCategory = participations.GroupBy(x => x.Event.EventCategory);
        foreach (var categoryGroup in groupedByCategory)
        {
            var eventCategory = categoryGroup.Key;

            var athletesInCategory = categoryGroup
                .Where(x => !x.Athlete.IsActive)
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
                            
                            var pr = prLookup.TryGetValue(key, out var value)
                                ? value
                                : "N/A";

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
                        Class = first.Athlete.GraduationYear.ToString(),
                        EventsInCategory = events
                    };
                })
                .OrderBy(x => x.FullName)
                .ToList();
            
            result[eventCategory.Value] = athletesInCategory;
        }

        return result;
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
            _ => "Graduate"
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