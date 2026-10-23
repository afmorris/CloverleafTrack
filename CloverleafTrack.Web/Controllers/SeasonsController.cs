using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.Web.Pdf;
using CloverleafTrack.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class SeasonsController(ISeasonService seasonService, IScoringService scoringService, ISeasonWrapService wrapService, ISeoUrlBuilder seoUrls) : Controller
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

    /// <summary>Season wrap: team highlights, athlete spotlights, scoring and progression, computed live from the season's data (BRAIN.md [C123]).</summary>
    [HttpGet("/seasons/{name}/wrap")]
    public async Task<IActionResult> Wrap(string name)
    {
        var vm = await wrapService.GetSeasonWrapAsync(name);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }

    /// <summary>The wrap as a one-page poster PDF (linked from the wrap page, so the static export picks it up).</summary>
    [HttpGet("/seasons/{name}/wrap.pdf")]
    public async Task<IActionResult> WrapPdf(string name)
    {
        var vm = await wrapService.GetSeasonWrapAsync(name);
        if (vm == null || !vm.HasData)
        {
            return NotFound();
        }

        return File(SeasonWrapPoster.Generate(vm, seoUrls.BaseUrl), "application/pdf");
    }
}
