using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels;
using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Web.Controllers;

public class RosterController(IAthleteService athleteService, ISeasonService seasonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new RosterViewModel
        {
            ActiveAthletes = await athleteService.GetActiveAthletesGroupedByEventCategoryAsync(await seasonService.GetCurrentSeasonAsync()),
            FormerAthletes = await athleteService.GetFormerAthletesGroupedByGraduationYearAsync()
        };
        
        return View(vm);
    }

    
}