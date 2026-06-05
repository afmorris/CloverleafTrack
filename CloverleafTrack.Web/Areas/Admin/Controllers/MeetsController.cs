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
    IAdminMeetParticipantRepository participantRepository,
    IAdminSchoolRepository schoolRepository,
    IAdminMeetTeamResultRepository teamResultRepository) : Controller
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

        await SyncParticipants(id, model.ParticipantSchoolIds, new List<int>(), new List<MeetParticipant>());

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

        var existingResults = await teamResultRepository.GetForMeetAsync(id);

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
            ParticipantSchoolIds = participants.Select(p => p.SchoolId).ToList(),
            ParticipantIds = participants.Select(p => p.Id).ToList(),
            TeamResults = BuildTeamResultRows(meet.MeetType, participants, existingResults)
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
        await SyncParticipants(model.Id, model.ParticipantSchoolIds, model.ParticipantIds, existing);
        await SyncTeamResults(model.Id, model.TeamResults);

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
            ParticipantSchoolIds = participants.Select(p => p.SchoolId).ToList()
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

        var schools = await schoolRepository.GetAllAsync();
        viewModel.Schools = schools.Select(s => new SchoolOptionViewModel
        {
            Id = s.Id,
            Name = s.Name,
            ShortName = s.ShortName
        }).ToList();
    }

    /// <summary>Returns the Id of the built-in dual/double dual scoring template.</summary>
    private async Task<int?> GetDualTemplateIdAsync()
    {
        var templates = await scoringTemplateRepository.GetAllAsync();
        return templates.FirstOrDefault(t => t.IsBuiltIn && t.Name.Contains("Dual"))?.Id;
    }

    /// <summary>Reconciles submitted school IDs with existing participant rows.</summary>
    private async Task SyncParticipants(
        int meetId,
        List<int> schoolIds,
        List<int> existingIds,
        List<MeetParticipant> currentParticipants)
    {
        for (var i = 0; i < schoolIds.Count; i++)
        {
            var schoolId = schoolIds[i];
            var existingId = i < existingIds.Count ? existingIds[i] : 0;

            if (schoolId <= 0)
            {
                if (existingId > 0)
                    await participantRepository.DeleteAsync(existingId);
                continue;
            }

            if (existingId > 0)
            {
                var existing = currentParticipants.FirstOrDefault(p => p.Id == existingId);
                if (existing != null && existing.SchoolId != schoolId)
                {
                    existing.SchoolId = schoolId;
                    existing.SortOrder = i;
                    await participantRepository.UpdateAsync(existing);
                }
            }
            else
            {
                await participantRepository.CreateAsync(new MeetParticipant
                {
                    MeetId = meetId,
                    SchoolId = schoolId,
                    SortOrder = i
                });
            }
        }

        foreach (var p in currentParticipants)
        {
            if (!existingIds.Contains(p.Id))
                await participantRepository.DeleteAsync(p.Id);
        }
    }

    private async Task SyncTeamResults(int meetId, List<MeetTeamResultFormViewModel> rows)
    {
        await teamResultRepository.DeleteAllForMeetAsync(meetId);
        foreach (var r in rows.Where(r => r.OurScore.HasValue || r.OpponentScore.HasValue || r.Place.HasValue))
        {
            await teamResultRepository.CreateAsync(new MeetTeamResult
            {
                MeetId = meetId,
                Gender = r.Gender,
                OpponentMeetParticipantId = r.OpponentMeetParticipantId,
                OurScore = r.OurScore,
                OpponentScore = r.OpponentScore,
                Place = r.Place,
                FieldSize = r.FieldSize
            });
        }
    }

    private static List<MeetTeamResultFormViewModel> BuildTeamResultRows(
        MeetType meetType,
        List<MeetParticipant> participants,
        List<MeetTeamResult> existing)
    {
        var rows = new List<MeetTeamResultFormViewModel>();
        var genders = new[] { Gender.Male, Gender.Female };

        if (meetType == MeetType.Invitational)
        {
            foreach (var gender in genders)
            {
                var ex = existing.FirstOrDefault(r => r.Gender == gender && r.OpponentMeetParticipantId == null);
                rows.Add(new MeetTeamResultFormViewModel
                {
                    Id = ex?.Id ?? 0,
                    Gender = gender,
                    OpponentMeetParticipantId = null,
                    IsInvitational = true,
                    Place = ex?.Place,
                    FieldSize = ex?.FieldSize
                });
            }
        }
        else
        {
            foreach (var participant in participants)
            {
                foreach (var gender in genders)
                {
                    var ex = existing.FirstOrDefault(r => r.Gender == gender && r.OpponentMeetParticipantId == participant.Id);
                    rows.Add(new MeetTeamResultFormViewModel
                    {
                        Id = ex?.Id ?? 0,
                        Gender = gender,
                        OpponentMeetParticipantId = participant.Id,
                        OpponentName = participant.SchoolName,
                        IsInvitational = false,
                        OurScore = ex?.OurScore,
                        OpponentScore = ex?.OpponentScore
                    });
                }
            }
        }

        return rows;
    }
}
