using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class MeetsController(ILogger<MeetsController> logger, IMeetService meetService) : Controller
{
    private readonly ILogger<MeetsController> logger = logger;
    private readonly IMeetService meetService = meetService;
    
    [HttpGet("/meets/{name}")]
    public async Task<IActionResult> Details(string name)
    {
        var vm = await meetService.GetMeetDetailsAsync(name);
        if (vm == null)
        {
            return NotFound();
        }

        return View(vm);
    }
}