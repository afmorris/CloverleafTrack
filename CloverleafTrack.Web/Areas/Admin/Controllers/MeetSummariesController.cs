using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Admin.MeetSummaries;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class MeetSummariesController(
    IAdminMeetRepository meetRepository,
    IAdminSeasonRepository seasonRepository,
    IMeetSummaryRepository summaryRepository,
    IMeetSummaryService summaryService,
    IMeetSummaryLinkService linkService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? seasonId)
    {
        var seasons = (await seasonRepository.GetAllAsync()).OrderByDescending(s => s.StartDate).ToList();
        var season = seasons.FirstOrDefault(s => s.Id == seasonId)
                     ?? seasons.FirstOrDefault(s => s.IsCurrentSeason)
                     ?? seasons.FirstOrDefault();
        if (season == null) return View(new MeetSummariesIndexViewModel());

        var meets = (await meetRepository.GetFilteredAsync(null, season.Id, null, null)).OrderBy(m => m.Date).ToList();
        var summaries = (await summaryRepository.GetForMeetsAsync(meets.Select(m => m.Id)))
            .ToDictionary(s => s.MeetId);

        var rows = new List<MeetSummaryRow>();
        foreach (var m in meets)
        {
            summaries.TryGetValue(m.Id, out var s);
            rows.Add(new MeetSummaryRow
            {
                MeetId = m.Id,
                MeetName = m.Name,
                Date = m.Date,
                PublicUrl = m.ResultsUrl,
                Environment = m.Environment.ToString(),
                IsJv = m.MeetLevel == Models.Enums.MeetLevel.JV,
                SummaryId = s?.Id,
                SummaryText = s?.SummaryText,
                SummaryHtml = s == null ? null : await linkService.LinkifyAsync(m.Id, s.SummaryText),
                Status = s?.Status,
                Model = s?.Model,
                GeneratedAt = s?.GeneratedAt,
                ReviewedAt = s?.ReviewedAt
            });
        }

        return View(new MeetSummariesIndexViewModel
        {
            SeasonId = season.Id,
            SeasonName = season.Name,
            Seasons = seasons.Select(s => new SeasonChoice { Id = s.Id, Name = s.Name }).ToList(),
            Rows = rows
        });
    }

    /// <summary>
    /// The season's meets with their summary state, as JSON, so a Claude Code session can enumerate what to write.
    /// Pass seasonId or seasonName (e.g. "2025-2026"); defaults to the current season.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(int? seasonId, string? seasonName)
    {
        var seasons = await seasonRepository.GetAllAsync();
        var season = seasons.FirstOrDefault(s => seasonId.HasValue ? s.Id == seasonId : seasonName != null ? s.Name == seasonName : s.IsCurrentSeason);
        if (season == null) return NotFound("Season not found.");

        var meets = (await meetRepository.GetFilteredAsync(null, season.Id, null, null)).OrderBy(m => m.Date).ToList();
        var summaries = (await summaryRepository.GetForMeetsAsync(meets.Select(m => m.Id))).ToDictionary(s => s.MeetId);
        return Json(new
        {
            seasonId = season.Id,
            seasonName = season.Name,
            meets = meets.Select(m =>
            {
                summaries.TryGetValue(m.Id, out var s);
                return new
                {
                    meetId = m.Id,
                    name = m.Name,
                    date = m.Date.ToString("yyyy-MM-dd"),
                    level = m.MeetLevel.ToString(),
                    hasSummary = s != null,
                    status = s?.Status.ToString(),
                    model = s?.Model
                };
            })
        });
    }

    /// <summary>Raw fact sheet JSON for a meet — lets a Claude Code session (or you) read exactly what the model is given.</summary>
    [HttpGet]
    public async Task<IActionResult> FactSheet(int meetId)
    {
        var json = await summaryService.GetFactSheetAsync(meetId);
        return json == null ? NotFound("Meet has no performances.") : Content(json, "application/json");
    }

    /// <summary>Saves externally authored text as a Draft (no API call). Used when summaries are written by Claude Code in a session.</summary>
    [HttpPost]
    public async Task<IActionResult> Import(int meetId, string text, string model)
    {
        if (string.IsNullOrWhiteSpace(text)) return BadRequest("text is required");
        await summaryService.SaveManualDraftAsync(meetId, text, string.IsNullOrWhiteSpace(model) ? "manual" : model);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, int seasonId, bool approved = true)
    {
        await summaryRepository.SetApprovedAsync(id, approved);
        return RedirectToAction(nameof(Index), new { seasonId });
    }

    /// <summary>Reviewer edits the text; this resets the summary to Draft so edited text is re-approved explicitly.</summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, int seasonId, string text)
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
        return RedirectToAction(nameof(Index), new { seasonId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int seasonId)
    {
        await summaryRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { seasonId });
    }
}
