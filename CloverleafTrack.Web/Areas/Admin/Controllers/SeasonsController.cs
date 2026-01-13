using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class SeasonsController(IAdminSeasonRepository seasonRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var seasons = await seasonRepository.GetAllAsync();
        return View(seasons);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        var season = new Season
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(6),
            Status = SeasonStatus.Draft
        };
        return View(season);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Season season, bool setAsCurrent = false)
    {
        if (!ModelState.IsValid)
        {
            return View(season);
        }
        
        // If setting as current, unset all other current seasons first
        if (setAsCurrent)
        {
            var allSeasons = await seasonRepository.GetAllAsync();
            foreach (var s in allSeasons.Where(s => s.IsCurrentSeason))
            {
                s.IsCurrentSeason = false;
                await seasonRepository.UpdateAsync(s);
            }
            season.IsCurrentSeason = true;
        }
        
        await seasonRepository.CreateAsync(season);
        TempData["SuccessMessage"] = $"Season '{season.Name}' created successfully!";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var season = await seasonRepository.GetByIdAsync(id);
        if (season == null)
        {
            return NotFound();
        }
        
        return View(season);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Season season, bool setAsCurrent = false)
    {
        if (!ModelState.IsValid)
        {
            return View(season);
        }
        
        // If setting as current, unset all other current seasons first
        if (setAsCurrent && !season.IsCurrentSeason)
        {
            var allSeasons = await seasonRepository.GetAllAsync();
            foreach (var s in allSeasons.Where(s => s.IsCurrentSeason && s.Id != season.Id))
            {
                s.IsCurrentSeason = false;
                await seasonRepository.UpdateAsync(s);
            }
            season.IsCurrentSeason = true;
        }
        
        await seasonRepository.UpdateAsync(season);
        TempData["SuccessMessage"] = "Season updated successfully!";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var season = await seasonRepository.GetByIdAsync(id);
        if (season?.IsCurrentSeason == true)
        {
            TempData["ErrorMessage"] = "Cannot delete the current season. Set another season as current first.";
            return RedirectToAction(nameof(Index));
        }
        
        await seasonRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "Season deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCurrent(int id)
    {
        // Unset all current seasons
        var allSeasons = await seasonRepository.GetAllAsync();
        foreach (var s in allSeasons.Where(s => s.IsCurrentSeason))
        {
            s.IsCurrentSeason = false;
            await seasonRepository.UpdateAsync(s);
        }
        
        // Set the new current season
        var season = await seasonRepository.GetByIdAsync(id);
        if (season != null)
        {
            season.IsCurrentSeason = true;
            await seasonRepository.UpdateAsync(season);
            TempData["SuccessMessage"] = $"'{season.Name}' is now the current season.";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
