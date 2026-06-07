using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/baits")]
public class BaitApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaitDto>>> GetAll([FromQuery] string? q, [FromQuery] BaitType? baitType)
    {
        var query = dbContext.Baits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(b =>
                b.BaitName.Contains(term) ||
                b.PreparationMethod.Contains(term));
        }

        if (baitType.HasValue)
        {
            query = query.Where(b => b.BaitType == baitType.Value);
        }

        var items = await query
            .OrderBy(b => b.BaitName)
            .Select(b => b.ToDto())
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaitDto>> GetById(int id)
    {
        var bait = await dbContext.Baits.FirstOrDefaultAsync(b => b.BaitID == id);
        if (bait == null)
        {
            return NotFound();
        }

        return Ok(bait.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<BaitDto>> Create([FromBody] BaitUpsertDto model)
    {
        var bait = new Bait
        {
            BaitName = model.BaitName,
            BaitType = model.BaitType,
            PreparationMethod = model.PreparationMethod,
            AveragePriceEur = model.AveragePriceEur
        };

        dbContext.Baits.Add(bait);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = bait.BaitID }, bait.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaitDto>> Update(int id, [FromBody] BaitUpsertDto model)
    {
        var bait = await dbContext.Baits.FirstOrDefaultAsync(b => b.BaitID == id);
        if (bait == null)
        {
            return NotFound();
        }

        bait.BaitName = model.BaitName;
        bait.BaitType = model.BaitType;
        bait.PreparationMethod = model.PreparationMethod;
        bait.AveragePriceEur = model.AveragePriceEur;

        await dbContext.SaveChangesAsync();
        return Ok(bait.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var bait = await dbContext.Baits.FirstOrDefaultAsync(b => b.BaitID == id);
        if (bait == null)
        {
            return NotFound();
        }

        dbContext.Baits.Remove(bait);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Bait cannot be deleted while it is referenced by fish records.");
        }

        return NoContent();
    }
}
