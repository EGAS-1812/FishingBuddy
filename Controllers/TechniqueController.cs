using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers
{
    public class TechniqueController : Controller
    {
        private readonly IFishingRepository _repository;

        public TechniqueController(IFishingRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.Techniques);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manage()
        {
            var techniques = _repository.Techniques
                .OrderBy(t => t.TechniqueName)
                .ToList();

            return View(techniques);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.Techniques.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(t =>
                    (t.TechniqueName != null && t.TechniqueName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (t.PerformanceNote != null && t.PerformanceNote.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (t.TutorialUrl != null && t.TutorialUrl.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_TechniqueRows", query.OrderBy(t => t.TechniqueName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Autocomplete(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var results = _repository.Techniques
                .Where(t => string.IsNullOrWhiteSpace(normalized) || t.TechniqueName.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.TechniqueName)
                .Take(10)
                .Select(t => new
                {
                    id = t.TechniqueID,
                    label = t.TechniqueName,
                    subtitle = t.PerformanceNote
                })
                .ToList();

            return Json(results);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();
            return View(technique);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new Technique());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(Technique technique)
        {
            NormalizeOptionalTutorialUrl(technique);

            if (!ModelState.IsValid)
            {
                return View(technique);
            }

            _repository.AddTechnique(technique);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();

            return View(technique);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id, Technique technique)
        {
            if (id != technique.TechniqueID)
            {
                return NotFound();
            }

            NormalizeOptionalTutorialUrl(technique);

            if (!ModelState.IsValid)
            {
                return View(technique);
            }

            _repository.UpdateTechnique(technique);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();
            return View(technique);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteTechnique(id);
            return RedirectToAction(nameof(Manage));
        }

        private void NormalizeOptionalTutorialUrl(Technique technique)
        {
            if (string.IsNullOrWhiteSpace(technique.TutorialUrl))
            {
                technique.TutorialUrl = string.Empty;
                ModelState.Remove(nameof(Technique.TutorialUrl));
            }
        }
    }
}
