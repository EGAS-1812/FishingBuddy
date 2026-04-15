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

        public IActionResult Details(int id)
        {
            var technique = _repository.GetTechniqueById(id);
            if (technique == null) return NotFound();
            return View(technique);
        }
    }
}
