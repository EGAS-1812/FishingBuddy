using FishingBuddy.Repositories;
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

        public IActionResult Index()
        {
            return View(_repository.Baits);
        }

        public IActionResult Details(int id)
        {
            var bait = _repository.GetBaitById(id);
            if (bait == null) return NotFound();
            return View(bait);
        }
    }
}
