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

    public async Task<List<AthleteViewModel>> GetGraduatedAthletesAsync()
    {
        var all = await repository.GetAllAsync();
        var currentYear = DateTime.Now.Year;
        return all
            .Where(a => a.GraduationYear < currentYear)
            .Select(MapToViewModel)
            .ToList();
    }

    public async Task<AthleteViewModel?> GetByIdAsync(int id)
    {
        var athlete = await repository.GetByIdAsync(id);
        return athlete is null ? null : MapToViewModel(athlete);
    }

    public async Task<Dictionary<EventCategory, List<AthleteViewModel>>> GetAthletesGroupedByEventCategoryAsync(int currentSeason)
    {
        var rawAthletes = await repository.GetAllWithPerformancesAsync();
    
        var result = new Dictionary<EventCategory, List<AthleteViewModel>>();

        foreach (var athlete in rawAthletes)
        {
            var eventGroups = athlete.EventParticipations
                .GroupBy(e => e.EventCategory);

            foreach (var group in eventGroups)
            {
                var category = group.Key;
                if (category != null)
                {
                    if (!result.ContainsKey(category.Value))
                        result[category.Value] = new List<AthleteViewModel>();

                    result[category.Value].Add(new AthleteViewModel
                    {
                        FirstName = athlete.FirstName,
                        LastName = athlete.LastName,
                        Class = GraduationYearToClass(athlete.GraduationYear, currentSeason),
                        EventsInCategory = group.Select(e => new EventParticipationViewModel
                        {
                            Name = e.Name,
                            PersonalRecord = "N/A" // Placeholder unless we hydrate it
                        }).ToList()
                    });
                }
            }
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
}