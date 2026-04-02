using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels;
using CloverleafTrack.ViewModels.Seasons;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Seasons;

public class SeasonCardViewModelTests
{
    // -------------------------------------------------------------------------
    // IndoorSchoolRecordCount
    // -------------------------------------------------------------------------

    [Fact]
    public void IndoorSchoolRecordCount_ReturnsZero_WhenListIsNull()
    {
        var vm = new SeasonCardViewModel { IndoorSchoolRecords = null };
        vm.IndoorSchoolRecordCount.Should().Be(0);
    }

    [Fact]
    public void IndoorSchoolRecordCount_ReturnsZero_WhenListIsEmpty()
    {
        var vm = new SeasonCardViewModel { IndoorSchoolRecords = new List<SchoolRecordViewModel>() };
        vm.IndoorSchoolRecordCount.Should().Be(0);
    }

    [Fact]
    public void IndoorSchoolRecordCount_ReturnsCount_WhenListHasItems()
    {
        var vm = new SeasonCardViewModel
        {
            IndoorSchoolRecords = new List<SchoolRecordViewModel> { new(), new(), new() }
        };
        vm.IndoorSchoolRecordCount.Should().Be(3);
    }

    // -------------------------------------------------------------------------
    // OutdoorSchoolRecordCount
    // -------------------------------------------------------------------------

    [Fact]
    public void OutdoorSchoolRecordCount_ReturnsZero_WhenListIsNull()
    {
        var vm = new SeasonCardViewModel { OutdoorSchoolRecords = null };
        vm.OutdoorSchoolRecordCount.Should().Be(0);
    }

    [Fact]
    public void OutdoorSchoolRecordCount_ReturnsZero_WhenListIsEmpty()
    {
        var vm = new SeasonCardViewModel { OutdoorSchoolRecords = new List<SchoolRecordViewModel>() };
        vm.OutdoorSchoolRecordCount.Should().Be(0);
    }

    [Fact]
    public void OutdoorSchoolRecordCount_ReturnsCount_WhenListHasItems()
    {
        var vm = new SeasonCardViewModel
        {
            OutdoorSchoolRecords = new List<SchoolRecordViewModel> { new(), new() }
        };
        vm.OutdoorSchoolRecordCount.Should().Be(2);
    }

    // -------------------------------------------------------------------------
    // StatusBadge
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(SeasonStatus.Draft,      "Draft")]
    [InlineData(SeasonStatus.Importing,  "Importing")]
    [InlineData(SeasonStatus.Partial,    "Partial")]
    [InlineData(SeasonStatus.RecordOnly, "RecordOnly")]
    [InlineData(SeasonStatus.Complete,   "Complete")]
    public void StatusBadge_ReturnsEnumName_ForAllStatuses(SeasonStatus status, string expected)
    {
        var vm = new SeasonCardViewModel { Status = status };
        vm.StatusBadge.Should().Be(expected);
    }
}
