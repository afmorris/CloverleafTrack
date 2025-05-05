using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using Moq;

namespace CloverleafTrack.Tests.Unit.Services;

public class SeasonServiceTests
{
    private readonly Mock<ISeasonRepository> repository;
    private readonly ISeasonService service;

    public SeasonServiceTests()
    {
        repository = new Mock<ISeasonRepository>();
        service = new SeasonService(repository.Object);
    }

    [Fact]
    public async Task GetCurrentSeason_ReturnsCorrectSeasonId()
    {
        // Arrange
        var today = DateTime.Today;
        var seasons = new List<Season>
        {
            new() { Id = 1, Name = "2022-2023", StartDate = today.AddYears(-2), EndDate = today.AddYears(-1), IsCurrentSeason = false},
            new() { Id = 2, Name = "2023-2024", StartDate = today.AddMonths(-1), EndDate = today.AddMonths(1), IsCurrentSeason = false},
            new() { Id = 3, Name = "2024-2025", StartDate = today.AddYears(1), EndDate = today.AddYears(2), IsCurrentSeason = true}
        };

        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(seasons);

        // Act
        var result = await service.GetCurrentSeasonAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetCurrentSeason_ThrowsException_WhenNoCurrentSeason()
    {
        // Arrange
        var today = DateTime.Today;
        var seasons = new List<Season>
        {
            new() { Id = 1, Name = "Past Season", StartDate = today.AddYears(-5), EndDate = today.AddYears(-4), IsCurrentSeason = false },
            new() { Id = 2, Name = "Future Season", StartDate = today.AddYears(2), EndDate = today.AddYears(3), IsCurrentSeason = false }
        };

        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(seasons);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetCurrentSeasonAsync());
        Assert.Equal("No current season found.", ex.Message);
    }
}