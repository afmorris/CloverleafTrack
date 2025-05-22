using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class MeetsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}