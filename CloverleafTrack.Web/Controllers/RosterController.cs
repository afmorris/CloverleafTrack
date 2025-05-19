using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IAthleteService athleteService, IRosterService rosterService, ISeasonService seasonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new RosterViewModel
        {
            ActiveAthletes = await athleteService.GetActiveAthletesGroupedByEventCategoryAsync(await seasonService.GetCurrentSeasonAsync()),
            FormerAthletes = await athleteService.GetFormerAthletesGroupedByEventCategoryAsync(await seasonService.GetCurrentSeasonAsync())
        };
        
        return View(vm);
    }
}