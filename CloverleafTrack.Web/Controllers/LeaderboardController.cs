using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

// NOTE: the public-facing route is "/events" (see BRAIN.md "Events IA" entry), but the
// controller/service/table are intentionally still named "Leaderboard" — do not rename them.
public class LeaderboardController(ILeaderboardService leaderboardService) : Controller
{
    [HttpGet("/events")]
    public async Task<IActionResult> Index()
    {
        var viewModel = await leaderboardService.GetLeaderboardAsync();
        return View(viewModel);
    }

    [HttpGet("/events/{eventKey}")]
    public async Task<IActionResult> Details(string eventKey)
    {
        var viewModel = await leaderboardService.GetLeaderboardDetailsAsync(eventKey);

        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    // Legacy URLs — kept live and indexed, must never 404. 301 to the new /events routes.
    [HttpGet("/leaderboard")]
    public IActionResult IndexLegacyRedirect()
    {
        return RedirectToActionPermanent(nameof(Index));
    }

    [HttpGet("/leaderboard/{eventKey}")]
    public IActionResult DetailsLegacyRedirect(string eventKey)
    {
        return RedirectToActionPermanent(nameof(Details), new { eventKey });
    }
}