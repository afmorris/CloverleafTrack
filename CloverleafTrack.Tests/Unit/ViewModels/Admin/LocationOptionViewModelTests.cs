using CloverleafTrack.ViewModels.Admin.Meets;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Admin;

public class LocationOptionViewModelTests
{
    [Fact]
    public void DisplayText_IncludesCityAndState_WhenBothPresent()
    {
        var vm = new LocationOptionViewModel { Name = "Central Stadium", City = "Springfield", State = "IL" };
        vm.DisplayText.Should().Be("Central Stadium (Springfield, IL)");
    }

    [Fact]
    public void DisplayText_ReturnsNameOnly_WhenCityIsEmpty()
    {
        var vm = new LocationOptionViewModel { Name = "Central Stadium", City = "", State = "IL" };
        vm.DisplayText.Should().Be("Central Stadium");
    }

    [Fact]
    public void DisplayText_ReturnsNameOnly_WhenStateIsEmpty()
    {
        var vm = new LocationOptionViewModel { Name = "Central Stadium", City = "Springfield", State = "" };
        vm.DisplayText.Should().Be("Central Stadium");
    }

    [Fact]
    public void DisplayText_ReturnsNameOnly_WhenBothCityAndStateAreEmpty()
    {
        var vm = new LocationOptionViewModel { Name = "Unknown Venue" };
        vm.DisplayText.Should().Be("Unknown Venue");
    }
}
