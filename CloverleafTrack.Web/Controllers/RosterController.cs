using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IAthleteService athleteService, ISeasonService seasonService) : Controller
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
}
