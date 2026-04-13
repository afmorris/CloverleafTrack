using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Admin.MeetEntries;
using CloverleafTrack.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class MeetEntriesController(
    IAdminMeetEntryRepository meetEntryRepository,
    IAdminMeetPlacingRepository meetPlacingRepository,
    IAdminMeetParticipantRepository participantRepository,
    IAdminMeetRepository meetRepository,
    IAdminEventRepository eventRepository,
    IAdminAthleteRepository athleteRepository,
    IAdminPerformanceRepository performanceRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int meetId)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(meetId);
        if (meet == null) return NotFound();

        var entries = await meetEntryRepository.GetForMeetAsync(meetId);
        var participants = await participantRepository.GetForMeetAsync(meetId);
        var placings = await meetPlacingRepository.GetForMeetAsync(meetId);
        var placingLookup = placings.GroupBy(p => p.PerformanceId)
                                    .ToDictionary(g => g.Key, g => g.ToList());

        // For 4-event limit: count entries per athlete
        var athleteEventCounts = new Dictionary<int, int>();
        foreach (var entry in entries.Where(e => e.AthleteId.HasValue))
            athleteEventCounts[entry.AthleteId!.Value] =
                await meetEntryRepository.GetAthleteEventCountForMeetAsync(meetId, entry.AthleteId!.Value);

        var vm = new MeetEntriesIndexViewModel
        {
            MeetId = meetId,
            MeetName = meet.Name,
            MeetSlug = meet.Slug,
            MeetDate = meet.Date,
            MeetType = meet.MeetType,
            Participants = participants
        };

        foreach (var group in entries.GroupBy(e => new { e.EventGender, e.EventId, e.EventName, e.EventCategory, e.EventType }))
        {
            var eventGroup = new MeetEntryEventGroupViewModel
            {
                EventId = group.Key.EventId,
                EventName = group.Key.EventName,
                EventCategory = group.Key.EventCategory,
                EventType = group.Key.EventType,
                Entries = group.Select(e =>
                {
                    List<EntryPlacingDisplayViewModel> entryPlacings = new();
                    if (e.PerformanceId.HasValue && placingLookup.TryGetValue(e.PerformanceId.Value, out var perfPlacings))
                    {
                        entryPlacings = perfPlacings.Select(pl => new EntryPlacingDisplayViewModel
                        {
                            Place = pl.Place,
                            OpponentSchoolName = pl.MeetParticipant?.SchoolName
                        }).ToList();
                    }

                    var exceedsLimit = e.AthleteId.HasValue && athleteEventCounts.TryGetValue(e.AthleteId.Value, out var count) && count > 4;

                    string? formattedResult = null;
                    if (e.PerformanceId.HasValue)
                    {
                        var isField = e.EventType is EventType.Field or EventType.FieldRelay
                                                     or EventType.JumpRelay or EventType.ThrowsRelay;
                        formattedResult = isField
                            ? (e.DistanceInches.HasValue ? PerformanceFormatHelper.FormatDistance(e.DistanceInches.Value) : null)
                            : (e.TimeSeconds.HasValue    ? PerformanceFormatHelper.FormatTime(e.TimeSeconds.Value)         : null);
                    }

                    return new MeetEntryRowViewModel
                    {
                        EntryId = e.Id,
                        AthleteDisplay = e.AthleteDisplayName,
                        IsRelay = e.IsRelay,
                        HasResult = e.PerformanceId.HasValue,
                        FormattedResult = formattedResult,
                        HasPlacing = entryPlacings.Count > 0,
                        Placings = entryPlacings,
                        ExceedsEventLimit = exceedsLimit
                    };
                }).ToList()
            };

            if (group.Key.EventGender == Gender.Male)
                vm.BoysGroups.Add(eventGroup);
            else if (group.Key.EventGender == Gender.Female)
                vm.GirlsGroups.Add(eventGroup);
            else
                vm.MixedGroups.Add(eventGroup);
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> AddEntry(int meetId)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(meetId);
        if (meet == null) return NotFound();

        var events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment);
        var vm = new AddEntryViewModel
        {
            MeetId = meetId,
            MeetName = meet.Name,
            Events = events.Select(e => new MeetEntryEventOptionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                EventType = e.EventType,
                AthleteCount = e.AthleteCount,
                Gender = e.Gender.Value
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(AddEntryViewModel model)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(model.MeetId);
        if (meet == null) return NotFound();

        var evt = await eventRepository.GetByIdAsync(model.EventId);
        if (evt == null) return NotFound();

        var isRelay = evt.AthleteCount > 1;

        var entry = new MeetEntry
        {
            MeetId = model.MeetId,
            EventId = model.EventId,
            AthleteId = isRelay ? null : model.AthleteId
        };

        var entryId = await meetEntryRepository.CreateAsync(entry);

        if (isRelay)
        {
            var relayIds = model.RelayAthleteIds
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .ToList();
            if (relayIds.Count > 0)
                await meetEntryRepository.AddRelayAthletesAsync(entryId, relayIds);
        }

        TempData["SuccessMessage"] = "Entry added.";
        return RedirectToAction(nameof(Index), new { meetId = model.MeetId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAthletesForEvent(int meetId, int eventId)
    {
        var evt = await eventRepository.GetByIdAsync(eventId);
        if (evt == null) return Json(new List<object>());

        var athletes = await athleteRepository.GetAthletesForMeetAsync(meetId, evt.Gender);
        return Json(athletes.Select(a => new { id = a.Id, displayText = $"{a.LastName}, {a.FirstName}" }));
    }

    [HttpGet]
    public async Task<IActionResult> EnterResult(int id)
    {
        var entry = await meetEntryRepository.GetByIdAsync(id);
        if (entry == null) return NotFound();

        var meet = await meetRepository.GetByIdWithDetailsAsync(entry.MeetId);
        if (meet == null) return NotFound();

        var participants = await participantRepository.GetForMeetAsync(entry.MeetId);

        var vm = new EnterResultViewModel
        {
            EntryId = id,
            MeetId = entry.MeetId,
            MeetName = entry.EventName,
            EventId = entry.EventId,
            EventName = entry.EventName,
            EventType = entry.EventType,
            AthleteDisplay = entry.AthleteDisplayName,
            IsRelay = entry.IsRelay,
            MeetType = meet.MeetType,
            Participants = participants
        };

        // Build place input rows
        if (meet.MeetType == MeetType.Invitational)
        {
            vm.PlaceInputs.Add(new PlaceInputRow { MeetParticipantId = null, OpponentLabel = "Overall Place" });
        }
        else
        {
            foreach (var p in participants)
                vm.PlaceInputs.Add(new PlaceInputRow { MeetParticipantId = p.Id, OpponentLabel = $"vs. {p.SchoolName}" });
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterResult(EnterResultViewModel model)
    {
        var entry = await meetEntryRepository.GetByIdAsync(model.EntryId);
        if (entry == null) return NotFound();

        var evt = await eventRepository.GetByIdAsync(entry.EventId);
        if (evt == null) return NotFound();

        // Parse performance
        double? timeSeconds = null;
        double? distanceInches = null;
        var isField = evt.EventType is EventType.Field or EventType.FieldRelay
                                      or EventType.JumpRelay or EventType.ThrowsRelay;

        if (isField)
        {
            distanceInches = PerformanceFormatHelper.ParseDistance(model.DistanceInput);
            if (!distanceInches.HasValue && !string.IsNullOrWhiteSpace(model.DistanceInput))
            {
                ModelState.AddModelError(nameof(model.DistanceInput), "Invalid distance format.");
                await ReloadEnterResultForm(model);
                return View(model);
            }
        }
        else
        {
            timeSeconds = PerformanceFormatHelper.ParseTime(model.TimeInput);
            if (!timeSeconds.HasValue && !string.IsNullOrWhiteSpace(model.TimeInput))
            {
                ModelState.AddModelError(nameof(model.TimeInput), "Invalid time format.");
                await ReloadEnterResultForm(model);
                return View(model);
            }
        }

        // Create the performance
        var performance = new Performance
        {
            MeetId = entry.MeetId,
            EventId = entry.EventId,
            AthleteId = entry.IsRelay ? null : entry.AthleteId,
            TimeSeconds = timeSeconds,
            DistanceInches = distanceInches
        };

        var performanceId = await performanceRepository.CreateAsync(performance);

        // For relay: insert performance athletes from the pre-registered relay team
        if (entry.IsRelay)
        {
            var relayAthleteIds = await meetEntryRepository.GetRelayAthleteIdsAsync(model.EntryId);
            foreach (var athleteId in relayAthleteIds)
                await performanceRepository.CreatePerformanceAthleteAsync(performanceId, athleteId);
        }

        // Link the performance back to the entry
        await meetEntryRepository.UpdatePerformanceIdAsync(model.EntryId, performanceId);

        // Create placings for each input that has a value
        foreach (var placeInput in model.PlaceInputs.Where(p => p.Place.HasValue && p.Place.Value > 0))
        {
            var templatePoints = await meetPlacingRepository.GetTemplatePointsAsync(
                entry.MeetId, entry.EventId, placeInput.Place!.Value);

            var fullPoints = templatePoints;
            var splitPoints = entry.IsRelay && evt.AthleteCount > 1
                ? templatePoints / evt.AthleteCount
                : templatePoints;

            await meetPlacingRepository.UpsertAsync(new MeetPlacing
            {
                MeetId = entry.MeetId,
                PerformanceId = performanceId,
                MeetParticipantId = placeInput.MeetParticipantId,
                Place = placeInput.Place!.Value,
                FullPoints = fullPoints,
                SplitPoints = splitPoints
            });
        }

        TempData["SuccessMessage"] = "Result entered successfully!";
        return RedirectToAction(nameof(Index), new { meetId = entry.MeetId });
    }

    [HttpGet]
    public async Task<IActionResult> EditResult(int id)
    {
        var entry = await meetEntryRepository.GetByIdAsync(id);
        if (entry == null || !entry.PerformanceId.HasValue) return NotFound();

        var meet = await meetRepository.GetByIdWithDetailsAsync(entry.MeetId);
        if (meet == null) return NotFound();

        var performance = await performanceRepository.GetByIdAsync(entry.PerformanceId.Value);
        if (performance == null) return NotFound();

        var isField = entry.EventType is EventType.Field or EventType.FieldRelay
                                        or EventType.JumpRelay or EventType.ThrowsRelay;

        var vm = new EnterResultViewModel
        {
            EntryId = id,
            MeetId = entry.MeetId,
            EventId = entry.EventId,
            EventName = entry.EventName,
            EventType = entry.EventType,
            AthleteDisplay = entry.AthleteDisplayName,
            IsRelay = entry.IsRelay,
            MeetType = meet.MeetType,
            TimeInput = !isField && performance.TimeSeconds.HasValue
                ? PerformanceFormatHelper.FormatTime(performance.TimeSeconds.Value)
                : null,
            DistanceInput = isField && performance.DistanceInches.HasValue
                ? PerformanceFormatHelper.FormatDistance(performance.DistanceInches.Value)
                : null
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResult(EnterResultViewModel model)
    {
        var entry = await meetEntryRepository.GetByIdAsync(model.EntryId);
        if (entry == null || !entry.PerformanceId.HasValue) return NotFound();

        var performance = await performanceRepository.GetByIdAsync(entry.PerformanceId.Value);
        if (performance == null) return NotFound();

        var evt = await eventRepository.GetByIdAsync(entry.EventId);
        if (evt == null) return NotFound();

        double? timeSeconds = null;
        double? distanceInches = null;
        var isField = evt.EventType is EventType.Field or EventType.FieldRelay
                                      or EventType.JumpRelay or EventType.ThrowsRelay;

        if (isField)
        {
            distanceInches = PerformanceFormatHelper.ParseDistance(model.DistanceInput);
            if (!distanceInches.HasValue && !string.IsNullOrWhiteSpace(model.DistanceInput))
            {
                ModelState.AddModelError(nameof(model.DistanceInput), "Invalid distance format.");
                return View(model);
            }
        }
        else
        {
            timeSeconds = PerformanceFormatHelper.ParseTime(model.TimeInput);
            if (!timeSeconds.HasValue && !string.IsNullOrWhiteSpace(model.TimeInput))
            {
                ModelState.AddModelError(nameof(model.TimeInput), "Invalid time format.");
                return View(model);
            }
        }

        performance.TimeSeconds = timeSeconds;
        performance.DistanceInches = distanceInches;
        await performanceRepository.UpdateAsync(performance);

        TempData["SuccessMessage"] = "Result updated successfully!";
        return RedirectToAction(nameof(Index), new { meetId = entry.MeetId });
    }

    [HttpGet]
    public async Task<IActionResult> EnterPlacing(int id)
    {
        var entry = await meetEntryRepository.GetByIdAsync(id);
        if (entry == null || !entry.PerformanceId.HasValue) return NotFound();

        var meet = await meetRepository.GetByIdWithDetailsAsync(entry.MeetId);
        if (meet == null) return NotFound();

        var evt = await eventRepository.GetByIdAsync(entry.EventId);
        if (evt == null) return NotFound();

        var participants = await participantRepository.GetForMeetAsync(entry.MeetId);

        var isField = evt.EventType is EventType.Field or EventType.FieldRelay
                                       or EventType.JumpRelay or EventType.ThrowsRelay;
        var formattedResult = isField
            ? (entry.DistanceInches.HasValue ? PerformanceFormatHelper.FormatDistance(entry.DistanceInches.Value) : string.Empty)
            : (entry.TimeSeconds.HasValue    ? PerformanceFormatHelper.FormatTime(entry.TimeSeconds.Value)         : string.Empty);

        var vm = new EnterPlacingViewModel
        {
            EntryId = id,
            MeetId = entry.MeetId,
            PerformanceId = entry.PerformanceId.Value,
            EventName = entry.EventName,
            AthleteDisplay = entry.AthleteDisplayName,
            FormattedResult = formattedResult,
            IsRelay = entry.IsRelay,
            MeetType = meet.MeetType,
            Participants = participants
        };

        if (meet.MeetType == MeetType.Invitational)
        {
            vm.PlaceInputs.Add(new PlaceInputRow { MeetParticipantId = null, OpponentLabel = "Overall Place" });
        }
        else
        {
            foreach (var p in participants)
                vm.PlaceInputs.Add(new PlaceInputRow { MeetParticipantId = p.Id, OpponentLabel = $"vs. {p.SchoolName}" });
        }

        // Pre-fill existing placing values
        var existingPlacings = await meetPlacingRepository.GetForMeetAsync(entry.MeetId);
        var perfPlacings = existingPlacings.Where(p => p.PerformanceId == entry.PerformanceId.Value).ToList();

        foreach (var input in vm.PlaceInputs)
        {
            var existing = perfPlacings.FirstOrDefault(p => p.MeetParticipantId == input.MeetParticipantId);
            if (existing != null)
                input.Place = existing.Place;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterPlacing(EnterPlacingViewModel model)
    {
        var entry = await meetEntryRepository.GetByIdAsync(model.EntryId);
        if (entry == null || !entry.PerformanceId.HasValue) return NotFound();

        var evt = await eventRepository.GetByIdAsync(entry.EventId);
        if (evt == null) return NotFound();

        foreach (var placeInput in model.PlaceInputs.Where(p => p.Place.HasValue && p.Place.Value > 0))
        {
            var templatePoints = await meetPlacingRepository.GetTemplatePointsAsync(
                entry.MeetId, entry.EventId, placeInput.Place!.Value);

            var splitPoints = entry.IsRelay && evt.AthleteCount > 1
                ? templatePoints / evt.AthleteCount
                : templatePoints;

            await meetPlacingRepository.UpsertAsync(new MeetPlacing
            {
                MeetId = entry.MeetId,
                PerformanceId = entry.PerformanceId.Value,
                MeetParticipantId = placeInput.MeetParticipantId,
                Place = placeInput.Place!.Value,
                FullPoints = templatePoints,
                SplitPoints = splitPoints
            });
        }

        TempData["SuccessMessage"] = "Placing saved!";
        return RedirectToAction(nameof(Index), new { meetId = entry.MeetId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var entry = await meetEntryRepository.GetByIdAsync(id);
        if (entry == null) return NotFound();

        var meetId = entry.MeetId;
        await meetEntryRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Entry removed.";
        return RedirectToAction(nameof(Index), new { meetId });
    }

    private async Task ReloadEnterResultForm(EnterResultViewModel model)
    {
        var participants = await participantRepository.GetForMeetAsync(model.MeetId);
        model.Participants = participants;
    }
}
