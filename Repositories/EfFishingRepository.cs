using FishingBuddy.Data;
using FishingBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Repositories;

public class EfFishingRepository(FishingBuddyDbContext dbContext) : IFishingRepository
{
    public IReadOnlyList<Technique> Techniques => dbContext.Techniques
        .AsNoTracking()
        .Include(t => t.FishUsingTechnique)
        .ToList();

    public IReadOnlyList<Bait> Baits => dbContext.Baits.AsNoTracking().ToList();

    public IReadOnlyList<Fish> Fish => dbContext.Fish
        .AsNoTracking()
        .Include(f => f.PreferredMethod)
        .Include(f => f.FavouriteBait)
        .Include(f => f.PossibleSpots)
        .ToList();

    public IReadOnlyList<CatchRecord> CatchRecords => dbContext.CatchRecords
        .AsNoTracking()
        .Include(c => c.Fish)
        .Include(c => c.User)
        .ToList();

    public IReadOnlyList<FishingLicense> FishingLicenses => dbContext.FishingLicenses.AsNoTracking().ToList();

    public IReadOnlyList<User> Users => dbContext.Users
        .AsNoTracking()
        .Include(u => u.FishingLicense)
        .Include(u => u.FavoriteFish)
        .Include(u => u.CatchRecords)
        .ToList();

    public IReadOnlyList<FishingSpot> FishingSpots => dbContext.FishingSpots
        .AsNoTracking()
        .Include(s => s.MostLikelyCatch)
        .ToList();

    public Technique? GetTechniqueById(int id) => dbContext.Techniques.AsNoTracking().FirstOrDefault(t => t.TechniqueID == id);

    public Bait? GetBaitById(int id) => dbContext.Baits.AsNoTracking().FirstOrDefault(b => b.BaitID == id);

    public Fish? GetFishById(int id) => dbContext.Fish
        .AsNoTracking()
        .Include(f => f.PreferredMethod)
        .Include(f => f.FavouriteBait)
        .Include(f => f.PossibleSpots)
        .FirstOrDefault(f => f.FishID == id);

    public CatchRecord? GetCatchRecordById(int id) => dbContext.CatchRecords
        .AsNoTracking()
        .Include(c => c.Fish)
        .Include(c => c.User)
        .FirstOrDefault(c => c.CatchID == id);

    public FishingLicense? GetFishingLicenseByUserId(int userId) => dbContext.FishingLicenses
        .AsNoTracking()
        .FirstOrDefault(l => l.UserID == userId);

    public User? GetUserById(int id) => dbContext.Users
        .AsNoTracking()
        .Include(u => u.FishingLicense)
        .Include(u => u.FavoriteFish)
        .Include(u => u.CatchRecords)
        .FirstOrDefault(u => u.UserID == id);

    public FishingSpot? GetFishingSpotById(int id) => dbContext.FishingSpots
        .AsNoTracking()
        .Include(s => s.MostLikelyCatch)
        .FirstOrDefault(s => s.SpotID == id);

    public void AddTechnique(Technique technique)
    {
        dbContext.Techniques.Add(technique);
        dbContext.SaveChanges();
    }

    public void UpdateTechnique(Technique technique)
    {
        var existingTechnique = dbContext.Techniques.FirstOrDefault(t => t.TechniqueID == technique.TechniqueID);
        if (existingTechnique == null)
        {
            throw new InvalidOperationException($"Technique with ID {technique.TechniqueID} was not found.");
        }

        existingTechnique.TechniqueName = technique.TechniqueName;
        existingTechnique.PerformanceNote = technique.PerformanceNote;
        existingTechnique.TutorialUrl = technique.TutorialUrl;

        dbContext.SaveChanges();
    }
}