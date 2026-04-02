using CloverleafTrack.ViewModels.Athletes;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Athletes;

public class IndividualPerformanceViewModelTests
{
    [Fact]
    public void IsRelay_ReturnsFalse_WhenRelayAthletesIsNull()
    {
        var vm = new IndividualPerformanceViewModel { RelayAthletes = null };
        vm.IsRelay.Should().BeFalse();
    }

    [Fact]
    public void IsRelay_ReturnsTrue_WhenRelayAthletesIsSet()
    {
        var vm = new IndividualPerformanceViewModel { RelayAthletes = "Jane Doe|~|Mary Smith" };
        vm.IsRelay.Should().BeTrue();
    }

    [Fact]
    public void RelayMembers_ReturnsEmptyArray_WhenRelayAthletesIsNull()
    {
        var vm = new IndividualPerformanceViewModel { RelayAthletes = null };
        vm.RelayMembers.Should().BeEmpty();
    }

    [Fact]
    public void RelayMembers_ParsesPipeSeparatedNames()
    {
        var vm = new IndividualPerformanceViewModel
        {
            RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown"
        };
        vm.RelayMembers.Should().HaveCount(4);
        vm.RelayMembers.Should().ContainInOrder("Jane Doe", "Mary Smith", "Lisa Jones", "Sarah Brown");
    }

    [Fact]
    public void RelayMembers_HandlesSingleMember()
    {
        var vm = new IndividualPerformanceViewModel { RelayAthletes = "Jane Doe" };
        vm.RelayMembers.Should().HaveCount(1);
        vm.RelayMembers[0].Should().Be("Jane Doe");
    }
}

public class PersonalRecordViewModelTests
{
    [Fact]
    public void IsRelay_ReturnsFalse_WhenRelayAthletesIsNull()
    {
        var vm = new PersonalRecordViewModel { RelayAthletes = null };
        vm.IsRelay.Should().BeFalse();
    }

    [Fact]
    public void IsRelay_ReturnsTrue_WhenRelayAthletesIsSet()
    {
        var vm = new PersonalRecordViewModel { RelayAthletes = "Jane Doe|~|Mary Smith" };
        vm.IsRelay.Should().BeTrue();
    }

    [Fact]
    public void RelayMembers_ReturnsEmptyArray_WhenRelayAthletesIsNull()
    {
        var vm = new PersonalRecordViewModel { RelayAthletes = null };
        vm.RelayMembers.Should().BeEmpty();
    }

    [Fact]
    public void RelayMembers_ParsesPipeSeparatedNames()
    {
        var vm = new PersonalRecordViewModel
        {
            RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown"
        };
        vm.RelayMembers.Should().HaveCount(4);
        vm.RelayMembers.Should().ContainInOrder("Jane Doe", "Mary Smith", "Lisa Jones", "Sarah Brown");
    }
}
