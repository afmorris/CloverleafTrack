using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class MeetsController(ILogger<MeetsController> logger, IMeetService meetService) : Controller
{
    private readonly ILogger<MeetsController> logger = logger;
    private readonly IMeetService meetService = meetService;

    public async Task<IActionResult> Index()
    {
        var vm = await meetService.GetMeetsIndexAsync();
        return View(vm);
    }
    
    [HttpGet("/meets/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var vm = await meetService.GetMeetDetailsAsync(slug);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }
}