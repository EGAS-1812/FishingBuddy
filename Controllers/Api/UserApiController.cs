using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UserApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] string? q)
    {
        var query = dbContext.Users
            .Include(u => u.FishingLicense)
            .Include(u => u.FavoriteFish)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                u.Username.Contains(term) ||
                u.Email.Contains(term));
        }

        var users = await query
            .OrderBy(u => u.Username)
            .ToListAsync();

        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await dbContext.Users
            .Include(u => u.FishingLicense)
            .Include(u => u.FavoriteFish)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserUpsertDto model)
    {
        if (model.LicenseBeginDate.HasValue != model.LicenseExpirationDate.HasValue)
        {
            ModelState.AddModelError(nameof(UserUpsertDto.LicenseBeginDate), "Both license dates are required when setting a fishing license.");
            return ValidationProblem(ModelState);
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email
        };

        if (model.LicenseBeginDate.HasValue)
        {
            user.FishingLicense = new FishingLicense
            {
                BeginDate = model.LicenseBeginDate.Value,
                ExpirationDate = model.LicenseExpirationDate!.Value
            };
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        if (user.FishingLicense != null)
        {
            user.FishingLicense.UserID = user.UserID;
            await dbContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = user.UserID }, user.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserUpsertDto model)
    {
        var user = await dbContext.Users
            .Include(u => u.FishingLicense)
            .Include(u => u.FavoriteFish)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null)
        {
            return NotFound();
        }

        if (model.LicenseBeginDate.HasValue != model.LicenseExpirationDate.HasValue)
        {
            ModelState.AddModelError(nameof(UserUpsertDto.LicenseBeginDate), "Both license dates are required when setting a fishing license.");
            return ValidationProblem(ModelState);
        }

        user.Username = model.Username;
        user.Email = model.Email;

        if (!model.LicenseBeginDate.HasValue)
        {
            if (user.FishingLicense != null)
            {
                dbContext.FishingLicenses.Remove(user.FishingLicense);
                user.FishingLicense = null;
            }
        }
        else if (user.FishingLicense == null)
        {
            user.FishingLicense = new FishingLicense
            {
                UserID = user.UserID,
                BeginDate = model.LicenseBeginDate.Value,
                ExpirationDate = model.LicenseExpirationDate!.Value
            };
        }
        else
        {
            user.FishingLicense.BeginDate = model.LicenseBeginDate.Value;
            user.FishingLicense.ExpirationDate = model.LicenseExpirationDate!.Value;
        }

        await dbContext.SaveChangesAsync();
        return Ok(user.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await dbContext.Users
            .Include(u => u.FishingLicense)
            .FirstOrDefaultAsync(u => u.UserID == id);

        if (user == null)
        {
            return NotFound();
        }

        if (user.FishingLicense != null)
        {
            dbContext.FishingLicenses.Remove(user.FishingLicense);
        }

        dbContext.Users.Remove(user);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("User cannot be deleted while related records exist.");
        }

        return NoContent();
    }
}
