using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Athletes;

/// <summary>
/// Career progression chart for one event (issue #26). All pixel coordinates are precomputed
/// server-side (CareerChartGeometry) against a fixed SVG viewBox — the view only draws what's
/// here, it never does axis-inversion or domain math itself.
/// </summary>
public class CareerChartViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    /// <summary>Indoor and outdoor versions of the same event name (e.g. "Shot Put") are different EventIds and get separate charts — this is what lets the UI tell them apart, since EventName alone is ambiguous.</summary>
    public Environment Environment { get; set; }
    public bool IsFieldEvent { get; set; }
    public bool IsRelay { get; set; }

    public List<CareerChartPointViewModel> Points { get; set; } = new();
    public List<ClassYearTickViewModel> ClassTicks { get; set; } = new();
    public List<CareerChartYTickViewModel> YTicks { get; set; } = new();

    // Record territory — null/false when there's no known record, or the athlete already holds it
    // (the "how much air is left" zone is meaningless once you ARE the record).
    public bool ShowRecordZone { get; set; }
    public string? RecordFormatted { get; set; }
    public double? RecordLinePixelY { get; set; }
    public double? RecordZoneTopPixelY { get; set; }
    public double? RecordZoneBottomPixelY { get; set; }

    // Program median / interquartile band — suppressed for relays and events with < 10 marks.
    public bool ShowMedianBand { get; set; }
    public string? MedianFormatted { get; set; }
    public double? MedianLinePixelY { get; set; }
    public double? IqrZoneTopPixelY { get; set; }
    public double? IqrZoneBottomPixelY { get; set; }

    // Stat row
    public string CareerBestFormatted { get; set; } = string.Empty;
    public string? CareerImprovementFormatted { get; set; }
    public byte? BestPercentile { get; set; }
    public string? DeltaOffRecordFormatted { get; set; }

    // Fixed plot geometry, shared with the view for line/point drawing.
    public double PlotLeft { get; set; }
    public double PlotRight { get; set; }
    public double PlotTop { get; set; }
    public double PlotBottom { get; set; }

    public string PolylinePoints => string.Join(" ", Points.Select(p => $"{p.PixelX:0.##},{p.PixelY:0.##}"));
}

public class CareerChartPointViewModel
{
    public double PixelX { get; set; }
    public double PixelY { get; set; }
    public string Formatted { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? ClassAtTime { get; set; }
    public bool IsCareerBest { get; set; }
}

public class ClassYearTickViewModel
{
    public double PixelX { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class CareerChartYTickViewModel
{
    public double PixelY { get; set; }
    public string Label { get; set; } = string.Empty;

    /// <summary>Below `sm`, only 3 of these render — see the issue's mobile spec ("drop to three Y-axis ticks").</summary>
    public bool HiddenOnMobile { get; set; }
}
