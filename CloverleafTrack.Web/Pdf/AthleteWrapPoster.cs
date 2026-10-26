using CloverleafTrack.ViewModels.Athletes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CloverleafTrack.Web.Pdf;

/// <summary>
/// One-page poster of an athlete's career wrap (11x17 in, scales down to letter). Built from the same <see cref="AthleteWrapViewModel"/>
/// as the page. QuestPDF Community license (set in Program.cs). BRAIN.md [C126].
/// </summary>
public static class AthleteWrapPoster
{
    private const string Nav = "#233625";
    private const string Accent = "#7ac943";
    private const string Muted = "#6b7280";
    private const string Ink = "#1f2937";

    public const int MaxBests = 8;
    public const int MaxSeasons = 5;

    public static byte[] Generate(AthleteWrapViewModel vm, string siteUrl) => Build(vm, siteUrl).GeneratePdf();

    public static int PageCount(AthleteWrapViewModel vm, string siteUrl) => Build(vm, siteUrl).GenerateImages(new ImageGenerationSettings { RasterDpi = 20 }).Count();

    private static Document Build(AthleteWrapViewModel vm, string siteUrl)
    {
        var span = vm.Seasons == 1 ? vm.FirstSeason : $"{vm.FirstSeason} to {vm.LastSeason}";

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.Tabloid.Portrait());
            page.Margin(0);
            page.DefaultTextStyle(t => t.FontSize(14).FontColor(Ink));

            page.Content().Column(col =>
            {
                col.Item().Background(Nav).PaddingHorizontal(40).PaddingVertical(34).Column(h =>
                {
                    h.Item().Text("CLOVERLEAF TRACK & FIELD  ·  CAREER WRAP").FontSize(15).Bold().FontColor(Accent).LetterSpacing(0.12f);
                    h.Item().Text(vm.FullName).FontSize(60).Bold().FontColor(Colors.White);
                    h.Item().Text($"Class of {vm.GraduationYear}  ·  {vm.TeamLabel}  ·  {span}").FontSize(18).FontColor("#d1d5db");
                });

                col.Item().PaddingHorizontal(40).PaddingTop(28).Column(body =>
                {
                    body.Item().Row(row =>
                    {
                        Tile(row, vm.Seasons == 1 ? "SEASON" : "SEASONS", vm.Seasons.ToString());
                        Tile(row, "MEETS", vm.Meets.ToString());
                        Tile(row, "PRS", vm.PRs.ToString());
                        Tile(row, "SCHOOL RECORDS", vm.SchoolRecords.ToString());
                    });

                    body.Item().PaddingTop(26).Row(row =>
                    {
                        row.RelativeItem().PaddingRight(16).Column(left =>
                        {
                            if (vm.Bests.Count > 0)
                            {
                                Heading(left, "Career bests");
                                foreach (var b in vm.Bests.Take(MaxBests))
                                {
                                    left.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(b.EventLabel).SemiBold();
                                            rr.AutoItem().Text(b.Mark).Bold();
                                        });
                                        c.Item().Text(Detail(b)).FontSize(11).FontColor(Muted);
                                    });
                                }
                            }
                        });

                        row.RelativeItem().PaddingLeft(16).Column(right =>
                        {
                            if (vm.Improvements.Count > 0)
                            {
                                Heading(right, "Biggest improvements");
                                foreach (var i in vm.Improvements)
                                {
                                    right.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(i.EventLabel).SemiBold();
                                            rr.AutoItem().Text(i.Change).Bold();
                                        });
                                        c.Item().Text($"{i.First} to {i.Best}").FontSize(11).FontColor(Muted);
                                    });
                                }
                            }

                            if (vm.SeasonRows.Count > 0)
                            {
                                right.Item().PaddingTop(18);
                                Heading(right, "Season by season");
                                foreach (var s in vm.SeasonRows.TakeLast(MaxSeasons))
                                {
                                    right.Item().PaddingBottom(7).Column(c =>
                                    {
                                        c.Item().Row(rr =>
                                        {
                                            rr.RelativeItem().Text(s.Name).SemiBold();
                                            rr.AutoItem().Text($"{s.PRs} PRs").Bold();
                                        });
                                        c.Item().Text($"{s.Class}  ·  {s.Events} events  ·  {s.Results} marks" + (s.SchoolRecords > 0 ? $"  ·  {s.SchoolRecords} SR" : "")).FontSize(11).FontColor(Muted);
                                    });
                                }
                            }
                        });
                    });

                    body.Item().PaddingTop(20).Row(rr =>
                    {
                        rr.RelativeItem().Text($"{Short(siteUrl)}{vm.Url}").FontSize(12).Bold().FontColor(Nav);
                        rr.AutoItem().Text($"Updated {DateTime.Now:MMMM d, yyyy}").FontSize(11).FontColor(Muted);
                    });
                });
            });
        }));
    }

    private static string Detail(AthleteWrapBest b)
    {
        var parts = new List<string>();
        if (b.IsSchoolRecord) parts.Add("School record");
        else if (b.AllTimeRank is <= 10) parts.Add($"#{b.AllTimeRank} all-time");
        if (b.Percentile.HasValue) parts.Add($"{b.Percentile} percentile");
        parts.Add(b.MeetName);
        return string.Join("  ·  ", parts);
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

    private static string Short(string siteUrl) => siteUrl.Replace("https://", "").Replace("http://", "").TrimEnd('/');
}
