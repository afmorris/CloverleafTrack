using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class LeaderboardController(ILeaderboardService leaderboardService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var viewModel = await leaderboardService.GetLeaderboardAsync();
        return View(viewModel);
    }

    [HttpGet("/leaderboard/{eventKey}")]
    public async Task<IActionResult> Details(string eventKey)
    {
        var viewModel = await leaderboardService.GetLeaderboardDetailsAsync(eventKey);
        
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }
}