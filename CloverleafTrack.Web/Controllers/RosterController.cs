using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using CloverleafTrack.Web.Pdf;
using CloverleafTrack.Web.Utilities;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IAthleteService athleteService, ISeasonService seasonService, IAthleteWrapService wrapService, ISeoUrlBuilder seoUrls) : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentSeason = await seasonService.GetCurrentSeasonAsync();
        var vm = new RosterViewModel
        {
            ActiveAthletes = await athleteService.GetActiveAthletesGroupedByEventCategoryAsync(currentSeason),
            FlatActiveAthletes = await athleteService.GetFlatActiveAthletesAsync(currentSeason),
            FormerAthletes = await athleteService.GetFormerAthletesGroupedByGraduationYearAsync()
        };
        return View(vm);
    }

    [HttpGet("/roster/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var currentSeason = await seasonService.GetCurrentSeasonAsync();
        var vm = await athleteService.GetAthleteDetailsAsync(slug, currentSeason);

        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }

    /// <summary>Athlete career wrap: highlights, biggest improvements, season by season (BRAIN.md [C126]).</summary>
    [HttpGet("/roster/{slug}/wrap")]
    public async Task<IActionResult> Wrap(string slug)
    {
        var vm = await wrapService.GetAthleteWrapAsync(slug);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }

    /// <summary>The wrap as a one-page poster PDF (linked from the wrap page, so the static export picks it up).</summary>
    [HttpGet("/roster/{slug}/wrap.pdf")]
    public async Task<IActionResult> WrapPdf(string slug)
    {
        var vm = await wrapService.GetAthleteWrapAsync(slug);
        if (vm == null || !vm.HasData)
        {
            return NotFound();
        }

        return File(AthleteWrapPoster.Generate(vm, seoUrls.BaseUrl), "application/pdf");
    }
}
