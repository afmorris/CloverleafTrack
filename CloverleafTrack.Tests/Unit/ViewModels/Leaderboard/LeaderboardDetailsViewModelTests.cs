using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Leaderboard;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Leaderboard;

public class LeaderboardDetailsViewModelTests
{
    [Theory]
    [InlineData(Gender.Male, "Boys")]
    [InlineData(Gender.Female, "Girls")]
    [InlineData(Gender.Mixed, "Mixed")]
    public void GenderLabel_MapsKnownGenders(Gender gender, string expected)
    {
        var vm = new LeaderboardDetailsViewModel { Gender = gender };

        vm.GenderLabel.Should().Be(expected);
    }

    [Fact]
    public void GenderLabel_ReturnsUnknown_ForUnexpectedGenderValue()
    {
        var vm = new LeaderboardDetailsViewModel { Gender = (Gender)99 };

        vm.GenderLabel.Should().Be("Unknown");
    }
}
