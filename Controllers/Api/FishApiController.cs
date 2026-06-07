using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/fish")]
public class FishApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FishDto>>> GetAll([FromQuery] string? q, [FromQuery] Season? season, [FromQuery] FishFlesh? fleshColor)
    {
        var query = dbContext.Fish
            .Include(f => f.FavouriteBait)
            .Include(f => f.PreferredMethod)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(f =>
                f.SpeciesName.Contains(term) ||
                f.FavouriteBait!.BaitName.Contains(term) ||
                f.PreferredMethod.TechniqueName.Contains(term));
        }

        if (season.HasValue)
        {
            query = query.Where(f => f.CatchSeason == season.Value);
        }

        if (fleshColor.HasValue)
        {
            query = query.Where(f => f.FleshColor == fleshColor.Value);
        }

        var items = await query
            .OrderBy(f => f.SpeciesName)
            .ToListAsync();

        return Ok(items.Select(f => f.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FishDto>> GetById(int id)
    {
        var fish = await dbContext.Fish
            .Include(f => f.FavouriteBait)
            .Include(f => f.PreferredMethod)
            .FirstOrDefaultAsync(f => f.FishID == id);

        if (fish == null)
        {
            return NotFound();
        }

        return Ok(fish.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<FishDto>> Create([FromBody] FishUpsertDto model)
    {
        if (!await dbContext.Baits.AnyAsync(b => b.BaitID == model.FavouriteBaitID))
        {
            ModelState.AddModelError(nameof(FishUpsertDto.FavouriteBaitID), "Selected bait does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!await dbContext.Techniques.AnyAsync(t => t.TechniqueID == model.PreferredMethodID))
        {
            ModelState.AddModelError(nameof(FishUpsertDto.PreferredMethodID), "Selected technique does not exist.");
            return ValidationProblem(ModelState);
        }

        var fish = new Fish
        {
            SpeciesName = model.SpeciesName,
            CatchSeason = model.CatchSeason,
            FleshColor = model.FleshColor,
            FavouriteBaitID = model.FavouriteBaitID,
            PreferredMethodID = model.PreferredMethodID,
            Equipment = new Equipment
            {
                FReel = new FReel(model.Equipment.ReelSize, model.Equipment.ReelType),
                FRod = new FRod(model.Equipment.RodLengthMeters, model.Equipment.RodAction, model.Equipment.RodMinWeightGrams, model.Equipment.RodMaxWeightGrams),
                FLine = new FLine(model.Equipment.LineType, model.Equipment.LineThicknessMm)
            }
        };

        dbContext.Fish.Add(fish);
        await dbContext.SaveChangesAsync();

        await dbContext.Entry(fish).Reference(f => f.FavouriteBait).LoadAsync();
        await dbContext.Entry(fish).Reference(f => f.PreferredMethod).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = fish.FishID }, fish.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FishDto>> Update(int id, [FromBody] FishUpsertDto model)
    {
        var fish = await dbContext.Fish
            .Include(f => f.FavouriteBait)
            .Include(f => f.PreferredMethod)
            .FirstOrDefaultAsync(f => f.FishID == id);

        if (fish == null)
        {
            return NotFound();
        }

        if (!await dbContext.Baits.AnyAsync(b => b.BaitID == model.FavouriteBaitID))
        {
            ModelState.AddModelError(nameof(FishUpsertDto.FavouriteBaitID), "Selected bait does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!await dbContext.Techniques.AnyAsync(t => t.TechniqueID == model.PreferredMethodID))
        {
            ModelState.AddModelError(nameof(FishUpsertDto.PreferredMethodID), "Selected technique does not exist.");
            return ValidationProblem(ModelState);
        }

        fish.SpeciesName = model.SpeciesName;
        fish.CatchSeason = model.CatchSeason;
        fish.FleshColor = model.FleshColor;
        fish.FavouriteBaitID = model.FavouriteBaitID;
        fish.PreferredMethodID = model.PreferredMethodID;
        fish.Equipment = new Equipment
        {
            FReel = new FReel(model.Equipment.ReelSize, model.Equipment.ReelType),
            FRod = new FRod(model.Equipment.RodLengthMeters, model.Equipment.RodAction, model.Equipment.RodMinWeightGrams, model.Equipment.RodMaxWeightGrams),
            FLine = new FLine(model.Equipment.LineType, model.Equipment.LineThicknessMm)
        };

        await dbContext.SaveChangesAsync();

        await dbContext.Entry(fish).Reference(f => f.FavouriteBait).LoadAsync();
        await dbContext.Entry(fish).Reference(f => f.PreferredMethod).LoadAsync();

        return Ok(fish.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var fish = await dbContext.Fish.FirstOrDefaultAsync(f => f.FishID == id);
        if (fish == null)
        {
            return NotFound();
        }

        dbContext.Fish.Remove(fish);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Fish cannot be deleted while it is referenced by other records.");
        }

        return NoContent();
    }
}
