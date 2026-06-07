using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers
{
    public class BaitController : Controller
    {
        private readonly IFishingRepository _repository;

        public BaitController(IFishingRepository repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.Baits);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manage()
        {
            return View(_repository.Baits.OrderBy(b => b.BaitName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.Baits.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(b =>
                    (b.BaitName != null && b.BaitName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (b.BaitType.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (b.PreparationMethod != null && b.PreparationMethod.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_BaitRows", query.OrderBy(b => b.BaitName).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Autocomplete(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var results = _repository.Baits
                .Where(b => string.IsNullOrWhiteSpace(normalized) || b.BaitName.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.BaitName)
                .Take(10)
                .Select(b => new
                {
                    id = b.BaitID,
                    label = b.BaitName,
                    subtitle = b.BaitType.ToString()
                })
                .ToList();

            return Json(results);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var bait = _repository.GetBaitById(id);
            if (bait == null) return NotFound();
            return View(bait);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new Bait());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(Bait bait)
        {
            if (!ModelState.IsValid) return View(bait);
            _repository.AddBait(bait);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var bait = _repository.GetBaitById(id);
            if (bait == null) return NotFound();
            return View(bait);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id, Bait bait)
        {
            if (id != bait.BaitID) return NotFound();
            if (!ModelState.IsValid) return View(bait);
            _repository.UpdateBait(bait);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var bait = _repository.GetBaitById(id);
            if (bait == null) return NotFound();
            return View(bait);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteBait(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
