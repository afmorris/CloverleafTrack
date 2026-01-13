using CloverleafTrack.ViewModels.Admin;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class MeetsController(
    IAdminMeetRepository meetRepository,
    IAdminLocationRepository locationRepository,
    IAdminSeasonRepository seasonRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? searchName, int? seasonId, Environment? environment, MeetEntryStatus? entryStatus)
    {
        var meets = await meetRepository.GetMeetsByFiltersAsync(searchName, seasonId, environment, entryStatus);
        var seasons = await seasonRepository.GetAllSeasonsAsync();

        // Group by season and environment
        var seasonGroups = meets
            .GroupBy(m => new { m.SeasonId, m.Season.Name, m.Environment })
            .Select(g => new MeetSeasonGroupViewModel
            {
                SeasonName = g.Key.Name,
                Environment = g.Key.Environment,
                TotalMeets = g.Count(),
                EnteredMeets = g.Count(m => m.EntryStatus == MeetEntryStatus.Entered),
                TotalPerformances = 0, // Will be calculated if needed
                Meets = g.Select(m => new MeetListViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Date = m.Date,
                    LocationName = m.Location.Name,
                    LocationCity = m.Location.City ?? string.Empty,
                    LocationState = m.Location.State ?? string.Empty,
                    Environment = m.Environment,
                    EntryStatus = m.EntryStatus,
                    SeasonName = m.Season.Name,
                    PerformanceCount = 0 // Will be loaded separately if needed
                }).OrderByDescending(m => m.Date).ToList()
            })
            .OrderByDescending(g => g.SeasonName)
            .ThenByDescending(g => g.Environment)
            .ToList();

        var viewModel = new MeetsIndexViewModel
        {
            SeasonGroups = seasonGroups,
            SearchName = searchName,
            FilterSeasonId = seasonId,
            FilterEnvironment = environment,
            FilterEntryStatus = entryStatus,
            Seasons = seasons.Select(s => new SeasonOptionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                IsCurrentSeason = s.IsCurrentSeason
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new MeetFormViewModel
        {
            Date = DateTime.Today
        };

        await LoadFormData(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeetFormViewModel model, bool saveAndAddPerformances = false)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormData(model);
            return View(model);
        }

        var meet = new Meet
        {
            Name = model.Name,
            Date = model.Date,
            LocationId = model.LocationId,
            Environment = model.Environment,
            HandTimed = model.HandTimed,
            SeasonId = model.SeasonId,
            EntryStatus = model.EntryStatus,
            EntryNotes = model.EntryNotes
        };

        var id = await meetRepository.CreateMeetAsync(meet);
        TempData["SuccessMessage"] = $"Meet '{model.Name}' created successfully!";

        if (saveAndAddPerformances)
        {
            return RedirectToAction("Create", "Performances", new { meetId = id });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var meet = await meetRepository.GetMeetByIdAsync(id);
        if (meet == null)
        {
            return NotFound();
        }

        var viewModel = new MeetFormViewModel
        {
            Id = meet.Id,
            Name = meet.Name,
            Date = meet.Date,
            LocationId = meet.LocationId,
            Environment = meet.Environment,
            HandTimed = meet.HandTimed,
            SeasonId = meet.SeasonId,
            EntryStatus = meet.EntryStatus,
            EntryNotes = meet.EntryNotes
        };

        await LoadFormData(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MeetFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormData(model);
            return View(model);
        }

        var meet = new Meet
        {
            Id = model.Id,
            Name = model.Name,
            Date = model.Date,
            LocationId = model.LocationId,
            Environment = model.Environment,
            HandTimed = model.HandTimed,
            SeasonId = model.SeasonId,
            EntryStatus = model.EntryStatus,
            EntryNotes = model.EntryNotes
        };

        await meetRepository.UpdateMeetAsync(meet);

        TempData["SuccessMessage"] = "Meet updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Check if meet has performances
        var performanceCount = await meetRepository.GetPerformanceCountAsync(id);
        if (performanceCount > 0)
        {
            TempData["ErrorMessage"] = $"Cannot delete meet with {performanceCount} performances. Delete performances first.";
            return RedirectToAction(nameof(Index));
        }

        await meetRepository.DeleteMeetAsync(id);
        TempData["SuccessMessage"] = "Meet deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(int id)
    {
        var meet = await meetRepository.GetMeetWithDetailsAsync(id);
        if (meet == null)
        {
            return NotFound();
        }

        var viewModel = new MeetFormViewModel
        {
            Name = meet.Name + " (Copy)",
            Date = meet.Date.AddDays(7), // Default to one week later
            LocationId = meet.LocationId,
            Environment = meet.Environment,
            HandTimed = meet.HandTimed,
            SeasonId = meet.SeasonId,
            EntryStatus = MeetEntryStatus.NotAvailable
        };

        await LoadFormData(viewModel);
        return View("Create", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AutoDetectSeason(DateTime date)
    {
        var season = await seasonRepository.GetSeasonForDateAsync(date);

        if (season != null)
        {
            // Auto-detect environment based on date
            var month = date.Month;
            var suggestedEnvironment = (month >= 3 && month <= 10)
                ? Environment.Outdoor
                : Environment.Indoor;

            return Json(new
            {
                seasonId = season.Id,
                seasonName = season.Name,
                suggestedEnvironment = suggestedEnvironment.ToString()
            });
        }

        return Json(new { seasonId = (int?)null });
    }

    private async Task LoadFormData(MeetFormViewModel viewModel)
    {
        var locations = await locationRepository.GetAllLocationsAsync();
        viewModel.Locations = locations.Select(l => new LocationOptionViewModel
        {
            Id = l.Id,
            Name = l.Name,
            City = l.City ?? string.Empty,
            State = l.State ?? string.Empty
        }).ToList();

        var recentLocations = await locationRepository.GetRecentLocationsAsync(5);
        viewModel.RecentLocations = recentLocations.Select(l => new LocationOptionViewModel
        {
            Id = l.Id,
            Name = l.Name,
            City = l.City ?? string.Empty,
            State = l.State ?? string.Empty
        }).ToList();

        var seasons = await seasonRepository.GetAllSeasonsAsync();
        viewModel.Seasons = seasons.Select(s => new SeasonOptionViewModel
        {
            Id = s.Id,
            Name = s.Name,
            IsCurrentSeason = s.IsCurrentSeason
        }).ToList();
    }
}