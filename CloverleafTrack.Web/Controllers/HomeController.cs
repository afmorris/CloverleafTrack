using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> logger = logger;

    public IActionResult Index()
    {
        return View();
    }
}