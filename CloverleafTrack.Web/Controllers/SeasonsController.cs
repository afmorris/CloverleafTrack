using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class SeasonsController(ISeasonService seasonService, IScoringService scoringService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await seasonService.GetSeasonCardsAsync();
        return View(model);
    }

    [HttpGet("/seasons/{name}")]
    public async Task<IActionResult> Details(string name)
    {
        var vm = await seasonService.GetSeasonDetailsAsync(name);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }

    [HttpGet("/seasons/{name}/scoring")]
    public async Task<IActionResult> Scoring(string name)
    {
        var season = await seasonService.GetSeasonDetailsAsync(name);
        if (season == null)
        {
            return NotFound();
        }

        if (!season.ScoringEnabled)
        {
            return NotFound();
        }

        var vm = await scoringService.GetSeasonScoringAsync(season.SeasonId);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }
}