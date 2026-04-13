using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.ViewModels.Admin.ScoringTemplates;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

[Area("Admin")]
public class ScoringTemplatesController(IAdminScoringTemplateRepository templateRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await templateRepository.GetAllAsync();
        return View(new ScoringTemplateIndexViewModel { Templates = templates });
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new ScoringTemplateFormViewModel
        {
            Places = Enumerable.Range(1, 8).Select(i => new ScoringTemplatePlaceFormRow
            {
                Place = i,
                Points = 0
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScoringTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var template = new ScoringTemplate { Name = model.Name };
        var id = await templateRepository.CreateAsync(template);

        foreach (var row in model.Places.Where(p => p.Points > 0))
        {
            await templateRepository.AddPlaceAsync(new ScoringTemplatePlace
            {
                ScoringTemplateId = id,
                Place = row.Place,
                Points = row.Points
            });
        }

        TempData["SuccessMessage"] = $"Scoring template '{model.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var template = await templateRepository.GetByIdAsync(id);
        if (template == null) return NotFound();

        var vm = new ScoringTemplateFormViewModel
        {
            Id = template.Id,
            Name = template.Name,
            IsBuiltIn = template.IsBuiltIn
        };

        // Always show at least 8 place rows; add extras for existing places beyond 8
        var maxPlace = Math.Max(8, template.Places.Count > 0 ? template.Places.Max(p => p.Place) : 8);
        vm.Places = Enumerable.Range(1, maxPlace).Select(i =>
        {
            var existing = template.Places.FirstOrDefault(p => p.Place == i);
            return new ScoringTemplatePlaceFormRow
            {
                Id = existing?.Id ?? 0,
                Place = i,
                Points = existing?.Points ?? 0
            };
        }).ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ScoringTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var template = await templateRepository.GetByIdAsync(model.Id);
        if (template == null) return NotFound();

        // Update name (built-in templates can't be renamed)
        if (!template.IsBuiltIn)
        {
            template.Name = model.Name;
            await templateRepository.UpdateAsync(template);
        }

        // Rebuild all places: delete all and re-insert non-zero rows
        await templateRepository.DeleteAllPlacesAsync(model.Id);
        foreach (var row in model.Places.Where(p => p.Points > 0))
        {
            await templateRepository.AddPlaceAsync(new ScoringTemplatePlace
            {
                ScoringTemplateId = model.Id,
                Place = row.Place,
                Points = row.Points
            });
        }

        TempData["SuccessMessage"] = "Scoring template updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await templateRepository.DeleteAsync(id);
        if (!deleted)
            TempData["ErrorMessage"] = "Cannot delete a built-in scoring template.";
        else
            TempData["SuccessMessage"] = "Scoring template deleted.";

        return RedirectToAction(nameof(Index));
    }
}
