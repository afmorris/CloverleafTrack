using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.ViewModels.Admin.Schools;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class SchoolsController(IAdminSchoolRepository schoolRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var schools = await schoolRepository.GetAllAsync();
        var viewModel = new SchoolsIndexViewModel
        {
            Schools = schools.Select(s => new SchoolRowViewModel
            {
                Id = s.Id,
                Name = s.Name,
                ShortName = s.ShortName
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create() => View(new SchoolFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SchoolFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await schoolRepository.CreateAsync(new School { Name = model.Name.Trim(), ShortName = model.ShortName?.Trim() });
        TempData["SuccessMessage"] = $"School '{model.Name}' added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var school = await schoolRepository.GetByIdAsync(id);
        if (school == null) return NotFound();

        return View(new SchoolFormViewModel { Id = school.Id, Name = school.Name, ShortName = school.ShortName });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SchoolFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await schoolRepository.UpdateAsync(new School { Id = model.Id, Name = model.Name.Trim(), ShortName = model.ShortName?.Trim() });
        TempData["SuccessMessage"] = "School updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await schoolRepository.DeleteAsync(id);
        TempData["SuccessMessage"] = "School deleted.";
        return RedirectToAction(nameof(Index));
    }
}
