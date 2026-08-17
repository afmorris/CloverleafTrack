namespace CloverleafTrack.Services;

/// <summary>
/// Pure Y-axis mapping for the career progression chart (issue #26). Isolated from ViewModel/
/// view code specifically because the issue's own acceptance criteria flag this exact
/// computation as "trivially easy to get upside-down" — keeping it in one small, directly
/// tested function is the guard against that.
///
/// The chart always renders "better" as visually higher, regardless of event type:
///   - Field events: higher raw value (distance) = better = smaller pixelY (higher on screen).
///   - Running events: lower raw value (time) = better = smaller pixelY (higher on screen) —
///     i.e. the axis is inverted relative to the field-event case.
/// </summary>
public static class CareerChartGeometry
{
    /// <summary>
    /// Maps a raw value to a pixel Y coordinate within [plotTop, plotBottom] (SVG Y increases
    /// downward, so plotTop is the smaller number). min/max define the value domain; both must
    /// already account for every value that will be plotted (performances, record line, IQR
    /// band) — this function does not compute the domain, only the mapping.
    /// </summary>
    public static double MapValueToPixelY(double rawValue, double min, double max, double plotTop, double plotBottom, bool isFieldEvent)
    {
        if (max <= min)
        {
            return (plotTop + plotBottom) / 2.0;
        }

        var t = (rawValue - min) / (max - min); // 0 = min, 1 = max
        var betterT = isFieldEvent ? t : (1.0 - t); // 0 = worst, 1 = best, for either event type

        return plotBottom - betterT * (plotBottom - plotTop);
    }

    /// <summary>
    /// Computes the [min, max] value domain across every value that must fit on the chart —
    /// performances, the record value, and the IQR band edges — with symmetric padding, so
    /// reference lines/zones never clip outside the plot (acceptance criterion).
    /// </summary>
    public static (double Min, double Max) ComputeDomain(IEnumerable<double> values, double paddingFraction = 0.08)
    {
        var list = values.ToList();
        if (list.Count == 0) return (0, 1);

        var min = list.Min();
        var max = list.Max();

        if (max <= min)
        {
            // Degenerate: a single distinct value (or all-equal values). Pad by a fixed amount
            // so the point doesn't sit exactly on the plot edge.
            var pad = max == 0 ? 1.0 : Math.Abs(max) * 0.1;
            return (min - pad, max + pad);
        }

        var range = max - min;
        var padding = range * paddingFraction;
        return (min - padding, max + padding);
    }
}
