using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using Moq;

namespace CloverleafTrack.Tests.Unit.Services
{
    public class RosterServiceTests
    {
        private readonly Mock<IAthleteService> athleteService;
        private readonly IRosterService rosterService;

        public RosterServiceTests()
        {
            athleteService = new Mock<IAthleteService>();
            rosterService = new RosterService(athleteService.Object);
        }

        [Fact]
        public async Task GetRosterAsync_ReturnsCorrectAthletes()
        {
            // Arrange
            int currentSeason = 2025;
            var currentAthletes = new List<AthleteViewModel>
            {
                new() { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = currentSeason, Gender = Gender.Female }
            };
            var graduatedAthletes = new List<AthleteViewModel>
            {
                new() { Id = 2, FirstName = "John", LastName = "Smith", GraduationYear = currentSeason - 1, Gender = Gender.Male }
            };

            athleteService.Setup(s => s.GetActiveAthletesAsync(currentSeason)).ReturnsAsync(currentAthletes);
            athleteService.Setup(s => s.GetGraduatedAthletesAsync()).ReturnsAsync(graduatedAthletes);

            // Act
            var result = await rosterService.GetRosterAsync(currentSeason);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.ActiveAthletes);
            Assert.Single(result.GraduatedAthletes);
            Assert.Equal("Jane", result.ActiveAthletes[0].FirstName);
            Assert.Equal("John", result.GraduatedAthletes[0].FirstName);
        }
    }
}