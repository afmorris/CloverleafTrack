using CloverleafTrack.ViewModels.Admin.Performances;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class PerformancesController(
    IAdminPerformanceRepository performanceRepository,
    IAdminMeetRepository meetRepository,
    IAdminEventRepository eventRepository,
    IAdminAthleteRepository athleteRepository) : Controller
{
    private const string SessionKeyMeetId = "PerformanceEntry_MeetId";
    private const string SessionKeyEventId = "PerformanceEntry_EventId";
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var performances = await performanceRepository.GetAllWithDetailsAsync();
        return View(performances);
    }
    
    [HttpGet]
    public async Task<IActionResult> Create(int? meetId, int? eventId)
    {
        // Use provided meetId or get from session
        var selectedMeetId = meetId ?? HttpContext.Session.GetInt32(SessionKeyMeetId);
        var selectedEventId = eventId ?? HttpContext.Session.GetInt32(SessionKeyEventId);
        
        var viewModel = new PerformanceEntryViewModel();
        
        // Load all meets for dropdown
        var meets = await meetRepository.GetAllAsync();
        viewModel.Meets = meets.Select(m => new MeetOptionViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Date = m.Date,
            Environment = m.Environment,
            SeasonName = m.Season.Name
        }).ToList();
        
        // If meet is selected, load context
        if (selectedMeetId.HasValue)
        {
            var meet = await meetRepository.GetByIdWithDetailsAsync(selectedMeetId.Value);
            if (meet != null)
            {
                viewModel.MeetId = meet.Id;
                viewModel.MeetName = meet.Name;
                viewModel.MeetDate = meet.Date;
                
                // Get performance count for this meet
                viewModel.PerformanceCount = await meetRepository.GetPerformanceCountAsync(meet.Id);
                
                // Load events for this meet's environment
                var events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment);
                viewModel.Events = events.Select(e => new EventOptionViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventType = e.EventType,
                    EventCategory = e.EventCategory,
                    Gender = e.Gender,
                    AthleteCount = e.AthleteCount
                }).ToList();
                
                // Store in session for persistence
                HttpContext.Session.SetInt32(SessionKeyMeetId, meet.Id);
            }
        }
        
        // If event is selected, load athletes
        if (selectedEventId.HasValue && selectedMeetId.HasValue)
        {
            var evt = await eventRepository.GetByIdAsync(selectedEventId.Value);
            if (evt != null)
            {
                viewModel.EventId = evt.Id;
                viewModel.EventName = evt.Name;
                viewModel.EventType = evt.EventType;
                viewModel.EventAthleteCount = evt.AthleteCount;
                viewModel.EventGender = evt.Gender;
                
                // Load eligible athletes
                var athletes = await athleteRepository.GetAthletesForMeetAsync(selectedMeetId.Value, evt.Gender);
                viewModel.Athletes = athletes.Select(a => new AthleteOptionViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Gender = a.Gender,
                    GraduationYear = a.GraduationYear
                }).ToList();
                
                // Store in session
                HttpContext.Session.SetInt32(SessionKeyEventId, evt.Id);
            }
        }
        
        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerformanceEntryViewModel model, bool saveAndAddAnother = false)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            await ReloadDropdowns(model);
            return View(model);
        }
        
        // Parse time or distance
        double? timeSeconds = null;
        double? distanceInches = null;
        
        if (!string.IsNullOrWhiteSpace(model.TimeInput))
        {
            timeSeconds = ParseTime(model.TimeInput);
            if (!timeSeconds.HasValue)
            {
                ModelState.AddModelError(nameof(model.TimeInput), "Invalid time format");
                await ReloadDropdowns(model);
                return View(model);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(model.DistanceInput))
        {
            distanceInches = ParseDistance(model.DistanceInput);
            if (!distanceInches.HasValue)
            {
                ModelState.AddModelError(nameof(model.DistanceInput), "Invalid distance format");
                await ReloadDropdowns(model);
                return View(model);
            }
        }
        
        // Check if this is a relay
        var evt = await eventRepository.GetByIdAsync(model.EventId);
        var isRelay = evt?.AthleteCount > 1;
        
        // Create performance
        var performance = new Performance
        {
            MeetId = model.MeetId,
            EventId = model.EventId,
            AthleteId = isRelay ? null : model.AthleteId,
            TimeSeconds = timeSeconds,
            DistanceInches = distanceInches,
            SchoolRecord = false, // TODO: Calculate
            SeasonBest = false, // TODO: Calculate
            PersonalBest = false // TODO: Calculate
        };
        
        var performanceId = await performanceRepository.CreateAsync(performance);
        
        // If relay, add relay athletes
        if (isRelay && model.RelayAthleteIds.Any())
        {
            foreach (var athleteId in model.RelayAthleteIds)
            {
                await performanceRepository.CreatePerformanceAthleteAsync(performanceId, athleteId);
            }
        }
        
        TempData["SuccessMessage"] = "Performance added successfully!";
        
        if (saveAndAddAnother)
        {
            // Keep same meet and event
            return RedirectToAction(nameof(Create), new { meetId = model.MeetId, eventId = model.EventId });
        }
        
        // Clear session
        HttpContext.Session.Remove(SessionKeyMeetId);
        HttpContext.Session.Remove(SessionKeyEventId);
        
        return RedirectToAction("Index", "Dashboard");
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var performance = await performanceRepository.GetByIdAsync(id);
        if (performance == null)
        {
            return NotFound();
        }
        
        // Get meet and event to populate context info
        var meet = await meetRepository.GetByIdAsync(performance.MeetId);
        var evt = await eventRepository.GetByIdAsync(performance.EventId);
        
        var viewModel = new PerformanceEntryViewModel
        {
            Id = performance.Id,
            MeetId = performance.MeetId,
            MeetName = meet?.Name ?? "",
            MeetDate = meet?.Date ?? DateTime.Now,
            EventId = performance.EventId,
            EventName = evt?.Name ?? "",
            EventType = evt?.EventType ?? CloverleafTrack.Models.Enums.EventType.Running,
            EventGender = evt?.Gender,
            EventAthleteCount = evt?.AthleteCount ?? 1,
            AthleteId = performance.AthleteId
        };
        
        // Format the current value for display
        if (performance.TimeSeconds.HasValue)
        {
            viewModel.TimeInput = CloverleafTrack.Web.Utilities.PerformanceFormatHelper.FormatTime(performance.TimeSeconds.Value);
        }
        else if (performance.DistanceInches.HasValue)
        {
            viewModel.DistanceInput = CloverleafTrack.Web.Utilities.PerformanceFormatHelper.FormatDistance(performance.DistanceInches.Value);
        }
        
        // Load ALL recent meets for the dropdown
        var allMeets = await meetRepository.GetAllAsync();
        viewModel.Meets = allMeets.Select(m => new MeetOptionViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Date = m.Date,
            Environment = m.Environment,
            SeasonName = m.Season.Name
        }).ToList();
        
        // Load events for the current meet's environment
        if (meet != null)
        {
            var events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment);
            viewModel.Events = events.Select(e => new EventOptionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                EventType = e.EventType,
                EventCategory = e.EventCategory,
                Gender = e.Gender,
                AthleteCount = e.AthleteCount
            }).ToList();
        }
        
        // Load athletes dropdown (filtered by event and meet date)
        var athletes = await athleteRepository.GetAthletesForMeetAsync(performance.MeetId, evt?.Gender);
        viewModel.Athletes = athletes.Select(a => new AthleteOptionViewModel
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Gender = a.Gender,
            GraduationYear = a.GraduationYear
        }).ToList();
        
        // If relay, load relay athletes
        if (!performance.AthleteId.HasValue && evt?.AthleteCount > 1)
        {
            viewModel.RelayAthleteIds = await performanceRepository.GetAthleteIdsForPerformanceAsync(id);
        }
        
        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PerformanceEntryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload meet and event details
            var meet = await meetRepository.GetByIdAsync(model.MeetId);
            var eventDetails = await eventRepository.GetByIdAsync(model.EventId);
            
            model.MeetName = meet?.Name ?? "";
            model.MeetDate = meet?.Date ?? DateTime.Now;
            model.EventName = eventDetails?.Name ?? "";
            model.EventType = eventDetails?.EventType ?? CloverleafTrack.Models.Enums.EventType.Running;
            model.EventGender = eventDetails?.Gender;
            model.EventAthleteCount = eventDetails?.AthleteCount ?? 1;
            
            // Reload athletes
            var athletes = await athleteRepository.GetAthletesForMeetAsync(model.MeetId, eventDetails?.Gender);
            model.Athletes = athletes.Select(a => new AthleteOptionViewModel
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Gender = a.Gender,
                GraduationYear = a.GraduationYear
            }).ToList();
            
            return View(model);
        }
        
        // Parse time or distance
        double? timeSeconds = null;
        double? distanceInches = null;
        
        if (!string.IsNullOrWhiteSpace(model.TimeInput))
        {
            timeSeconds = CloverleafTrack.Web.Utilities.PerformanceFormatHelper.ParseTime(model.TimeInput);
        }
        
        if (!string.IsNullOrWhiteSpace(model.DistanceInput))
        {
            distanceInches = CloverleafTrack.Web.Utilities.PerformanceFormatHelper.ParseDistance(model.DistanceInput);
        }
        
        var performance = new Performance
        {
            Id = model.Id,
            MeetId = model.MeetId,
            EventId = model.EventId,
            AthleteId = model.AthleteId,
            TimeSeconds = timeSeconds,
            DistanceInches = distanceInches,
            SchoolRecord = false, // TODO: Recalculate
            SeasonBest = false,
            PersonalBest = false
        };
        
        await performanceRepository.UpdateAsync(performance);
        
        // Update relay athletes if needed
        var evt = await eventRepository.GetByIdAsync(model.EventId);
        if (evt?.AthleteCount > 1)
        {
            await performanceRepository.DeletePerformanceAthletesAsync(model.Id);
            foreach (var athleteId in model.RelayAthleteIds)
            {
                await performanceRepository.CreatePerformanceAthleteAsync(model.Id, athleteId);
            }
        }
        
        TempData["SuccessMessage"] = "Performance updated successfully!";
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await performanceRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Performance deleted successfully!";
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public async Task<IActionResult> CheckDuplicate(int meetId, int eventId, int? athleteId)
    {
        var similar = await performanceRepository.GetSimilarPerformanceAsync(meetId, eventId, athleteId);
        
        if (similar != null)
        {
            return Json(new
            {
                exists = true,
                performanceId = similar.Id,
                timeSeconds = similar.TimeSeconds,
                distanceInches = similar.DistanceInches
            });
        }
        
        return Json(new { exists = false });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCurrentPR(int athleteId, int eventId)
    {
        var pr = await performanceRepository.GetBestPerformanceForAthleteEventAsync(athleteId, eventId);
        
        if (pr != null)
        {
            return Json(new
            {
                exists = true,
                timeSeconds = pr.TimeSeconds,
                distanceInches = pr.DistanceInches
            });
        }
        
        return Json(new { exists = false });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetEventsForMeet(int meetId)
    {
        var meet = await meetRepository.GetByIdAsync(meetId);
        if (meet == null)
        {
            return Json(new List<object>());
        }
        
        var events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment);
        return Json(events.Select(e => new
        {
            id = e.Id,
            name = e.Name,
            eventType = e.EventType.ToString(),
            gender = e.Gender?.ToString() ?? "",
            athleteCount = e.AthleteCount
        }));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAthletesForMeetAndEvent(int meetId, int eventId)
    {
        var evt = await eventRepository.GetByIdAsync(eventId);
        if (evt == null)
        {
            return Json(new List<object>());
        }
        
        var athletes = await athleteRepository.GetAthletesForMeetAsync(meetId, evt.Gender);
        return Json(athletes.Select(a => new
        {
            id = a.Id,
            displayText = $"{a.LastName}, {a.FirstName} ({a.GraduationYear})"
        }));
    }
    
    private async Task ReloadDropdowns(PerformanceEntryViewModel model)
    {
        var meets = await meetRepository.GetRecentMeetsAsync(50);
        model.Meets = meets.Select(m => new MeetOptionViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Date = m.Date,
            Environment = m.Environment,
            SeasonName = m.Season.Name
        }).ToList();
        
        if (model.MeetId > 0)
        {
            var meet = await meetRepository.GetByIdWithDetailsAsync(model.MeetId);
            if (meet != null)
            {
                var events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment);
                model.Events = events.Select(e => new EventOptionViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventType = e.EventType,
                    EventCategory = e.EventCategory,
                    Gender = e.Gender,
                    AthleteCount = e.AthleteCount
                }).ToList();
            }
        }
        
        if (model.EventId > 0)
        {
            var evt = await eventRepository.GetByIdAsync(model.EventId);
            if (evt != null)
            {
                var athletes = await athleteRepository.GetAthletesForMeetAsync(model.MeetId, evt.Gender);
                model.Athletes = athletes.Select(a => new AthleteOptionViewModel
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Gender = a.Gender,
                    GraduationYear = a.GraduationYear
                }).ToList();
            }
        }
    }
    
    private static double? ParseTime(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        input = input.Trim().ToLower().Replace("s", "").Replace("m", ":");
        
        // Try simple decimal first (e.g., "11.24")
        if (double.TryParse(input, out var seconds))
        {
            return seconds;
        }
        
        // Try M:SS.ss format (e.g., "1:23.45")
        var colonMatch = Regex.Match(input, @"^(\d+):(\d+\.?\d*)$");
        if (colonMatch.Success)
        {
            var minutes = int.Parse(colonMatch.Groups[1].Value);
            var secs = double.Parse(colonMatch.Groups[2].Value);
            return (minutes * 60) + secs;
        }
        
        return null;
    }
    
    private static double? ParseDistance(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        input = input.Trim().ToLower();
        
        // Remove common words
        input = input.Replace("feet", "'").Replace("foot", "'")
                     .Replace("inches", "\"").Replace("inch", "\"")
                     .Replace(" ", "");
        
        // Try feet'inches" format (e.g., "19'4" or "19'4.5")
        var feetInchMatch = Regex.Match(input, @"^(\d+)'(\d+\.?\d*)");
        if (feetInchMatch.Success)
        {
            var feet = int.Parse(feetInchMatch.Groups[1].Value);
            var inches = double.Parse(feetInchMatch.Groups[2].Value);
            return (feet * 12) + inches;
        }
        
        // Try feet-inches format (e.g., "19-04")
        var dashMatch = Regex.Match(input, @"^(\d+)-(\d+\.?\d*)$");
        if (dashMatch.Success)
        {
            var feet = int.Parse(dashMatch.Groups[1].Value);
            var inches = double.Parse(dashMatch.Groups[2].Value);
            return (feet * 12) + inches;
        }
        
        // Try just inches (e.g., "234.5")
        if (double.TryParse(input.Replace("\"", ""), out var totalInches))
        {
            return totalInches;
        }
        
        return null;
    }
    
    private static string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }
}