using System.Globalization;
using System.Text;
using CloverleafTrack.ViewModels.Seasons;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CloverleafTrack.Web.Pdf;

/// <summary>
/// One-page poster of the season wrap (11x17 in, scales down to letter when printed). Built from the same <see cref="SeasonWrapViewModel"/>
/// as the page, so the two always agree. QuestPDF Community license (set in Program.cs). BRAIN.md [C123].
/// </summary>
public static class SeasonWrapPoster
{
    private const string Nav = "#233625";
    private const string Accent = "#7ac943";
    private const string Muted = "#6b7280";
    private const string Ink = "#1f2937";

    public const int MaxRecords = 8;

    public static byte[] Generate(SeasonWrapViewModel vm, string siteUrl) => Build(vm, siteUrl).GeneratePdf();

    /// <summary>Page images (used to check the poster stays on one page).</summary>
    public static int PageCount(SeasonWrapViewModel vm, string siteUrl) => Build(vm, siteUrl).GenerateImages(new ImageGenerationSettings { RasterDpi = 20 }).Count();

    private static Document Build(SeasonWrapViewModel vm, string siteUrl)
    {
        var through = vm.ThroughDate.HasValue
            ? (vm.InProgress ? $"The season so far, through {vm.ThroughDate:MMMM d, yyyy}" : $"{vm.StartDate:MMMM yyyy} to {vm.EndDate:MMMM yyyy}")
            : vm.Name;

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.Tabloid.Portrait());
            page.Margin(0);
            page.DefaultTextStyle(t => t.FontSize(14).FontColor(Ink));

            page.Content().Column(col =>
            {
                // Header band
                col.Item().Background(Nav).PaddingHorizontal(40).PaddingVertical(34).Column(h =>
                {
                    h.Item().Text("CLOVERLEAF TRACK & FIELD  ·  SEASON WRAP").FontSize(15).Bold().FontColor(Accent).LetterSpacing(0.12f);
                    h.Item().Text(vm.Name).FontSize(72).Bold().FontColor(Colors.White);
                    h.Item().Text(through).FontSize(18).FontColor("#d1d5db");
                });

                col.Item().PaddingHorizontal(40).PaddingTop(28).Column(body =>
                {
                    // Headline numbers
                    body.Item().Row(row =>
                    {
                        Tile(row, "MEETS", vm.Meets.ToString());
                        Tile(row, "ATHLETES", vm.AthletesCompeted.ToString());
                        Tile(row, "PRS", vm.PRs.ToString());
                        Tile(row, "SCHOOL RECORDS SET", vm.SchoolRecordsSet.ToString());
                    });

                    body.Item().PaddingTop(26).Row(row =>
                    {
                        // Left: records + best marks
                        row.RelativeItem().PaddingRight(16).Column(left =>
                        {
                            if (vm.SchoolRecords.Count > 0)
                            {
                                Heading(left, "School records set");
                                foreach (var r in vm.SchoolRecords.Take(MaxRecords))
                                {
                                    left.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(r.EventLabel).SemiBold();
                                            rr.AutoItem().Text(r.Mark).Bold();
                                        });
                                        c.Item().Text(string.Join(" / ", r.Holders.Select(h => h.Name)) + (r.TimesBroken > 1 ? $"  (broken {r.TimesBroken} times)" : ""))
                                            .FontSize(11).FontColor(Muted);
                                    });
                                }
                                if (vm.SchoolRecords.Count > MaxRecords)
                                    left.Item().Text($"+ {vm.SchoolRecords.Count - MaxRecords} more at {Short(siteUrl)}{vm.Url}").FontSize(11).FontColor(Muted);
                            }

                            if (vm.Standouts.Count > 0)
                            {
                                left.Item().PaddingTop(18);
                                Heading(left, "Best marks of the season");
                                foreach (var s in vm.Standouts.Take(4))
                                {
                                    left.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(s.Athlete.Name).SemiBold();
                                            rr.AutoItem().Text(s.Mark).Bold();
                                        });
                                        c.Item().Text($"{s.EventLabel}  ·  {s.Percentile} percentile" + (s.AllTimeRank is <= 10 ? $"  ·  #{s.AllTimeRank} all-time" : ""))
                                            .FontSize(11).FontColor(Muted);
                                    });
                                }
                            }
                        });

                        // Right: spotlights, team
                        row.RelativeItem().PaddingLeft(16).Column(right =>
                        {
                            if (vm.Breakouts.Count > 0)
                            {
                                Heading(right, "Most PRs");
                                foreach (var b in vm.Breakouts.Take(4))
                                {
                                    right.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(b.Athlete.Name).SemiBold();
                                            rr.AutoItem().Text($"{b.PRs} PRs").Bold();
                                        });
                                        c.Item().Text($"Biggest: {b.Highlight}").FontSize(11).FontColor(Muted);
                                    });
                                }
                            }

                            if (vm.Improvements.Count > 0)
                            {
                                right.Item().PaddingTop(18);
                                Heading(right, "Biggest jumps");
                                foreach (var i in vm.Improvements.Take(4))
                                {
                                    right.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(i.Athlete.Name).SemiBold();
                                            rr.AutoItem().Text(i.Change).Bold();
                                        });
                                        c.Item().Text($"{i.EventLabel}: {i.Before} to {i.After}").FontSize(11).FontColor(Muted);
                                    });
                                }
                            }

                            if (vm.DualRecord is { } d && (d.Boys.Any || d.Girls.Any))
                            {
                                right.Item().PaddingTop(18);
                                Heading(right, "Dual meet record");
                                right.Item().Row(rr =>
                                {
                                    rr.RelativeItem().Column(c => { c.Item().Text("BOYS").FontSize(11).FontColor(Muted); c.Item().Text(d.Boys.Any ? d.Boys.Text : "-").FontSize(34).Bold(); });
                                    rr.RelativeItem().Column(c => { c.Item().Text("GIRLS").FontSize(11).FontColor(Muted); c.Item().Text(d.Girls.Any ? d.Girls.Text : "-").FontSize(34).Bold(); });
                                });
                            }

                            if (vm.BoysLeaders.Count > 0 || vm.GirlsLeaders.Count > 0)
                            {
                                right.Item().PaddingTop(18);
                                Heading(right, "Scoring leaders");
                                right.Item().Row(rr =>
                                {
                                    Leaders(rr, "BOYS", vm.BoysLeaders);
                                    Leaders(rr, "GIRLS", vm.GirlsLeaders);
                                });
                            }
                        });
                    });

                    // Progression chart
                    if (vm.Progression.Count > 1)
                    {
                        body.Item().PaddingTop(20).Column(c =>
                        {
                            Heading(c, "Season progression: PRs per meet (bars) and average percentile (line)");
                            c.Item().Height(170).Svg(Chart(vm));
                        });
                    }

                    body.Item().PaddingTop(14).Row(rr =>
                    {
                        rr.RelativeItem().Text($"{Short(siteUrl)}{vm.Url}").FontSize(12).Bold().FontColor(Nav);
                        rr.AutoItem().Text($"Updated {DateTime.Now:MMMM d, yyyy}").FontSize(11).FontColor(Muted);
                    });
                });
            });
        }));
    }

    private static void Tile(RowDescriptor row, string label, string value) =>
        row.RelativeItem().Padding(4).Border(1).BorderColor("#d1d5db").Padding(14).Column(c =>
        {
            c.Item().Text(label).FontSize(11).SemiBold().FontColor(Muted).LetterSpacing(0.08f);
            c.Item().Text(value).FontSize(40).Bold().FontColor(Nav);
        });

    private static void Heading(ColumnDescriptor col, string text)
    {
        col.Item().PaddingBottom(8).BorderBottom(2).BorderColor(Accent).PaddingBottom(4).Text(text).FontSize(17).Bold().FontColor(Nav);
        col.Item().PaddingBottom(2);
    }

    private static void Leaders(RowDescriptor row, string title, List<WrapScoringLeader> leaders) =>
        row.RelativeItem().PaddingRight(6).Column(c =>
        {
            c.Item().Text(title).FontSize(11).FontColor(Muted);
            foreach (var l in leaders.Take(3))
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text(l.Athlete.Name).FontSize(12);
                    r.AutoItem().Text(l.Points.ToString("0.#", CultureInfo.InvariantCulture)).FontSize(12).Bold();
                });
        });

    private static string Short(string siteUrl) => siteUrl.Replace("https://", "").Replace("http://", "").TrimEnd('/');

    /// <summary>Plain SVG with explicit colours (the PDF renderer has no CSS).</summary>
    public static string Chart(SeasonWrapViewModel vm)
    {
        var pts = vm.Progression;
        const double W = 712, H = 170, left = 28, right = 28, top = 8, bottom = 28;
        var plotW = W - left - right;
        var plotH = H - top - bottom;
        var maxPrs = Math.Max(1, pts.Max(p => p.PRs));
        var slot = plotW / pts.Count;
        var barW = Math.Min(24, slot * 0.6);
        string N(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
        double X(int i) => left + slot * i + slot / 2;
        double YPct(double v) => top + plotH - plotH * v / 100;

        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {N(W)} {N(H)}\" width=\"{N(W)}\" height=\"{N(H)}\">");
        foreach (var tick in new[] { 0, 50, 100 })
        {
            var y = YPct(tick);
            sb.Append($"<line x1=\"{N(left)}\" x2=\"{N(W - right)}\" y1=\"{N(y)}\" y2=\"{N(y)}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
            sb.Append($"<text x=\"{N(W - right + 4)}\" y=\"{N(y + 4)}\" font-size=\"10\" fill=\"#6b7280\">{tick}</text>");
        }
        sb.Append($"<text x=\"{N(left - 4)}\" y=\"{N(top + 8)}\" font-size=\"10\" fill=\"#6b7280\" text-anchor=\"end\">{maxPrs}</text>");
        for (var i = 0; i < pts.Count; i++)
        {
            var h = plotH * pts[i].PRs / maxPrs;
            sb.Append($"<rect x=\"{N(X(i) - barW / 2)}\" y=\"{N(top + plotH - h)}\" width=\"{N(barW)}\" height=\"{N(h)}\" fill=\"#a855f7\"/>");
            if (i % 2 == 0 || pts.Count <= 14)
                sb.Append($"<text x=\"{N(X(i))}\" y=\"{N(top + plotH + 14)}\" font-size=\"9\" fill=\"#6b7280\" text-anchor=\"middle\">{pts[i].Date:M/d}</text>");
        }
        var line = pts.Select((p, i) => p.AveragePercentile > 0 ? $"{N(X(i))},{N(YPct(p.AveragePercentile))}" : null).Where(x => x != null).ToList();
        if (line.Count > 1)
            sb.Append($"<polyline points=\"{string.Join(" ", line)}\" fill=\"none\" stroke=\"#f59e0b\" stroke-width=\"2.5\" stroke-linejoin=\"round\"/>");
        for (var i = 0; i < pts.Count; i++)
            if (pts[i].AveragePercentile > 0)
                sb.Append($"<circle cx=\"{N(X(i))}\" cy=\"{N(YPct(pts[i].AveragePercentile))}\" r=\"3\" fill=\"#f59e0b\"/>");
        sb.Append("</svg>");
        return sb.ToString();
    }
}
