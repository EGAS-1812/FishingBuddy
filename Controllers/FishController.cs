using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Details(int id)
        {
            var fish = _repository.GetFishById(id);
            if (fish == null) return NotFound();

            ViewBag.FavouriteBait = _repository.GetBaitById(fish.FavouriteBaitID);
            return View(fish);
        }
    }
}
