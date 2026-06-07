using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/fishing-licenses")]
public class FishingLicenseApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FishingLicenseDto>>> GetAll([FromQuery] bool? validToday)
    {
        var query = dbContext.FishingLicenses.AsQueryable();

        if (validToday == true)
        {
            var now = DateTime.UtcNow.Date;
            query = query.Where(l => l.BeginDate.Date <= now && l.ExpirationDate.Date >= now);
        }

        var items = await query
            .OrderBy(l => l.UserID)
            .Select(l => l.ToDto())
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<FishingLicenseDto>> GetByUserId(int userId)
    {
        var license = await dbContext.FishingLicenses.FirstOrDefaultAsync(l => l.UserID == userId);
        if (license == null)
        {
            return NotFound();
        }

        return Ok(license.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<FishingLicenseDto>> Create([FromBody] FishingLicenseUpsertDto model)
    {
        if (!await dbContext.Users.AnyAsync(u => u.UserID == model.UserID))
        {
            ModelState.AddModelError(nameof(FishingLicenseUpsertDto.UserID), "User does not exist.");
            return ValidationProblem(ModelState);
        }

        if (model.ExpirationDate < model.BeginDate)
        {
            ModelState.AddModelError(nameof(FishingLicenseUpsertDto.ExpirationDate), "Expiration date must be greater than or equal to begin date.");
            return ValidationProblem(ModelState);
        }

        if (await dbContext.FishingLicenses.AnyAsync(l => l.UserID == model.UserID))
        {
            return Conflict("License already exists for this user.");
        }

        var license = new FishingLicense
        {
            UserID = model.UserID,
            BeginDate = model.BeginDate,
            ExpirationDate = model.ExpirationDate
        };

        dbContext.FishingLicenses.Add(license);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByUserId), new { userId = license.UserID }, license.ToDto());
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<FishingLicenseDto>> Update(int userId, [FromBody] FishingLicenseUpsertDto model)
    {
        if (userId != model.UserID)
        {
            return BadRequest("Route userId does not match model userId.");
        }

        var license = await dbContext.FishingLicenses.FirstOrDefaultAsync(l => l.UserID == userId);
        if (license == null)
        {
            return NotFound();
        }

        if (model.ExpirationDate < model.BeginDate)
        {
            ModelState.AddModelError(nameof(FishingLicenseUpsertDto.ExpirationDate), "Expiration date must be greater than or equal to begin date.");
            return ValidationProblem(ModelState);
        }

        license.BeginDate = model.BeginDate;
        license.ExpirationDate = model.ExpirationDate;

        await dbContext.SaveChangesAsync();
        return Ok(license.ToDto());
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Delete(int userId)
    {
        var license = await dbContext.FishingLicenses.FirstOrDefaultAsync(l => l.UserID == userId);
        if (license == null)
        {
            return NotFound();
        }

        dbContext.FishingLicenses.Remove(license);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
