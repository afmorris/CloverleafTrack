using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class AthletesController(IAthleteService athleteService, ISeasonService seasonService) : Controller
{
    [HttpGet("/athletes/{slug}")]
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