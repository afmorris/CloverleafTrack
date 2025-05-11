using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IAthleteService athleteService, IRosterService rosterService, ISeasonService seasonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var viewModel = await athleteService.GetActiveAthletesGroupedByEventCategoryAsync(await seasonService.GetCurrentSeasonAsync());
        return View(viewModel);
    }
}