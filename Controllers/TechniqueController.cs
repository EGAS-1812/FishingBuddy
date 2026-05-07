using FishingBuddy.Models;
using FishingBuddy.Repositories;
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

        public IActionResult Index()
        {
            return View(_repository.Techniques);
        }

        public IActionResult Manage()
        {
            var techniques = _repository.Techniques
                .OrderBy(t => t.TechniqueName)
                .ToList();

            return View(techniques);
        }

        public IActionResult Details(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();
            return View(technique);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Technique());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Technique technique)
        {
            if (!ModelState.IsValid)
            {
                return View(technique);
            }

            _repository.AddTechnique(technique);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();

            return View(technique);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Technique technique)
        {
            if (id != technique.TechniqueID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(technique);
            }

            _repository.UpdateTechnique(technique);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();
            return View(technique);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteTechnique(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
