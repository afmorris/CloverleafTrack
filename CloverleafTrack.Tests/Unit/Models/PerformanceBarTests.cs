using CloverleafTrack.Models;
using CloverleafTrack.Services;
using CloverleafTrack.Web.Utilities;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Models;

public class PerformanceBarTests
{
    private static PerformanceBar Bar(int perf, double h, string o) => new() { PerformanceId = perf, HeightInches = h, Outcome = o };

    [Fact]
    public void BestClearedHeight_IsHighestBarWithAClear_IgnoringPassesAndFailures()
    {
        var bars = new[] { Bar(1, 48, "O"), Bar(1, 50, "P"), Bar(1, 52, "XXO"), Bar(1, 54, "XXX") };
        PerformanceBarSeries.BestClearedHeight(bars).Should().Be(52);
    }

    [Fact]
    public void BestClearedHeight_IsNull_ForNoHeight()
    {
        PerformanceBarSeries.BestClearedHeight(new[] { Bar(1, 48, "XXX") }).Should().BeNull();
    }

    [Theory]
    [InlineData("O", true)] [InlineData("XXO", true)] [InlineData("XXX", false)] [InlineData("P", false)] [InlineData("XP", false)]
    public void IsCleared_FollowsLastCharacter(string outcome, bool cleared) => Bar(1, 48, outcome).IsCleared.Should().Be(cleared);

    [Theory]
    [InlineData("O", true)] [InlineData("XXP", true)] [InlineData("OO", false)] [InlineData("XXXX", false)] [InlineData("", false)] [InlineData(null, false)]
    public void IsValidOutcome(string? outcome, bool valid) => PerformanceBarSeries.IsValidOutcome(outcome).Should().Be(valid);

    [Fact]
    public void Builder_GroupsByPerformance_OrdersByHeight_AndMarksBest()
    {
        var lookup = PerformanceBarSeriesBuilder.BuildLookup(new[]
        {
            Bar(1, 52, "XXO"), Bar(1, 48, "O"), Bar(1, 54, "XXX"), Bar(2, 60, "O")
        });

        lookup[1].Bars.Select(b => b.HeightInches).Should().Equal(48, 52, 54);
        lookup[1].Bars.Single(b => b.IsBest).HeightInches.Should().Be(52);
        lookup[2].Bars.Single().IsBest.Should().BeTrue();
        PerformanceBarSeriesBuilder.BuildLookup(new List<PerformanceBar>()).Should().BeEmpty();
    }

    [Fact]
    public void Parser_ReadsHeightsAndOutcomes_SortedByHeight()
    {
        var bars = BarLadderParser.Parse("4'4\" XXO\n4'0\" O; 4'2\" P", out var errors);
        errors.Should().BeEmpty();
        bars.Select(b => (b.HeightInches, b.Outcome)).Should().Equal((48.0, "O"), (50.0, "P"), (52.0, "XXO"));
    }

    [Fact]
    public void Parser_ReportsBadOutcomeHeightAndDuplicates()
    {
        BarLadderParser.Parse("4'0\" OO\nxyz O\n4'2\" O\n4'2\" XO\n4'4\"", out var errors);
        errors.Should().HaveCount(4);
    }

    [Fact]
    public void Parser_RoundTripsThroughFormat()
    {
        var bars = BarLadderParser.Parse("5'0\" O\n5'2\" XO\n5'4\" XXX", out _);
        BarLadderParser.Parse(BarLadderParser.Format(bars), out var errors).Select(b => b.Outcome).Should().Equal("O", "XO", "XXX");
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("High Jump", true)] [InlineData("Pole Vault", true)] [InlineData("Seated High Jump", true)]
    [InlineData("2-Person High Jump Relay", false)] [InlineData("Long Jump", false)] [InlineData(null, false)]
    public void IsVerticalJump(string? name, bool expected) => BarLadderParser.IsVerticalJump(name).Should().Be(expected);
}
