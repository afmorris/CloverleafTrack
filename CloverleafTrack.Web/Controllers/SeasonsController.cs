using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class SeasonsController(ISeasonService seasonService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await seasonService.GetSeasonCardsAsync();
        return View(model);
    }
}