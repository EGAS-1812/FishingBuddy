using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/catch-records")]
public class CatchRecordApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatchRecordDto>>> GetAll([FromQuery] string? q, [FromQuery] int? userId, [FromQuery] int? fishId)
    {
        var query = dbContext.CatchRecords
            .Include(c => c.User)
            .Include(c => c.Fish)
            .Include(c => c.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(c =>
                c.Location.Contains(term) ||
                c.User.Username.Contains(term) ||
                c.Fish.SpeciesName.Contains(term));
        }

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserID == userId.Value);
        }

        if (fishId.HasValue)
        {
            query = query.Where(c => c.FishID == fishId.Value);
        }

        var records = await query
            .OrderByDescending(c => c.CatchDate)
            .ToListAsync();

        return Ok(records.Select(c => c.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatchRecordDto>> GetById(int id)
    {
        var record = await dbContext.CatchRecords
            .Include(c => c.User)
            .Include(c => c.Fish)
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.CatchID == id);

        if (record == null)
        {
            return NotFound();
        }

        return Ok(record.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CatchRecordDto>> Create([FromBody] CatchRecordUpsertDto model)
    {
        if (!await dbContext.Users.AnyAsync(u => u.UserID == model.UserID))
        {
            ModelState.AddModelError(nameof(CatchRecordUpsertDto.UserID), "Selected user does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!await dbContext.Fish.AnyAsync(f => f.FishID == model.FishID))
        {
            ModelState.AddModelError(nameof(CatchRecordUpsertDto.FishID), "Selected fish does not exist.");
            return ValidationProblem(ModelState);
        }

        var record = new CatchRecord
        {
            UserID = model.UserID,
            FishID = model.FishID,
            CatchDate = model.CatchDate,
            Weight = model.Weight,
            LengthCm = model.LengthCm,
            Location = model.Location
        };

        dbContext.CatchRecords.Add(record);
        await dbContext.SaveChangesAsync();

        record = await dbContext.CatchRecords
            .Include(c => c.User)
            .Include(c => c.Fish)
            .Include(c => c.Attachments)
            .FirstAsync(c => c.CatchID == record.CatchID);

        return CreatedAtAction(nameof(GetById), new { id = record.CatchID }, record.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CatchRecordDto>> Update(int id, [FromBody] CatchRecordUpsertDto model)
    {
        var record = await dbContext.CatchRecords
            .Include(c => c.User)
            .Include(c => c.Fish)
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.CatchID == id);

        if (record == null)
        {
            return NotFound();
        }

        if (!await dbContext.Users.AnyAsync(u => u.UserID == model.UserID))
        {
            ModelState.AddModelError(nameof(CatchRecordUpsertDto.UserID), "Selected user does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!await dbContext.Fish.AnyAsync(f => f.FishID == model.FishID))
        {
            ModelState.AddModelError(nameof(CatchRecordUpsertDto.FishID), "Selected fish does not exist.");
            return ValidationProblem(ModelState);
        }

        record.UserID = model.UserID;
        record.FishID = model.FishID;
        record.CatchDate = model.CatchDate;
        record.Weight = model.Weight;
        record.LengthCm = model.LengthCm;
        record.Location = model.Location;

        await dbContext.SaveChangesAsync();

        await dbContext.Entry(record).Reference(c => c.User).LoadAsync();
        await dbContext.Entry(record).Reference(c => c.Fish).LoadAsync();
        await dbContext.Entry(record).Collection(c => c.Attachments).LoadAsync();

        return Ok(record.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await dbContext.CatchRecords.FirstOrDefaultAsync(c => c.CatchID == id);
        if (record == null)
        {
            return NotFound();
        }

        dbContext.CatchRecords.Remove(record);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
