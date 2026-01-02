using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class HomeController(IHomeService homeService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var viewModel = await homeService.GetHomePageDataAsync();
        return View(viewModel);
    }
}
