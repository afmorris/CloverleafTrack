using CloverleafTrack.ViewModels.Leaderboard;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Leaderboard;

public class LeaderboardEventViewModelTests
{
    // -------------------------------------------------------------------------
    // RelayMembers — uses string.IsNullOrEmpty, unlike the |~|-split on Athletes
    // -------------------------------------------------------------------------

    [Fact]
    public void RelayMembers_ReturnsEmptyList_WhenRelayNameIsEmpty()
    {
        var vm = new LeaderboardEventViewModel { RelayName = "" };
        vm.RelayMembers.Should().BeEmpty();
    }

    [Fact]
    public void RelayMembers_ReturnsEmptyList_WhenRelayNameIsNull()
    {
        var vm = new LeaderboardEventViewModel();
        vm.RelayName = null!;
        vm.RelayMembers.Should().BeEmpty();
    }

    [Fact]
    public void RelayMembers_ParsesPipeSeparatedNames()
    {
        var vm = new LeaderboardEventViewModel
        {
            RelayName = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown"
        };
        vm.RelayMembers.Should().HaveCount(4);
        vm.RelayMembers.Should().ContainInOrder("Jane Doe", "Mary Smith", "Lisa Jones", "Sarah Brown");
    }

    [Fact]
    public void RelayMembers_HandlesSingleEntry()
    {
        var vm = new LeaderboardEventViewModel { RelayName = "Jane Doe" };
        vm.RelayMembers.Should().HaveCount(1);
        vm.RelayMembers[0].Should().Be("Jane Doe");
    }

    // -------------------------------------------------------------------------
    // AthleteFullName
    // -------------------------------------------------------------------------

    [Fact]
    public void AthleteFullName_ConcatenatesFirstAndLastName()
    {
        var vm = new LeaderboardEventViewModel { AthleteFirstName = "Jane", AthleteLastName = "Doe" };
        vm.AthleteFullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void AthleteFullName_PutsFirstNameBeforeLast()
    {
        var vm = new LeaderboardEventViewModel { AthleteFirstName = "Alice", AthleteLastName = "Smith" };
        vm.AthleteFullName.Should().StartWith("Alice");
    }
}
