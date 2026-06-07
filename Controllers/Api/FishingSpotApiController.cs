using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/fishing-spots")]
public class FishingSpotApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FishingSpotDto>>> GetAll([FromQuery] string? q, [FromQuery] string? region)
    {
        var query = dbContext.FishingSpots
            .Include(s => s.MostLikelyCatch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(s =>
                s.SpotName.Contains(term) ||
                s.Region.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            var regionTerm = region.Trim();
            query = query.Where(s => s.Region.Contains(regionTerm));
        }

        var items = await query
            .OrderBy(s => s.SpotName)
            .ToListAsync();

        return Ok(items.Select(s => s.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FishingSpotDto>> GetById(int id)
    {
        var spot = await dbContext.FishingSpots
            .Include(s => s.MostLikelyCatch)
            .FirstOrDefaultAsync(s => s.SpotID == id);

        if (spot == null)
        {
            return NotFound();
        }

        return Ok(spot.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<FishingSpotDto>> Create([FromBody] FishingSpotUpsertDto model)
    {
        var spot = new FishingSpot
        {
            SpotName = model.SpotName,
            Region = model.Region,
            HasPiers = model.HasPiers,
            BoatAccess = model.BoatAccess
        };

        dbContext.FishingSpots.Add(spot);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = spot.SpotID }, spot.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FishingSpotDto>> Update(int id, [FromBody] FishingSpotUpsertDto model)
    {
        var spot = await dbContext.FishingSpots
            .Include(s => s.MostLikelyCatch)
            .FirstOrDefaultAsync(s => s.SpotID == id);

        if (spot == null)
        {
            return NotFound();
        }

        spot.SpotName = model.SpotName;
        spot.Region = model.Region;
        spot.HasPiers = model.HasPiers;
        spot.BoatAccess = model.BoatAccess;

        await dbContext.SaveChangesAsync();
        return Ok(spot.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var spot = await dbContext.FishingSpots.FirstOrDefaultAsync(s => s.SpotID == id);
        if (spot == null)
        {
            return NotFound();
        }

        dbContext.FishingSpots.Remove(spot);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
