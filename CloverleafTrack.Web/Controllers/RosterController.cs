using CloverleafTrack.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

public class RosterController : Controller
{
    private readonly IAthleteRepository athleteRepository;

    public RosterController(IAthleteRepository athleteRepository)
    {
        this.athleteRepository = athleteRepository;
    }

    public IActionResult Index()
    {
        return View();
    }
}