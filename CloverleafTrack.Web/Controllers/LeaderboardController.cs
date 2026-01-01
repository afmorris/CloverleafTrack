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
}