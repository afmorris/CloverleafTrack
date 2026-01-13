using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class EventsController(IAdminEventRepository eventRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var events = await eventRepository.GetAllEventsAsync();

        // Group by gender and environment for better display
        var viewModel = events.GroupBy(e => new { e.Gender, e.Environment })
            .OrderBy(g => g.Key.Gender)
            .ThenByDescending(g => g.Key.Environment)
            .ToDictionary(
                g => $"{g.Key.Gender} {g.Key.Environment}",
                g => g.OrderBy(e => e.EventCategorySortOrder).ThenBy(e => e.SortOrder).ToList()
            );

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Event());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event evt)
    {
        if (!ModelState.IsValid)
        {
            return View(evt);
        }

        // Validate EventKey uniqueness
        var existing = await eventRepository.GetAllEventsAsync();
        if (existing.Any(e => e.EventKey == evt.EventKey))
        {
            ModelState.AddModelError(nameof(evt.EventKey), "EventKey must be unique");
            return View(evt);
        }

        await eventRepository.CreateEventAsync(evt);
        TempData["SuccessMessage"] = $"Event '{evt.Name}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var evt = await eventRepository.GetEventByIdAsync(id);
        if (evt == null)
        {
            return NotFound();
        }

        return View(evt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Event evt)
    {
        if (!ModelState.IsValid)
        {
            return View(evt);
        }

        await eventRepository.UpdateEventAsync(evt);
        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await eventRepository.DeleteEventAsync(id);
        TempData["SuccessMessage"] = "Event deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetByEnvironmentAndGender(Environment environment, Gender? gender)
    {
        var events = await eventRepository.GetEventsByEnvironmentAndGenderAsync(environment, gender);

        var result = events
            .OrderBy(e => e.EventCategorySortOrder)
            .ThenBy(e => e.SortOrder)
            .Select(e => new
            {
                id = e.Id,
                name = e.Name,
                eventType = e.EventType.ToString(),
                athleteCount = e.AthleteCount,
                category = e.EventCategory?.ToString() ?? "Other"
            });

        return Json(result);
    }
}