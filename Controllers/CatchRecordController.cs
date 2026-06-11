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
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

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
            ViewBag.FeaturedAttachment = _dbContext.Attachments
                .Where(a => a.CatchRecordID == record.CatchID && a.ContentType.StartsWith("image/"))
                .OrderByDescending(a => a.CreatedAtUtc)
                .FirstOrDefault();
            return View(record);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
            ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
            ViewData["SelectedUserLabel"] = string.Empty;
            ViewData["SelectedFishLabel"] = string.Empty;
            return View(new CatchRecord { CatchDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CatchRecord catchRecord, IFormFile? catchImage)
        {
            TryResolveCatchRecordForeignKeysFromDisplayName(catchRecord);
            ValidateRelatedEntities(catchRecord);
            ValidateOptionalImage(catchImage, nameof(catchImage));

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
                ViewData["SelectedUserLabel"] = Request.Form[$"{nameof(CatchRecord.UserID)}Display"].ToString();
                ViewData["SelectedFishLabel"] = Request.Form[$"{nameof(CatchRecord.FishID)}Display"].ToString();
                return View(catchRecord);
            }

            _repository.AddCatchRecord(catchRecord);

            if (catchImage != null && catchImage.Length > 0)
            {
                await SaveAttachmentAsync(catchRecord.CatchID, catchImage);
            }

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
            ViewData["SelectedUserLabel"] = record.User?.Username ?? string.Empty;
            ViewData["SelectedFishLabel"] = record.Fish?.SpeciesName ?? string.Empty;
            ViewBag.FeaturedAttachment = _dbContext.Attachments
                .Where(a => a.CatchRecordID == record.CatchID && a.ContentType.StartsWith("image/"))
                .OrderByDescending(a => a.CreatedAtUtc)
                .FirstOrDefault();
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, CatchRecord catchRecord, IFormFile? catchImage)
        {
            if (id != catchRecord.CatchID) return NotFound();

            TryResolveCatchRecordForeignKeysFromDisplayName(catchRecord);
            ValidateRelatedEntities(catchRecord);
            ValidateOptionalImage(catchImage, nameof(catchImage));

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username", catchRecord.UserID);
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName", catchRecord.FishID);
                ViewData["SelectedUserLabel"] = Request.Form[$"{nameof(CatchRecord.UserID)}Display"].ToString();
                ViewData["SelectedFishLabel"] = Request.Form[$"{nameof(CatchRecord.FishID)}Display"].ToString();
                return View(catchRecord);
            }

            _repository.UpdateCatchRecord(catchRecord);

            if (catchImage != null && catchImage.Length > 0)
            {
                await SaveAttachmentAsync(catchRecord.CatchID, catchImage);
            }

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

        private void TryResolveCatchRecordForeignKeysFromDisplayName(CatchRecord catchRecord)
        {
            if (catchRecord.UserID <= 0)
            {
                var userLabel = Request.Form[$"{nameof(CatchRecord.UserID)}Display"].ToString().Trim();
                if (!string.IsNullOrWhiteSpace(userLabel))
                {
                    var user = _repository.Users.FirstOrDefault(u =>
                        string.Equals(u.Username, userLabel, StringComparison.OrdinalIgnoreCase));

                    if (user != null)
                    {
                        catchRecord.UserID = user.UserID;
                    }
                }
            }

            if (catchRecord.FishID <= 0)
            {
                var fishLabel = Request.Form[$"{nameof(CatchRecord.FishID)}Display"].ToString().Trim();
                if (!string.IsNullOrWhiteSpace(fishLabel))
                {
                    var fish = _repository.Fish.FirstOrDefault(f =>
                        string.Equals(f.SpeciesName, fishLabel, StringComparison.OrdinalIgnoreCase));

                    if (fish != null)
                    {
                        catchRecord.FishID = fish.FishID;
                    }
                }
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

            if (!IsAllowedImage(file))
            {
                return BadRequest("Only image files (jpg, jpeg, png, webp, gif) are allowed.");
            }

            await SaveAttachmentAsync(catchRecordId, file);

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

        private void ValidateOptionalImage(IFormFile? file, string modelKey)
        {
            if (file == null || file.Length == 0)
            {
                return;
            }

            if (!IsAllowedImage(file))
            {
                ModelState.AddModelError(modelKey, "Dozvoljene su samo slike (jpg, jpeg, png, webp, gif).");
            }
        }

        private static bool IsAllowedImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            return AllowedImageContentTypes.Contains(file.ContentType) && AllowedImageExtensions.Contains(extension);
        }

        private async Task SaveAttachmentAsync(int catchRecordId, IFormFile file)
        {
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
        }
    }
}
