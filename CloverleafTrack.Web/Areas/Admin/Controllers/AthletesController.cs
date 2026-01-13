using CloverleafTrack.ViewModels.Admin.Athletes;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class AthletesController(
    IAdminAthleteRepository athleteRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? searchName, Gender? gender, bool? isActive, int? graduationYear)
    {
        var athletes = await athleteRepository.GetFilteredAsync(searchName, gender, isActive, graduationYear);
        
        var viewModel = new AthletesIndexViewModel
        {
            Athletes = athletes.Select(a => new AthleteListViewModel
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Gender = a.Gender,
                GraduationYear = a.GraduationYear,
                IsActive = a.IsActive,
                PerformanceCount = 0 // Will be populated via separate calls if needed
            }).ToList(),
            SearchName = searchName,
            FilterGender = gender,
            FilterIsActive = isActive,
            FilterGraduationYear = graduationYear,
            TotalAthletes = athletes.Count,
            ActiveAthletes = athletes.Count(a => a.IsActive),
            GraduatedAthletes = athletes.Count(a => !a.IsActive)
        };
        
        return View(viewModel);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new AthleteFormViewModel();
        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AthleteFormViewModel model, bool saveAndAddAnother = false)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var athlete = new Athlete
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Gender = model.Gender,
            GraduationYear = model.GraduationYear,
            IsActive = model.IsActive
        };
        
        var id = await athleteRepository.CreateAsync(athlete);
        
        if (saveAndAddAnother)
        {
            TempData["SuccessMessage"] = $"Athlete {model.FirstName} {model.LastName} created successfully!";
            return RedirectToAction(nameof(Create));
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var athlete = await athleteRepository.GetByIdAsync(id);
        if (athlete == null)
        {
            return NotFound();
        }
        
        var viewModel = new AthleteFormViewModel
        {
            Id = athlete.Id,
            FirstName = athlete.FirstName,
            LastName = athlete.LastName,
            Gender = athlete.Gender,
            GraduationYear = athlete.GraduationYear,
            IsActive = athlete.IsActive
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AthleteFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var athlete = new Athlete
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Gender = model.Gender,
            GraduationYear = model.GraduationYear,
            IsActive = model.IsActive
        };
        
        await athleteRepository.UpdateAsync(athlete);
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await athleteRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> CheckSimilar(string firstName, string lastName)
    {
        var similar = await athleteRepository.GetSimilarAthletesAsync(firstName, lastName);
        
        var result = similar.Select(a => new
        {
            id = a.Id,
            name = $"{a.FirstName} {a.LastName}",
            graduationYear = a.GraduationYear,
            gender = a.Gender.ToString()
        });
        
        return Json(result);
    }
}
