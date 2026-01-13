using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class LocationsController(IAdminLocationRepository locationRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var locations = await locationRepository.GetAllLocationsAsync();
        return View(locations);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Location { Country = "USA" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Location location)
    {
        if (!ModelState.IsValid)
        {
            return View(location);
        }

        await locationRepository.CreateLocationAsync(location);
        TempData["SuccessMessage"] = $"Location '{location.Name}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var location = await locationRepository.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Location location)
    {
        if (!ModelState.IsValid)
        {
            return View(location);
        }

        await locationRepository.UpdateLocationAsync(location);
        TempData["SuccessMessage"] = "Location updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await locationRepository.DeleteLocationAsync(id);
        TempData["SuccessMessage"] = "Location deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // API endpoint for quick-add from other forms
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickLocationViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest("Location name is required");
        }

        var location = new Location
        {
            Name = model.Name,
            City = model.City,
            State = model.State,
            Country = "USA"
        };

        var id = await locationRepository.CreateLocationAsync(location);

        return Json(new
        {
            id,
            name = location.Name,
            displayText = !string.IsNullOrEmpty(location.City) && !string.IsNullOrEmpty(location.State)
                ? $"{location.Name} ({location.City}, {location.State})"
                : location.Name
        });
    }
}

public class QuickLocationViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
}