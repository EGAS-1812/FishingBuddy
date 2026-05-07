using FishingBuddy.Models;
using FishingBuddy.Repositories;
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

        public IActionResult Index()
        {
            return View(_repository.FishingSpots);
        }

        public IActionResult Manage()
        {
            return View(_repository.FishingSpots.OrderBy(s => s.SpotName).ToList());
        }

        public IActionResult Details(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new FishingSpot());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FishingSpot spot)
        {
            if (!ModelState.IsValid) return View(spot);
            _repository.AddFishingSpot(spot);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FishingSpot spot)
        {
            if (id != spot.SpotID) return NotFound();
            if (!ModelState.IsValid) return View(spot);
            _repository.UpdateFishingSpot(spot);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteFishingSpot(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
