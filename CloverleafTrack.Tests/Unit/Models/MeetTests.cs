using CloverleafTrack.Models;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Models;

public class MeetTests
{
    [Fact]
    public void Slug_IsGeneratedFromName()
    {
        var meet = new Meet { Name = "Spring Invitational" };
        meet.Slug.Should().Be("spring-invitational");
    }

    [Theory]
    [InlineData("Regional Championships", "regional-championships")]
    [InlineData("Indoor Track Meet",      "indoor-track-meet")]
    [InlineData("5A State Finals",        "5a-state-finals")]
    public void Slug_LowercasesAndHyphenatesWords(string name, string expectedSlug)
    {
        var meet = new Meet { Name = name };
        meet.Slug.Should().Be(expectedSlug);
    }

    [Fact]
    public void Slug_StripsApostrophes()
    {
        // SlugHelper keeps periods but strips apostrophes
        var meet = new Meet { Name = "St. Mary's Invitational" };
        meet.Slug.Should().NotBeNullOrWhiteSpace();
        meet.Slug.Should().NotContain("'");
    }

    [Fact]
    public void ResultsUrl_FormatIsMeetsSlashSlug()
    {
        var meet = new Meet { Name = "Spring Invitational" };
        meet.ResultsUrl.Should().Be("/meets/spring-invitational");
    }

    [Fact]
    public void ResultsUrl_AlwaysStartsWithMeetsPrefix()
    {
        var meet = new Meet { Name = "Any Meet Name" };
        meet.ResultsUrl.Should().StartWith("/meets/");
    }

    [Fact]
    public void ResultsUrl_ContainsTheMeetSlug()
    {
        var meet = new Meet { Name = "Regional Championships" };
        meet.ResultsUrl.Should().Contain(meet.Slug);
    }
}
