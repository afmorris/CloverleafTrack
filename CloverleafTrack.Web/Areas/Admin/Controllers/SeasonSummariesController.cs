using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Admin.SeasonSummaries;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

/// <summary>Review of the written "season in words" on the season wrap. Text is written in chat (skill generate-season-summary); the site never calls a model. BRAIN.md [C124].</summary>
[Area("Admin")]
public class SeasonSummariesController(
    IAdminSeasonRepository seasonRepository,
    ISeasonSummaryRepository summaryRepository,
    ISeasonSummaryService summaryService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var seasons = (await seasonRepository.GetAllAsync()).OrderByDescending(s => s.StartDate).ToList();
        var summaries = (await summaryRepository.GetAllAsync()).ToDictionary(s => s.SeasonId);
        var rows = seasons.Select(season =>
        {
            summaries.TryGetValue(season.Id, out var s);
            return new SeasonSummaryRow
            {
                SeasonId = season.Id,
                SeasonName = season.Name,
                WrapUrl = $"/seasons/{season.Name}/wrap",
                SummaryId = s?.Id,
                SummaryText = s?.SummaryText,
                Status = s?.Status,
                Model = s?.Model,
                GeneratedAt = s?.GeneratedAt,
                ReviewedAt = s?.ReviewedAt
            };
        }).ToList();
        return View(new SeasonSummariesIndexViewModel { Rows = rows });
    }

    /// <summary>Seasons with their summary state, as JSON, for a Claude Code session to enumerate.</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var seasons = (await seasonRepository.GetAllAsync()).OrderByDescending(s => s.StartDate).ToList();
        var summaries = (await summaryRepository.GetAllAsync()).ToDictionary(s => s.SeasonId);
        return Json(new
        {
            seasons = seasons.Select(season =>
            {
                summaries.TryGetValue(season.Id, out var s);
                return new
                {
                    seasonId = season.Id,
                    name = season.Name,
                    current = season.IsCurrentSeason,
                    hasSummary = s != null,
                    status = s?.Status.ToString(),
                    model = s?.Model
                };
            })
        });
    }

    /// <summary>Raw fact sheet JSON for a season — exactly what the writer is given.</summary>
    [HttpGet]
    public async Task<IActionResult> FactSheet(int seasonId)
    {
        var json = await summaryService.GetFactSheetAsync(seasonId);
        return json == null ? NotFound("Season has no results.") : Content(json, "application/json");
    }

    /// <summary>
    /// Saves chat-authored text as a Draft. Refuses to replace an Approved summary (which would pull it from the public wrap)
    /// unless <paramref name="overwrite"/> is true.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Import(int seasonId, string text, string model, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(text)) return BadRequest("text is required");
        var existing = await summaryRepository.GetForSeasonAsync(seasonId);
        if (existing is { Status: Models.Enums.SummaryStatus.Approved } && !overwrite)
            return Conflict("An Approved summary exists; pass overwrite=true to replace it (it returns to Draft).");
        await summaryService.SaveManualDraftAsync(seasonId, text, string.IsNullOrWhiteSpace(model) ? "manual" : model);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, bool approved = true)
    {
        await summaryRepository.SetApprovedAsync(id, approved);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Reviewer edits the text; this resets the summary to Draft so edited text is re-approved explicitly.</summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string text)
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await summaryRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
