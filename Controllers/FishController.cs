using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FishingBuddy.Controllers
{
    public class FishController : Controller
    {
        private readonly IFishingRepository _repository;

        public FishController(IFishingRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.Fish);
        }

        public IActionResult Manage()
        {
            return View(_repository.Fish.OrderBy(f => f.SpeciesName).ToList());
        }

        public IActionResult Details(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();

            ViewBag.FavouriteBait = _repository.GetBaitById(fish.FavouriteBaitID);
            ViewBag.CommonCatchSpots = _repository.FishingSpots
                .Where(spot => spot.MostLikelyCatch.Any(currentFish => currentFish.FishID == fish.FishID))
                .Take(3)
                .ToList();

            return View(fish);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName");
            ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName");
            return View(new Fish());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Fish fish)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName");
                ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName");
                return View(fish);
            }
            _repository.AddFish(fish);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();
            ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName", fish.FavouriteBaitID);
            ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName", fish.PreferredMethodID);
            return View(fish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Fish fish)
        {
            if (id != fish.FishID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName", fish.FavouriteBaitID);
                ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName", fish.PreferredMethodID);
                return View(fish);
            }
            _repository.UpdateFish(fish);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();
            return View(fish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteFish(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
