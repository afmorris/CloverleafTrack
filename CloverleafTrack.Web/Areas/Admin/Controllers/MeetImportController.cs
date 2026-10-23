using System.Text.Json;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Web.Import;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

/// <summary>
/// Review-then-apply import of a meet's results (and pre-meet entries) from a file prepared outside the site (BRAIN.md [C122]).
/// Stateless: the file travels with each request and is re-planned every time, so nothing is staged in the database and nothing is
/// written until Apply.
/// </summary>
[Area("Admin")]
public class MeetImportController(MeetImportService importService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int meetId)
    {
        var ctx = await importService.LoadContextAsync(meetId);
        if (ctx == null) return NotFound();
        return View(new MeetImportIndexViewModel { MeetId = meetId, MeetName = ctx.Meet.Name, MeetDate = ctx.Meet.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int meetId, string? json, IFormFile? file, List<MeetImportRowChoice>? rows)
    {
        var ctx = await importService.LoadContextAsync(meetId);
        if (ctx == null) return NotFound();

        if (file is { Length: > 0 })
        {
            using var reader = new StreamReader(file.OpenReadStream());
            json = await reader.ReadToEndAsync();
        }

        ResultsFile parsed;
        try
        {
            parsed = ResultsFile.Parse(json ?? "");
        }
        catch (ResultsFileException ex)
        {
            return View("Index", new MeetImportIndexViewModel
            {
                MeetId = meetId, MeetName = ctx.Meet.Name, MeetDate = ctx.Meet.Date, Json = json ?? "", Error = ex.Message
            });
        }

        var overrides = (rows ?? new()).Where(r => r.AthleteId.HasValue).ToDictionary(r => r.Index, r => r.AthleteId!.Value);
        var plan = MeetImportPlanner.Plan(parsed, ctx, overrides);

        // First visit: tick what is safe to apply. Re-checks (after picking an athlete) keep the admin's ticks.
        var included = rows is { Count: > 0 }
            ? rows.Where(r => r.Include).Select(r => r.Index).ToHashSet()
            : plan.Rows.Where(r => r.DefaultInclude).Select(r => r.Index).ToHashSet();
        if (rows is { Count: > 0 })
            foreach (var r in plan.Rows.Where(r => overrides.ContainsKey(r.Index) && r.DefaultInclude)) included.Add(r.Index);

        return View(new MeetImportReviewViewModel
        {
            MeetId = meetId, MeetName = ctx.Meet.Name, MeetDate = ctx.Meet.Date,
            Json = json ?? "", Plan = plan, Included = included, AthleteOverrides = overrides
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int meetId, string json, List<MeetImportRowChoice>? rows)
    {
        var ctx = await importService.LoadContextAsync(meetId);
        if (ctx == null) return NotFound();

        ResultsFile parsed;
        try
        {
            parsed = ResultsFile.Parse(json);
        }
        catch (ResultsFileException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { meetId });
        }

        rows ??= new();
        var overrides = rows.Where(r => r.AthleteId.HasValue).ToDictionary(r => r.Index, r => r.AthleteId!.Value);
        var plan = MeetImportPlanner.Plan(parsed, ctx, overrides);
        var chosen = rows.Where(r => r.Include).Select(r => r.Index).ToHashSet();

        var outcome = await importService.ApplyAsync(ctx, plan, chosen);

        var parts = new List<string>();
        if (outcome.PerformancesCreated > 0) parts.Add($"{outcome.PerformancesCreated} result{(outcome.PerformancesCreated == 1 ? "" : "s")} added");
        if (outcome.MarksReplaced > 0) parts.Add($"{outcome.MarksReplaced} mark{(outcome.MarksReplaced == 1 ? "" : "s")} replaced");
        if (outcome.PlacingsWritten > 0) parts.Add($"{outcome.PlacingsWritten} placing{(outcome.PlacingsWritten == 1 ? "" : "s")} saved");
        if (outcome.EntriesCreated > 0) parts.Add($"{outcome.EntriesCreated} entr{(outcome.EntriesCreated == 1 ? "y" : "ies")} created");
        if (outcome.EntriesLinked > 0) parts.Add($"{outcome.EntriesLinked} linked to pre-meet entries");
        TempData["SuccessMessage"] = parts.Count == 0 ? "Nothing was applied." : "Imported: " + string.Join(", ", parts) + ". Leaderboards rebuilt.";
        return RedirectToAction("Index", "MeetEntries", new { meetId });
    }

    /// <summary>
    /// What a file for this meet has to match, as JSON: the meet, its opponents, the exact event names, the athletes who could be at it,
    /// and what is already stored. For whoever prepares the file (see docs/meet-results-format.md); the site itself does no AI.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Context(int meetId)
    {
        var ctx = await importService.LoadContextAsync(meetId);
        if (ctx == null) return NotFound();
        var year = ctx.Meet.Date.Year;

        var payload = new
        {
            meet = new
            {
                id = ctx.Meet.Id, name = ctx.Meet.Name, date = ctx.Meet.Date.ToString("yyyy-MM-dd"),
                type = ctx.Meet.MeetType.ToString(), environment = ctx.Meet.Environment.ToString(),
                opponents = ctx.Participants.Select(p => p.SchoolName)
            },
            events = ctx.Events.OrderBy(e => e.Gender).ThenBy(e => e.EventCategorySortOrder).ThenBy(e => e.SortOrder).Select(e => new
            {
                gender = e.Gender == Gender.Male ? "Boys" : e.Gender == Gender.Female ? "Girls" : "Mixed",
                name = e.Name, athletes = e.AthleteCount
            }),
            athletes = ctx.Athletes.Where(a => a.GraduationYear >= year - 1 && a.GraduationYear <= year + 4)
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .Select(a => new { name = $"{a.FirstName} {a.LastName}", gender = a.Gender == Gender.Male ? "Boys" : "Girls", gradYear = a.GraduationYear, active = a.IsActive }),
            entries = ctx.Entries.Select(e => new { @event = e.EventName, gender = e.EventGender.ToString(), athlete = e.AthleteDisplayName, resultEntered = e.PerformanceId.HasValue }),
            storedResults = ctx.Performances.Count
        };

        return Json(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
