using CloverleafTrack.ViewModels.Admin.Meets;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Admin.ScoringTemplates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class MeetsController(
    IAdminMeetRepository meetRepository,
    IAdminLocationRepository locationRepository,
    IAdminSeasonRepository seasonRepository,
    IAdminScoringTemplateRepository scoringTemplateRepository,
    IAdminMeetParticipantRepository participantRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? searchName, int? seasonId, Environment? environment, MeetEntryStatus? entryStatus)
    {
        var meets = await meetRepository.GetFilteredAsync(searchName, seasonId, environment, entryStatus);
        var seasons = await seasonRepository.GetAllAsync();

        // Group by season and environment
        var seasonGroups = meets
            .GroupBy(m => new { m.SeasonId, m.Season.Name, m.Environment })
            .Select(g => new MeetSeasonGroupViewModel
            {
                SeasonName = g.Key.Name,
                Environment = g.Key.Environment,
                TotalMeets = g.Count(),
                EnteredMeets = g.Count(m => m.EntryStatus == MeetEntryStatus.Entered),
                TotalPerformances = 0,
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
                    PerformanceCount = 0
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
        var viewModel = new MeetFormViewModel { Date = DateTime.Today };
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

        // Auto-assign dual template for Dual/DoubleDual meets
        if (model.MeetType != MeetType.Invitational)
            model.ScoringTemplateId = await GetDualTemplateIdAsync();

        var meet = new Meet
        {
            Name = model.Name,
            Date = model.Date,
            LocationId = model.LocationId,
            Environment = model.Environment,
            HandTimed = model.HandTimed,
            SeasonId = model.SeasonId,
            EntryStatus = model.EntryStatus,
            EntryNotes = model.EntryNotes,
            MeetType = model.MeetType,
            ScoringTemplateId = model.ScoringTemplateId
        };

        var id = await meetRepository.CreateAsync(meet);

        // Create participants
        await SyncParticipants(id, model.ParticipantSchoolNames, new List<int>(), new List<MeetParticipant>());

        TempData["SuccessMessage"] = $"Meet '{model.Name}' created successfully!";

        if (saveAndAddPerformances)
            return RedirectToAction("Create", "Performances", new { meetId = id });

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(id);
        if (meet == null) return NotFound();

        var participants = await participantRepository.GetForMeetAsync(id);

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
            EntryNotes = meet.EntryNotes,
            MeetType = meet.MeetType,
            ScoringTemplateId = meet.ScoringTemplateId,
            ExistingParticipants = participants,
            ParticipantSchoolNames = participants.Select(p => p.SchoolName).ToList(),
            ParticipantIds = participants.Select(p => p.Id).ToList()
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

        // Auto-assign dual template for Dual/DoubleDual meets
        if (model.MeetType != MeetType.Invitational)
            model.ScoringTemplateId = await GetDualTemplateIdAsync();

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
            EntryNotes = model.EntryNotes,
            MeetType = model.MeetType,
            ScoringTemplateId = model.ScoringTemplateId
        };

        await meetRepository.UpdateAsync(meet);

        var existing = await participantRepository.GetForMeetAsync(model.Id);
        await SyncParticipants(model.Id, model.ParticipantSchoolNames, model.ParticipantIds, existing);

        TempData["SuccessMessage"] = "Meet updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var performanceCount = await meetRepository.GetPerformanceCountAsync(id);
        if (performanceCount > 0)
        {
            TempData["ErrorMessage"] = $"Cannot delete meet with {performanceCount} performances. Delete performances first.";
            return RedirectToAction(nameof(Index));
        }

        await meetRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Meet deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(int id)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(id);
        if (meet == null) return NotFound();

        var participants = await participantRepository.GetForMeetAsync(id);

        var viewModel = new MeetFormViewModel
        {
            Name = meet.Name + " (Copy)",
            Date = meet.Date.AddDays(7),
            LocationId = meet.LocationId,
            Environment = meet.Environment,
            HandTimed = meet.HandTimed,
            SeasonId = meet.SeasonId,
            EntryStatus = MeetEntryStatus.NotAvailable,
            MeetType = meet.MeetType,
            ScoringTemplateId = meet.ScoringTemplateId,
            ParticipantSchoolNames = participants.Select(p => p.SchoolName).ToList()
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

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task LoadFormData(MeetFormViewModel viewModel)
    {
        var locations = await locationRepository.GetAllAsync();
        viewModel.Locations = locations.Select(l => new LocationOptionViewModel
        {
            Id = l.Id,
            Name = l.Name,
            City = l.City ?? string.Empty,
            State = l.State ?? string.Empty
        }).ToList();

        var recentLocations = await locationRepository.GetRecentlyUsedAsync(5);
        viewModel.RecentLocations = recentLocations.Select(l => new LocationOptionViewModel
        {
            Id = l.Id,
            Name = l.Name,
            City = l.City ?? string.Empty,
            State = l.State ?? string.Empty
        }).ToList();

        var seasons = await seasonRepository.GetAllAsync();
        viewModel.Seasons = seasons.Select(s => new SeasonOptionViewModel
        {
            Id = s.Id,
            Name = s.Name,
            IsCurrentSeason = s.IsCurrentSeason
        }).ToList();

        var templates = await scoringTemplateRepository.GetAllAsync();
        viewModel.ScoringTemplates = templates.Select(t => new ScoringTemplateOptionViewModel
        {
            Id = t.Id,
            Name = t.Name,
            IsBuiltIn = t.IsBuiltIn
        }).ToList();
    }

    /// <summary>Returns the Id of the built-in dual/double dual scoring template.</summary>
    private async Task<int?> GetDualTemplateIdAsync()
    {
        var templates = await scoringTemplateRepository.GetAllAsync();
        return templates.FirstOrDefault(t => t.IsBuiltIn && t.Name.Contains("Dual"))?.Id;
    }

    /// <summary>
    /// Reconciles the submitted school names with existing participant rows.
    /// Names at index i correspond to ParticipantIds[i] (0 = new row).
    /// </summary>
    private async Task SyncParticipants(
        int meetId,
        List<string> schoolNames,
        List<int> existingIds,
        List<MeetParticipant> currentParticipants)
    {
        for (var i = 0; i < schoolNames.Count; i++)
        {
            var name = schoolNames[i].Trim();
            var existingId = i < existingIds.Count ? existingIds[i] : 0;

            if (string.IsNullOrEmpty(name))
            {
                if (existingId > 0)
                    await participantRepository.DeleteAsync(existingId);
                continue;
            }

            if (existingId > 0)
            {
                var existing = currentParticipants.FirstOrDefault(p => p.Id == existingId);
                if (existing != null && existing.SchoolName != name)
                {
                    existing.SchoolName = name;
                    existing.SortOrder = i;
                    await participantRepository.UpdateAsync(existing);
                }
            }
            else
            {
                await participantRepository.CreateAsync(new MeetParticipant
                {
                    MeetId = meetId,
                    SchoolName = name,
                    SortOrder = i
                });
            }
        }

        // Delete any existing participants not in the new list
        foreach (var p in currentParticipants)
        {
            if (!existingIds.Contains(p.Id))
                await participantRepository.DeleteAsync(p.Id);
        }
    }
}
