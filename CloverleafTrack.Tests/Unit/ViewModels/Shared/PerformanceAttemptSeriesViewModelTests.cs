using CloverleafTrack.ViewModels.Shared;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.ViewModels.Shared;

public class PerformanceAttemptSeriesViewModelTests
{
    [Fact]
    public void HasAttempts_FalseForEmptySeries()
    {
        var vm = new PerformanceAttemptSeriesViewModel();
        vm.HasAttempts.Should().BeFalse();
    }

    [Fact]
    public void HasAttempts_TrueWhenAnyAttemptPresent()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, IsFoul = true }
            }
        };

        vm.HasAttempts.Should().BeTrue();
    }

    [Fact]
    public void ValidAttemptCount_CountsOnlyNonFoulNonPassWithDistance()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, DistanceInches = 300 },
                new() { AttemptNumber = 2, IsFoul = true },
                new() { AttemptNumber = 3, IsPass = true },
                new() { AttemptNumber = 4, DistanceInches = 320 },
            }
        };

        vm.ValidAttemptCount.Should().Be(2);
    }

    [Fact]
    public void AverageValidInches_NullWhenNoValidAttempts()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, IsFoul = true },
                new() { AttemptNumber = 2, IsPass = true },
            }
        };

        vm.AverageValidInches.Should().BeNull();
    }

    [Fact]
    public void AverageValidInches_AveragesOnlyValidAttempts()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, DistanceInches = 300 },
                new() { AttemptNumber = 2, DistanceInches = 320 },
                new() { AttemptNumber = 3, IsFoul = true },
            }
        };

        vm.AverageValidInches.Should().Be(310);
    }

    [Fact]
    public void SpreadInches_NullWhenNoValidAttempts()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, IsFoul = true }
            }
        };

        vm.SpreadInches.Should().BeNull();
    }

    [Fact]
    public void SpreadInches_ZeroForSingleValidAttempt()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, DistanceInches = 400 }
            }
        };

        vm.SpreadInches.Should().Be(0);
    }

    [Fact]
    public void SpreadInches_IsBestMinusWorstAmongValidAttempts()
    {
        var vm = new PerformanceAttemptSeriesViewModel
        {
            Attempts = new List<PerformanceAttemptViewModel>
            {
                new() { AttemptNumber = 1, DistanceInches = 300 },
                new() { AttemptNumber = 2, DistanceInches = 350 },
                new() { AttemptNumber = 3, IsFoul = true },
                new() { AttemptNumber = 4, DistanceInches = 280 },
            }
        };

        vm.SpreadInches.Should().Be(70); // 350 - 280
    }
}
