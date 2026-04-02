using CloverleafTrack.ViewModels.Admin.Dashboard;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Admin;

public class SeasonProgressViewModelTests
{
    [Fact]
    public void PercentComplete_ReturnsZero_WhenTotalMeetsIsZero()
    {
        // Guard against divide-by-zero
        var vm = new SeasonProgressViewModel { TotalMeets = 0, EnteredMeets = 0 };
        vm.PercentComplete.Should().Be(0);
    }

    [Fact]
    public void PercentComplete_ReturnsZero_WhenNoMeetsEntered()
    {
        var vm = new SeasonProgressViewModel { TotalMeets = 5, EnteredMeets = 0 };
        vm.PercentComplete.Should().Be(0);
    }

    [Fact]
    public void PercentComplete_Returns100_WhenAllMeetsEntered()
    {
        var vm = new SeasonProgressViewModel { TotalMeets = 8, EnteredMeets = 8 };
        vm.PercentComplete.Should().Be(100);
    }

    [Theory]
    [InlineData(10, 3,  30)]
    [InlineData(10, 5,  50)]
    [InlineData(10, 10, 100)]
    [InlineData(4,  1,  25)]
    public void PercentComplete_ReturnsCorrectPercentage(int total, int entered, int expected)
    {
        var vm = new SeasonProgressViewModel { TotalMeets = total, EnteredMeets = entered };
        vm.PercentComplete.Should().Be(expected);
    }

    [Fact]
    public void PercentComplete_TruncatesRemainder_NotRounds()
    {
        // 1 of 3 = 33.33...% — integer division yields 33, not 34
        var vm = new SeasonProgressViewModel { TotalMeets = 3, EnteredMeets = 1 };
        vm.PercentComplete.Should().Be(33);
    }

    [Fact]
    public void PercentComplete_TruncatesRemainder_TwoThirds()
    {
        // 2 of 3 = 66.66...% — integer division yields 66, not 67
        var vm = new SeasonProgressViewModel { TotalMeets = 3, EnteredMeets = 2 };
        vm.PercentComplete.Should().Be(66);
    }
}
