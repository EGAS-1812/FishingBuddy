using FishingBuddy.Models;
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

        public IActionResult Manage()
        {
            return View(_repository.Users.OrderBy(u => u.Username).ToList());
        }

        [HttpGet]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(u =>
                    (u.Username != null && u.Username.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Email != null && u.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_UserRows", query.OrderBy(u => u.Username).ToList());
        }

        [HttpGet]
        public IActionResult Autocomplete(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var results = _repository.Users
                .Where(u => string.IsNullOrWhiteSpace(normalized) ||
                            u.Username.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                            u.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.Username)
                .Take(10)
                .Select(u => new
                {
                    id = u.UserID,
                    label = u.Username,
                    subtitle = u.Email
                })
                .ToList();

            return Json(results);
        }

        public IActionResult Details(int id)
        {
            var user = _repository.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid) return View(user);
            _repository.AddUser(user);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _repository.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User user)
        {
            if (id != user.UserID) return NotFound();
            if (!ModelState.IsValid) return View(user);
            _repository.UpdateUser(user);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _repository.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteUser(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
