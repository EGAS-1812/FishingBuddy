using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Controllers.Api;

[ApiController]
[Route("api/techniques")]
public class TechniqueApiController(FishingBuddyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TechniqueDto>>> GetAll([FromQuery] string? q)
    {
        var query = dbContext.Techniques.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(t =>
                t.TechniqueName.Contains(term) ||
                t.PerformanceNote.Contains(term) ||
                t.TutorialUrl.Contains(term));
        }

        var items = await query
            .OrderBy(t => t.TechniqueName)
            .Select(t => t.ToDto())
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TechniqueDto>> GetById(int id)
    {
        var technique = await dbContext.Techniques.FirstOrDefaultAsync(t => t.TechniqueID == id);
        if (technique == null)
        {
            return NotFound();
        }

        return Ok(technique.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TechniqueDto>> Create([FromBody] TechniqueUpsertDto model)
    {
        var technique = new Technique
        {
            TechniqueName = model.TechniqueName,
            PerformanceNote = model.PerformanceNote,
            TutorialUrl = model.TutorialUrl
        };

        dbContext.Techniques.Add(technique);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = technique.TechniqueID }, technique.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TechniqueDto>> Update(int id, [FromBody] TechniqueUpsertDto model)
    {
        var technique = await dbContext.Techniques.FirstOrDefaultAsync(t => t.TechniqueID == id);
        if (technique == null)
        {
            return NotFound();
        }

        technique.TechniqueName = model.TechniqueName;
        technique.PerformanceNote = model.PerformanceNote;
        technique.TutorialUrl = model.TutorialUrl;

        await dbContext.SaveChangesAsync();
        return Ok(technique.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var technique = await dbContext.Techniques.FirstOrDefaultAsync(t => t.TechniqueID == id);
        if (technique == null)
        {
            return NotFound();
        }

        dbContext.Techniques.Remove(technique);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Technique cannot be deleted while it is referenced by fish records.");
        }

        return NoContent();
    }
}
