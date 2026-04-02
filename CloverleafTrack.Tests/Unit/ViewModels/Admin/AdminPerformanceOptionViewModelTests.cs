using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Admin.Performances;
using FluentAssertions;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.ViewModels.Admin;

public class AthleteOptionViewModelTests
{
    [Fact]
    public void DisplayText_FormatsAsLastNameCommaFirstNameAndYear()
    {
        var vm = new AthleteOptionViewModel { FirstName = "Jane", LastName = "Doe", GraduationYear = 2025 };
        vm.DisplayText.Should().Be("Doe, Jane (2025)");
    }

    [Fact]
    public void DisplayText_PutsLastNameFirst()
    {
        var vm = new AthleteOptionViewModel { FirstName = "Alice", LastName = "Smith", GraduationYear = 2026 };
        vm.DisplayText.Should().StartWith("Smith");
    }
}

public class EventOptionViewModelTests
{
    [Fact]
    public void DisplayText_IncludesEventNameAndGender()
    {
        var vm = new EventOptionViewModel { Name = "100m", Gender = Gender.Female };
        vm.DisplayText.Should().Be("100m (Female)");
    }

    [Fact]
    public void DisplayText_WorksForMaleEvents()
    {
        var vm = new EventOptionViewModel { Name = "Shot Put", Gender = Gender.Male };
        vm.DisplayText.Should().Be("Shot Put (Male)");
    }

    [Fact]
    public void CategoryName_ReturnsEnumName_WhenCategorySet()
    {
        var vm = new EventOptionViewModel { EventCategory = EventCategory.Sprints };
        vm.CategoryName.Should().Be("Sprints");
    }

    [Fact]
    public void CategoryName_ReturnsOther_WhenCategoryIsNull()
    {
        var vm = new EventOptionViewModel { EventCategory = null };
        vm.CategoryName.Should().Be("Other");
    }

    [Theory]
    [InlineData(EventCategory.Sprints,  "Sprints")]
    [InlineData(EventCategory.Distance, "Distance")]
    [InlineData(EventCategory.Hurdles,  "Hurdles")]
    [InlineData(EventCategory.Jumps,    "Jumps")]
    [InlineData(EventCategory.Throws,   "Throws")]
    [InlineData(EventCategory.Relays,   "Relays")]
    public void CategoryName_ReturnsCorrectLabel_ForAllCategories(EventCategory category, string expected)
    {
        var vm = new EventOptionViewModel { EventCategory = category };
        vm.CategoryName.Should().Be(expected);
    }
}

public class MeetOptionViewModelTests
{
    [Fact]
    public void DisplayText_IncludesNameDateAndEnvironment()
    {
        var vm = new MeetOptionViewModel
        {
            Name = "Spring Invitational",
            Date = new DateTime(2024, 3, 15),
            Environment = Environment.Outdoor
        };
        vm.DisplayText.Should().Be("Spring Invitational - Mar 15, 2024 (Outdoor)");
    }

    [Fact]
    public void DisplayText_ShowsIndoor_ForIndoorMeet()
    {
        var vm = new MeetOptionViewModel
        {
            Name = "Winter Classic",
            Date = new DateTime(2024, 1, 20),
            Environment = Environment.Indoor
        };
        vm.DisplayText.Should().Contain("Indoor");
    }

    [Fact]
    public void DisplayText_FormatsDateWithFullMonthName()
    {
        var vm = new MeetOptionViewModel
        {
            Name = "Meet",
            Date = new DateTime(2024, 11, 5),
            Environment = Environment.Outdoor
        };
        vm.DisplayText.Should().Contain("Nov 5, 2024");
    }
}
