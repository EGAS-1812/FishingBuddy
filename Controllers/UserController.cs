using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers
{
    public class UserController : Controller
    {
        private readonly IFishingRepository _repository;

        public UserController(IFishingRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.Users);
        }

        public IActionResult Details(int id)
        {
            var user = _repository.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
