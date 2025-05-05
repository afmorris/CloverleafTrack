using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IRosterService rosterService, ISeasonService seasonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var viewModel = await rosterService.GetRosterAsync(await seasonService.GetCurrentSeasonAsync());
        return View(viewModel);
    }
}