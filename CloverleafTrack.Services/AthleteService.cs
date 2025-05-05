using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
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
}