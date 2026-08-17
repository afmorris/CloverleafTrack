using CloverleafTrack.Services;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Services;

public class CareerChartGeometryTests
{
    // -------------------------------------------------------------------------
    // MapValueToPixelY — the exact "trivially easy to get upside-down" trap
    // the issue's acceptance criteria call out. Field and running events must
    // map in OPPOSITE raw-value directions but the SAME visual direction
    // (better = higher on screen = smaller pixelY).
    // -------------------------------------------------------------------------

    [Fact]
    public void MapValueToPixelY_FieldEvent_HigherRawValue_IsHigherOnScreen()
    {
        // Field: bigger distance = better = should render near the top (smaller pixelY).
        var bestY = CareerChartGeometry.MapValueToPixelY(rawValue: 100, min: 0, max: 100, plotTop: 0, plotBottom: 200, isFieldEvent: true);
        var worstY = CareerChartGeometry.MapValueToPixelY(rawValue: 0, min: 0, max: 100, plotTop: 0, plotBottom: 200, isFieldEvent: true);

        bestY.Should().BeLessThan(worstY, "the best (highest) field-event mark should render higher on screen (smaller pixelY)");
        bestY.Should().Be(0);
        worstY.Should().Be(200);
    }

    [Fact]
    public void MapValueToPixelY_RunningEvent_LowerRawValue_IsHigherOnScreen()
    {
        // Running: smaller time = better = should ALSO render near the top (smaller pixelY) —
        // the opposite raw-value direction from field, but the same visual direction.
        var bestY = CareerChartGeometry.MapValueToPixelY(rawValue: 10, min: 10, max: 20, plotTop: 0, plotBottom: 200, isFieldEvent: false);
        var worstY = CareerChartGeometry.MapValueToPixelY(rawValue: 20, min: 10, max: 20, plotTop: 0, plotBottom: 200, isFieldEvent: false);

        bestY.Should().BeLessThan(worstY, "the best (lowest) running-event time should render higher on screen (smaller pixelY)");
        bestY.Should().Be(0);
        worstY.Should().Be(200);
    }

    [Fact]
    public void MapValueToPixelY_FieldAndRunning_SameRelativeStanding_RenderAtSameHeight()
    {
        // A field mark at the top of its domain and a running mark at the bottom of its domain
        // are both "the best possible mark" — they must map to the same pixelY.
        var fieldBestY = CareerChartGeometry.MapValueToPixelY(rawValue: 50, min: 0, max: 50, plotTop: 10, plotBottom: 210, isFieldEvent: true);
        var runningBestY = CareerChartGeometry.MapValueToPixelY(rawValue: 0, min: 0, max: 50, plotTop: 10, plotBottom: 210, isFieldEvent: false);

        fieldBestY.Should().Be(runningBestY);
    }

    [Fact]
    public void MapValueToPixelY_Midpoint_RendersAtCenterOfPlot_RegardlessOfEventType()
    {
        var fieldMidY = CareerChartGeometry.MapValueToPixelY(rawValue: 50, min: 0, max: 100, plotTop: 0, plotBottom: 200, isFieldEvent: true);
        var runningMidY = CareerChartGeometry.MapValueToPixelY(rawValue: 50, min: 0, max: 100, plotTop: 0, plotBottom: 200, isFieldEvent: false);

        fieldMidY.Should().Be(100);
        runningMidY.Should().Be(100);
    }

    [Fact]
    public void MapValueToPixelY_DegenerateDomain_ReturnsPlotCenter()
    {
        var y = CareerChartGeometry.MapValueToPixelY(rawValue: 5, min: 5, max: 5, plotTop: 0, plotBottom: 200, isFieldEvent: true);

        y.Should().Be(100);
    }

    [Fact]
    public void MapValueToPixelY_NeverExceedsPlotBounds()
    {
        var y1 = CareerChartGeometry.MapValueToPixelY(rawValue: 0, min: 0, max: 100, plotTop: 20, plotBottom: 220, isFieldEvent: true);
        var y2 = CareerChartGeometry.MapValueToPixelY(rawValue: 100, min: 0, max: 100, plotTop: 20, plotBottom: 220, isFieldEvent: true);

        y1.Should().BeInRange(20, 220);
        y2.Should().BeInRange(20, 220);
    }

    // -------------------------------------------------------------------------
    // ComputeDomain — must cover every value that will be plotted, with padding,
    // so reference lines/zones never clip (acceptance criterion).
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeDomain_CoversAllInputValues_WithPadding()
    {
        var (min, max) = CareerChartGeometry.ComputeDomain(new[] { 10.0, 20.0, 15.0 });

        min.Should().BeLessThan(10);
        max.Should().BeGreaterThan(20);
    }

    [Fact]
    public void ComputeDomain_IncludesRecordAndIqrValues_WhenPassedIn()
    {
        // Simulates passing performance values + record value + Q1/Q3 all together, as the
        // caller must do to satisfy "the Y domain accounts for them when computing min/max".
        var allValues = new List<double> { 50.0, 55.0, 60.0 }; // performances
        allValues.Add(65.0); // record — better than any performance, must not clip
        allValues.Add(40.0); // Q1 — worse than any performance, must not clip

        var (min, max) = CareerChartGeometry.ComputeDomain(allValues);

        min.Should().BeLessThan(40);
        max.Should().BeGreaterThan(65);
    }

    [Fact]
    public void ComputeDomain_SingleValue_PadsSymmetrically()
    {
        var (min, max) = CareerChartGeometry.ComputeDomain(new[] { 42.0 });

        min.Should().BeLessThan(42);
        max.Should().BeGreaterThan(42);
    }

    [Fact]
    public void ComputeDomain_EmptyInput_ReturnsDefaultRange()
    {
        var (min, max) = CareerChartGeometry.ComputeDomain(Array.Empty<double>());

        max.Should().BeGreaterThan(min);
    }
}
