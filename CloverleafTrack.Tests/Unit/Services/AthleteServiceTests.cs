using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using Moq;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;

namespace CloverleafTrack.Tests.Unit.Services;

public class AthleteServiceTests
{
    private readonly Mock<IAthleteRepository> repository;
    private readonly IAthleteService service;

    public AthleteServiceTests()
    {
        repository = new Mock<IAthleteRepository>();
        service = new AthleteService(repository.Object);
    }

    [Fact]
    public async Task GetCurrentAthletesAsync_ReturnsOnlyCurrent()
    {
        var currentYear = DateTime.Now.Year;
        var athletes = new List<Athlete>
        {
            new() { Id = 1, FirstName = "A", LastName = "A", GraduationYear = currentYear },
            new() { Id = 2, FirstName = "B", LastName = "B", GraduationYear = currentYear + 1 },
            new() { Id = 3, FirstName = "C", LastName = "C", GraduationYear = currentYear - 1 }
        };
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(athletes);

        var result = await service.GetActiveAthletesAsync(currentYear);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.GraduationYear >= currentYear));
    }

    [Fact]
    public async Task GetGraduatedAthletesAsync_ReturnsOnlyGraduated()
    {
        var currentYear = DateTime.Now.Year;
        var athletes = new List<Athlete>
        {
            new() { Id = 1, FirstName = "A", LastName = "A", GraduationYear = currentYear - 1 },
            new() { Id = 2, FirstName = "B", LastName = "B", GraduationYear = currentYear }
        };
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(athletes);

        var result = await service.GetGraduatedAthletesAsync();

        Assert.Single(result);
        Assert.True(result[0].GraduationYear < currentYear);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullIfNotFound()
    {
        repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Athlete?)null);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNewId()
    {
        var vm = new AthleteViewModel { FirstName = "X", LastName = "Y", GraduationYear = 2025 };
        repository.Setup(r => r.CreateAsync(It.IsAny<Athlete>())).ReturnsAsync(42);

        var id = await service.CreateAsync(vm);

        Assert.Equal(42, id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue()
    {
        var vm = new AthleteViewModel { Id = 1, FirstName = "Edit", LastName = "User", GraduationYear = 2024 };
        repository.Setup(r => r.UpdateAsync(It.IsAny<Athlete>())).ReturnsAsync(true);

        var result = await service.UpdateAsync(vm);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Athlete?)null);

        var result = await service.DeleteAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        var entity = new Athlete { Id = 1, FirstName = "A", LastName = "B", GraduationYear = 2024 };
        repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        repository.Setup(r => r.DeleteAsync(entity)).ReturnsAsync(true);

        var result = await service.DeleteAsync(1);

        Assert.True(result);
    }
}