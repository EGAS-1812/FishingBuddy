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

        public IActionResult Details(int id)
        {
            var spot = _repository.GetFishingSpotById(id);
            if (spot == null) return NotFound();
            return View(spot);
        }
    }
}
