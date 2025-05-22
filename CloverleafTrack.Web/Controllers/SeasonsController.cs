using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class SeasonsController(ISeasonService seasonService) : Controller
{
    private readonly ISeasonService seasonService = seasonService;

    public async Task<IActionResult> Index()
    {
        var model = await seasonService.GetSeasonCardsAsync();
        return View(model);
    }
}