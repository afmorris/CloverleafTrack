using CloverleafTrack.ViewModels.Meets;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Meets;

public class MeetListItemViewModelTests
{
    [Fact]
    public void IsUpcoming_ReturnsTrue_WhenDateIsInFuture()
    {
        var vm = new MeetListItemViewModel { Date = DateTime.Now.AddDays(1) };
        vm.IsUpcoming.Should().BeTrue();
    }

    [Fact]
    public void IsUpcoming_ReturnsFalse_WhenDateIsInPast()
    {
        var vm = new MeetListItemViewModel { Date = DateTime.Now.AddDays(-1) };
        vm.IsUpcoming.Should().BeFalse();
    }

    [Fact]
    public void IsUpcoming_ReturnsFalse_WhenDateIsFarInPast()
    {
        var vm = new MeetListItemViewModel { Date = new DateTime(2020, 1, 1) };
        vm.IsUpcoming.Should().BeFalse();
    }

    [Fact]
    public void IsUpcoming_ReturnsTrue_WhenDateIsFarInFuture()
    {
        var vm = new MeetListItemViewModel { Date = DateTime.Now.AddYears(1) };
        vm.IsUpcoming.Should().BeTrue();
    }
}
