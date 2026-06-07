using FishingBuddy.Data;
using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers
{
    public class CatchRecordController : Controller
    {
        private readonly IFishingRepository _repository;
        private readonly FishingBuddyDbContext _dbContext;

        public CatchRecordController(IFishingRepository repository, FishingBuddyDbContext dbContext)
        {
            _repository = repository;
            _dbContext = dbContext;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_repository.CatchRecords);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manage()
        {
            return View(_repository.CatchRecords.OrderByDescending(c => c.CatchDate).ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var normalized = term?.Trim() ?? string.Empty;
            var query = _repository.CatchRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(c =>
                    (c.Location != null && c.Location.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    c.CatchDate.ToString("g").Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                    c.Weight.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                    c.LengthCm.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                    (c.Fish != null && c.Fish.SpeciesName != null && c.Fish.SpeciesName.Contains(normalized, StringComparison.OrdinalIgnoreCase)) ||
                    (c.User != null && c.User.Username != null && c.User.Username.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                );
            }

            return PartialView("_CatchRecordRows", query.OrderByDescending(c => c.CatchDate).ToList());
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();

            ViewBag.Fish = _repository.GetFishById(record.FishID);
            ViewBag.User = _repository.GetUserById(record.UserID);
            return View(record);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
            ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
            return View(new CatchRecord { CatchDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(CatchRecord catchRecord)
        {
            ValidateRelatedEntities(catchRecord);

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
                return View(catchRecord);
            }
            _repository.AddCatchRecord(catchRecord);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();
            ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username", record.UserID);
            ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName", record.FishID);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id, CatchRecord catchRecord)
        {
            if (id != catchRecord.CatchID) return NotFound();

            ValidateRelatedEntities(catchRecord);

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username", catchRecord.UserID);
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName", catchRecord.FishID);
                return View(catchRecord);
            }
            _repository.UpdateCatchRecord(catchRecord);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteCatchRecord(id);
            return RedirectToAction(nameof(Manage));
        }

        private void ValidateRelatedEntities(CatchRecord catchRecord)
        {
            if (_repository.GetUserById(catchRecord.UserID) == null)
            {
                ModelState.AddModelError(nameof(CatchRecord.UserID), "Selected user is not valid.");
            }

            if (_repository.GetFishById(catchRecord.FishID) == null)
            {
                ModelState.AddModelError(nameof(CatchRecord.FishID), "Selected fish is not valid.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadAttachment(int catchRecordId, IFormFile? file)
        {
            var catchRecord = await _dbContext.CatchRecords.FirstOrDefaultAsync(c => c.CatchID == catchRecordId);
            if (catchRecord == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "catch-records",
                catchRecordId.ToString());

            Directory.CreateDirectory(uploadsPath);

            var generatedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var physicalPath = Path.Combine(uploadsPath, generatedName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                CatchRecordID = catchRecordId,
                FileName = file.FileName,
                FilePath = $"/uploads/catch-records/{catchRecordId}/{generatedName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Attachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAttachments(int catchRecordId)
        {
            var attachments = await _dbContext.Attachments
                .Where(a => a.CatchRecordID == catchRecordId)
                .OrderByDescending(a => a.CreatedAtUtc)
                .ToListAsync();

            return PartialView("_AttachmentList", attachments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _dbContext.Attachments.FirstOrDefaultAsync(a => a.AttachmentID == id);
            if (attachment == null)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _dbContext.Attachments.Remove(attachment);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
