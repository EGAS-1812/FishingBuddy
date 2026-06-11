using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.Fish);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manage()
        {
            return View(_repository.Fish.OrderBy(f => f.SpeciesName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.Fish.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(f =>
                    (f.SpeciesName != null && f.SpeciesName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (f.CatchSeason.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (f.FleshColor.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (f.FavouriteBait != null && f.FavouriteBait.BaitName != null && f.FavouriteBait.BaitName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (f.PreferredMethod != null && f.PreferredMethod.TechniqueName != null && f.PreferredMethod.TechniqueName.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_FishRows", query.OrderBy(f => f.SpeciesName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Autocomplete(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var results = _repository.Fish
                .Where(f => string.IsNullOrWhiteSpace(normalized) || f.SpeciesName.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.SpeciesName)
                .Take(10)
                .Select(f => new
                {
                    id = f.FishID,
                    label = f.SpeciesName,
                    subtitle = f.CatchSeason.ToString()
                })
                .ToList();

            return Json(results);
        }

        [AllowAnonymous]
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
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName");
            ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName");
            ViewData["PreferredMethodLabel"] = string.Empty;
            return View(new Fish());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(Fish fish)
        {
            TryResolvePreferredMethodFromDisplayName(fish);
            ValidateRelatedEntities(fish);

            if (!ModelState.IsValid)
            {
                ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName");
                ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName");
                ViewData["PreferredMethodLabel"] = Request.Form[$"{nameof(Fish.PreferredMethodID)}Display"].ToString();
                return View(fish);
            }
            _repository.AddFish(fish);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();
            ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName", fish.FavouriteBaitID);
            ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName", fish.PreferredMethodID);
            ViewData["PreferredMethodLabel"] = fish.PreferredMethod?.TechniqueName ?? string.Empty;
            return View(fish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id, Fish fish)
        {
            if (id != fish.FishID) return NotFound();

            TryResolvePreferredMethodFromDisplayName(fish);
            ValidateRelatedEntities(fish);

            if (!ModelState.IsValid)
            {
                ViewBag.Baits = new SelectList(_repository.Baits, "BaitID", "BaitName", fish.FavouriteBaitID);
                ViewBag.Techniques = new SelectList(_repository.Techniques, "TechniqueID", "TechniqueName", fish.PreferredMethodID);
                ViewData["PreferredMethodLabel"] = Request.Form[$"{nameof(Fish.PreferredMethodID)}Display"].ToString();
                return View(fish);
            }
            _repository.UpdateFish(fish);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();
            return View(fish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteFish(id);
            return RedirectToAction(nameof(Manage));
        }

        private void ValidateRelatedEntities(Fish fish)
        {
            if (_repository.GetBaitById(fish.FavouriteBaitID) == null)
            {
                ModelState.AddModelError(nameof(Fish.FavouriteBaitID), "Selected bait is not valid.");
            }

            if (_repository.GetTechniqueById(fish.PreferredMethodID) == null)
            {
                ModelState.AddModelError(nameof(Fish.PreferredMethodID), "Selected technique is not valid.");
            }
        }

        private void TryResolvePreferredMethodFromDisplayName(Fish fish)
        {
            if (fish.PreferredMethodID > 0)
            {
                return;
            }

            var displayName = Request.Form[$"{nameof(Fish.PreferredMethodID)}Display"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            var technique = _repository.Techniques.FirstOrDefault(t =>
                string.Equals(t.TechniqueName, displayName, StringComparison.OrdinalIgnoreCase));

            if (technique != null)
            {
                fish.PreferredMethodID = technique.TechniqueID;
            }
        }
    }
}
