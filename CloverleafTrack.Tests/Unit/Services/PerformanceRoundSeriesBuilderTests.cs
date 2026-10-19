using CloverleafTrack.Models;
using CloverleafTrack.Services;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Services;

public class PerformanceRoundSeriesBuilderTests
{
    [Fact]
    public void BuildLookup_Empty_ForNoRounds()
    {
        PerformanceRoundSeriesBuilder.BuildLookup(new List<PerformanceRound>()).Should().BeEmpty();
    }

    [Fact]
    public void BuildLookup_OrdersRounds_AndMarksFastestAsBest()
    {
        var lookup = PerformanceRoundSeriesBuilder.BuildLookup(new List<PerformanceRound>
        {
            new() { PerformanceId = 1, Round = 3, TimeSeconds = 10.94 },
            new() { PerformanceId = 1, Round = 1, TimeSeconds = 11.08 },
        });

        var series = lookup[1];
        series.Rounds.Select(r => r.Round).Should().Equal(1, 3);
        series.Rounds.Single(r => r.IsBest).Round.Should().Be(3);
        series.HasRounds.Should().BeTrue();
    }

    [Fact]
    public void BuildLookup_MarksPrelimBest_WhenFinalWasSlower()
    {
        var series = PerformanceRoundSeriesBuilder.BuildLookup(new List<PerformanceRound>
        {
            new() { PerformanceId = 1, Round = 1, TimeSeconds = 11.21 },
            new() { PerformanceId = 1, Round = 3, TimeSeconds = 11.30 },
        })[1];

        series.Rounds.Single(r => r.IsBest).Round.Should().Be(1);
    }

    [Fact]
    public void BuildLookup_Tie_MarksLaterRoundBest()
    {
        var series = PerformanceRoundSeriesBuilder.BuildLookup(new List<PerformanceRound>
        {
            new() { PerformanceId = 1, Round = 1, TimeSeconds = 11.00 },
            new() { PerformanceId = 1, Round = 3, TimeSeconds = 11.00 },
        })[1];

        series.Rounds.Single(r => r.IsBest).Round.Should().Be(3);
    }

    [Fact]
    public void SingleRound_HasRoundsIsFalse_SoNoChainIsShown()
    {
        var series = PerformanceRoundSeriesBuilder.BuildLookup(new List<PerformanceRound>
        {
            new() { PerformanceId = 1, Round = 1, TimeSeconds = 11.55 },
        })[1];

        series.HasRounds.Should().BeFalse();
    }

    [Fact]
    public void BestTime_IsMinimum_OrNullWhenEmpty()
    {
        PerformanceRoundSeries.BestTime(new List<PerformanceRound>()).Should().BeNull();
        PerformanceRoundSeries.BestTime(new List<PerformanceRound>
        {
            new() { TimeSeconds = 11.2 }, new() { TimeSeconds = 10.9 }
        }).Should().Be(10.9);
    }
}
