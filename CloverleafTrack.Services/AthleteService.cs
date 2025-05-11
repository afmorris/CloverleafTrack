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

        var groupedByCategory = participations.GroupBy(x => x.Event.EventCategory);
        foreach (var categoryGroup in groupedByCategory)
        {
            var eventCategory = categoryGroup.Key;
            var athletesInCategory = categoryGroup
                .Where(x => x.Athlete.GraduationYear >= currentSeason)
                .GroupBy(x => x.Athlete.Id)
                .Select(x =>
                {
                    var first = x.First();
                    return new AthleteViewModel
                    {
                        FirstName = first.Athlete.FirstName,
                        LastName = first.Athlete.LastName,
                        Class = GraduationYearToClass(first.Athlete.GraduationYear, currentSeason),
                        EventsInCategory = x.Select(z => new EventParticipationViewModel
                        {
                            Id = z.Event.Id,
                            Name = z.Event.Name,
                            PersonalRecord = "N/A",
                            Environment = z.Event.Environment,
                            SortOrder = z.Event.SortOrder
                        }).DistinctBy(e => e.Id)
                            .ToList()
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
}