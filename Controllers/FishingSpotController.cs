using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers
{
    public class FishingSpotController : Controller
    {
        private readonly IFishingRepository _repository;

        public FishingSpotController(IFishingRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.FishingSpots);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manage()
        {
            return View(_repository.FishingSpots.OrderBy(s => s.SpotName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.FishingSpots.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(s =>
                    (s.SpotName != null && s.SpotName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (s.Region != null && s.Region.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_FishingSpotRows", query.OrderBy(s => s.SpotName).ToList());
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new FishingSpot());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(FishingSpot spot)
        {
            if (!ModelState.IsValid) return View(spot);
            _repository.AddFishingSpot(spot);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id, FishingSpot spot)
        {
            if (id != spot.SpotID) return NotFound();
            if (!ModelState.IsValid) return View(spot);
            _repository.UpdateFishingSpot(spot);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteFishingSpot(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
