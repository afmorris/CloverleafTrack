using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.ViewModels.Admin.AthleteSummaries;
using Microsoft.AspNetCore.Mvc;
using Slugify;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class AthleteSummariesController(
    IAdminAthleteRepository athleteRepository,
    IAthleteSummaryRepository summaryRepository,
    IAthleteSummaryService summaryService,
    IAthleteSummaryLinkService linkService) : Controller
{
    private static readonly SlugHelper Slugs = new();

    /// <summary>
    /// Review page. Defaults to the most recently graduated class ("last year's seniors"). Only athletes who competed in
    /// their senior school year are listed; <paramref name="includeAll"/> also shows the rest.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? graduationYear, bool includeAll = false)
    {
        var all = await athleteRepository.GetAllAsync();
        var years = all.Select(a => a.GraduationYear).Where(y => y > 0).Distinct().OrderByDescending(y => y).ToList();
        var year = graduationYear ?? DefaultClassYear(years);

        var classAthletes = all.Where(a => a.GraduationYear == year)
            .OrderBy(a => a.Gender).ThenBy(a => a.LastName).ThenBy(a => a.FirstName).ToList();
        var competedSenior = await CompetedSeniorYearAsync(year);
        var athletes = classAthletes.Where(a => includeAll || competedSenior.Contains(a.Id)).ToList();
        var summaries = (await summaryRepository.GetForAthletesAsync(athletes.Select(a => a.Id))).ToDictionary(s => s.AthleteId);

        var rows = await Task.WhenAll(athletes.Select(async a =>
        {
            summaries.TryGetValue(a.Id, out var s);
            return new AthleteSummaryRow
            {
                AthleteId = a.Id,
                Name = $"{a.FirstName} {a.LastName}",
                Team = a.Gender == Gender.Female ? "Girls" : "Boys",
                PublicUrl = "/roster/" + Slugs.GenerateSlug($"{a.FirstName}-{a.LastName}"),
                CompetedSenior = competedSenior.Contains(a.Id),
                SummaryId = s?.Id,
                SummaryText = s?.SummaryText,
                SummaryHtml = s == null ? null : await linkService.LinkifyAsync(a.Id, s.SummaryText),
                Status = s?.Status,
                Model = s?.Model,
                GeneratedAt = s?.GeneratedAt
            };
        }));

        return View(new AthleteSummariesIndexViewModel
        {
            GraduationYear = year, GraduationYears = years, Rows = rows.ToList(), IncludeAll = includeAll,
            ExcludedCount = classAthletes.Count(a => !competedSenior.Contains(a.Id))
        });
    }

    /// <summary>
    /// A class's athletes with summary state, as JSON, so a Claude Code session can enumerate what to write. Athletes who
    /// did not compete in their senior school year are not in <c>athletes</c> (they are named in <c>excluded</c>) unless includeAll.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(int? graduationYear, bool includeAll = false)
    {
        var all = await athleteRepository.GetAllAsync();
        var year = graduationYear ?? DefaultClassYear(all.Select(a => a.GraduationYear).Distinct().OrderByDescending(y => y).ToList());
        var classAthletes = all.Where(a => a.GraduationYear == year).OrderBy(a => a.Gender).ThenBy(a => a.LastName).ToList();
        var competedSenior = await CompetedSeniorYearAsync(year);
        var athletes = classAthletes.Where(a => includeAll || competedSenior.Contains(a.Id)).ToList();
        var summaries = (await summaryRepository.GetForAthletesAsync(athletes.Select(a => a.Id))).ToDictionary(s => s.AthleteId);
        return Json(new
        {
            graduationYear = year,
            athletes = athletes.Select(a =>
            {
                summaries.TryGetValue(a.Id, out var s);
                return new
                {
                    athleteId = a.Id,
                    name = $"{a.FirstName} {a.LastName}",
                    team = a.Gender == Gender.Female ? "girls" : "boys",
                    competedSenior = competedSenior.Contains(a.Id),
                    hasSummary = s != null,
                    status = s?.Status.ToString(),
                    model = s?.Model
                };
            }),
            excluded = classAthletes.Where(a => !competedSenior.Contains(a.Id)).Select(a => $"{a.FirstName} {a.LastName}")
        });
    }

    /// <summary>Raw fact sheet JSON for an athlete: exactly what a writer is given.</summary>
    [HttpGet]
    public async Task<IActionResult> FactSheet(int athleteId)
    {
        var json = await summaryService.GetFactSheetAsync(athleteId);
        return json == null ? NotFound("Athlete has no performances.") : Content(json, "application/json");
    }

    /// <summary>Saves externally authored text as a Draft (no API call).</summary>
    [HttpPost]
    public async Task<IActionResult> Import(int athleteId, string text, string model)
    {
        if (string.IsNullOrWhiteSpace(text)) return BadRequest("text is required");
        await summaryService.SaveManualDraftAsync(athleteId, text, string.IsNullOrWhiteSpace(model) ? "manual" : model);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, int graduationYear, bool approved = true, bool includeAll = false)
    {
        await summaryRepository.SetApprovedAsync(id, approved);
        return RedirectToAction(nameof(Index), new { graduationYear, includeAll });
    }

    /// <summary>Reviewer edits the text; this resets the summary to Draft so edited text is re-approved explicitly.</summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, int graduationYear, string text, bool includeAll = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["ErrorMessage"] = "Summary text cannot be empty. Use Delete to remove a summary.";
        }
        else
        {
            await summaryRepository.UpdateTextAsync(id, text.Trim());
            TempData["SuccessMessage"] = "Text saved (back to Draft until approved).";
        }
        return RedirectToAction(nameof(Index), new { graduationYear, includeAll });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int graduationYear, bool includeAll = false)
    {
        await summaryRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { graduationYear, includeAll });
    }

    /// <summary>Athletes with at least one individual or relay performance in the school year that ends in <paramref name="graduationYear"/>.</summary>
    private Task<HashSet<int>> CompetedSeniorYearAsync(int graduationYear)
    {
        var (start, end) = ClassYearCalculator.GetSeniorSchoolYear(graduationYear);
        return athleteRepository.GetAthleteIdsWithPerformancesBetweenAsync(start, end);
    }

    /// <summary>The most recent class that has graduated: from July on, this calendar year's class; before July, last year's.</summary>
    private static int DefaultClassYear(List<int> years)
    {
        var now = DateTime.Now;
        var target = now.Month >= 7 ? now.Year : now.Year - 1;
        return years.Where(y => y <= target).DefaultIfEmpty(years.FirstOrDefault()).Max();
    }
}
