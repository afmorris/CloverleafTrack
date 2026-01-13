using CloverleafTrack.ViewModels.Admin.Dashboard;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController(
    IAdminAthleteRepository athleteRepository,
    IAdminMeetRepository meetRepository,
    IAdminSeasonRepository seasonRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel();
        
        // Get basic stats
        var allAthletes = await athleteRepository.GetAllAsync();
        viewModel.TotalAthletes = allAthletes.Count;
        
        var allMeets = await meetRepository.GetAllAsync();
        viewModel.TotalMeets = allMeets.Count;
        
        // Count incomplete meets (not entered)
        viewModel.IncompleteMeets = allMeets.Count(m => m.EntryStatus != MeetEntryStatus.Entered);
        
        // Calculate total performances (approximate - sum performance counts)
        viewModel.TotalPerformances = 0;
        foreach (var meet in allMeets)
        {
            viewModel.TotalPerformances += await meetRepository.GetPerformanceCountAsync(meet.Id);
        }
        
        // Get season progress
        var seasons = await seasonRepository.GetAllAsync();
        foreach (var season in seasons.OrderByDescending(s => s.StartDate).Take(3))
        {
            var seasonMeets = allMeets.Where(m => m.SeasonId == season.Id).ToList();
            var enteredMeets = seasonMeets.Count(m => m.EntryStatus == MeetEntryStatus.Entered);
            
            viewModel.SeasonProgress.Add(new SeasonProgressViewModel
            {
                SeasonName = season.Name,
                TotalMeets = seasonMeets.Count,
                EnteredMeets = enteredMeets,
                IsCurrentSeason = season.IsCurrentSeason
            });
        }
        
        // Generate data quality issues
        var meetsWithoutPerformances = 0;
        foreach (var meet in allMeets.Where(m => m.EntryStatus == MeetEntryStatus.Entered))
        {
            var perfCount = await meetRepository.GetPerformanceCountAsync(meet.Id);
            if (perfCount == 0)
            {
                meetsWithoutPerformances++;
            }
        }
        
        if (meetsWithoutPerformances > 0)
        {
            viewModel.DataQualityIssues.Add(new DataQualityIssueViewModel
            {
                Type = "warning",
                Count = meetsWithoutPerformances,
                Description = "meets marked as 'Entered' have zero performances",
                ActionLink = "/Admin/Meets"
            });
        }
        
        var notAvailableMeets = allMeets.Count(m => m.EntryStatus == MeetEntryStatus.NotAvailable);
        if (notAvailableMeets > 0)
        {
            viewModel.DataQualityIssues.Add(new DataQualityIssueViewModel
            {
                Type = "warning",
                Count = notAvailableMeets,
                Description = "meets have no results available",
                ActionLink = "/Admin/Meets"
            });
        }
        
        return View(viewModel);
    }
}
