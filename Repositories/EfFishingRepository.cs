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
        var existing = dbContext.Techniques.FirstOrDefault(t => t.TechniqueID == technique.TechniqueID);
        if (existing == null) throw new InvalidOperationException($"Technique with ID {technique.TechniqueID} was not found.");
        existing.TechniqueName = technique.TechniqueName;
        existing.PerformanceNote = technique.PerformanceNote;
        existing.TutorialUrl = technique.TutorialUrl;
        dbContext.SaveChanges();
    }

    public void DeleteTechnique(int id)
    {
        var existing = dbContext.Techniques.FirstOrDefault(t => t.TechniqueID == id);
        if (existing == null) throw new InvalidOperationException($"Technique with ID {id} was not found.");
        dbContext.Techniques.Remove(existing);
        dbContext.SaveChanges();
    }

    public void AddBait(Bait bait) { dbContext.Baits.Add(bait); dbContext.SaveChanges(); }

    public void UpdateBait(Bait bait)
    {
        var existing = dbContext.Baits.FirstOrDefault(b => b.BaitID == bait.BaitID);
        if (existing == null) throw new InvalidOperationException($"Bait with ID {bait.BaitID} was not found.");
        existing.BaitName = bait.BaitName;
        existing.BaitType = bait.BaitType;
        existing.PreparationMethod = bait.PreparationMethod;
        existing.AveragePriceEur = bait.AveragePriceEur;
        dbContext.SaveChanges();
    }

    public void DeleteBait(int id)
    {
        var existing = dbContext.Baits.FirstOrDefault(b => b.BaitID == id);
        if (existing == null) throw new InvalidOperationException($"Bait with ID {id} was not found.");
        dbContext.Baits.Remove(existing);
        dbContext.SaveChanges();
    }

    public void AddFish(Fish fish) { dbContext.Fish.Add(fish); dbContext.SaveChanges(); }

    public void UpdateFish(Fish fish)
    {
        var existing = dbContext.Fish.FirstOrDefault(f => f.FishID == fish.FishID);
        if (existing == null) throw new InvalidOperationException($"Fish with ID {fish.FishID} was not found.");
        existing.SpeciesName = fish.SpeciesName;
        existing.CatchSeason = fish.CatchSeason;
        existing.FleshColor = fish.FleshColor;
        existing.FavouriteBaitID = fish.FavouriteBaitID;
        existing.PreferredMethodID = fish.PreferredMethodID;
        dbContext.SaveChanges();
    }

    public void DeleteFish(int id)
    {
        var existing = dbContext.Fish.FirstOrDefault(f => f.FishID == id);
        if (existing == null) throw new InvalidOperationException($"Fish with ID {id} was not found.");
        dbContext.Fish.Remove(existing);
        dbContext.SaveChanges();
    }

    public void AddUser(User user)
    {
        var license = user.FishingLicense;
        user.FishingLicense = null;

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        if (license != null)
        {
            license.UserID = user.UserID;
            dbContext.FishingLicenses.Add(license);
            dbContext.SaveChanges();
        }
    }

    public void UpdateUser(User user)
    {
        var existing = dbContext.Users
            .Include(u => u.FishingLicense)
            .FirstOrDefault(u => u.UserID == user.UserID);
        if (existing == null) throw new InvalidOperationException($"User with ID {user.UserID} was not found.");
        existing.Username = user.Username;
        existing.Email = user.Email;

        if (user.FishingLicense == null)
        {
            if (existing.FishingLicense != null)
            {
                dbContext.FishingLicenses.Remove(existing.FishingLicense);
                existing.FishingLicense = null;
            }
        }
        else
        {
            if (existing.FishingLicense == null)
            {
                existing.FishingLicense = new FishingLicense
                {
                    UserID = existing.UserID,
                    BeginDate = user.FishingLicense.BeginDate,
                    ExpirationDate = user.FishingLicense.ExpirationDate
                };
            }
            else
            {
                existing.FishingLicense.BeginDate = user.FishingLicense.BeginDate;
                existing.FishingLicense.ExpirationDate = user.FishingLicense.ExpirationDate;
            }
        }

        dbContext.SaveChanges();
    }

    public void DeleteUser(int id)
    {
        var existing = dbContext.Users
            .Include(u => u.FishingLicense)
            .FirstOrDefault(u => u.UserID == id);
        if (existing == null) throw new InvalidOperationException($"User with ID {id} was not found.");

        if (existing.FishingLicense != null)
        {
            dbContext.FishingLicenses.Remove(existing.FishingLicense);
        }

        dbContext.Users.Remove(existing);
        dbContext.SaveChanges();
    }

    public void AddFishingSpot(FishingSpot spot) { dbContext.FishingSpots.Add(spot); dbContext.SaveChanges(); }

    public void UpdateFishingSpot(FishingSpot spot)
    {
        var existing = dbContext.FishingSpots.FirstOrDefault(s => s.SpotID == spot.SpotID);
        if (existing == null) throw new InvalidOperationException($"FishingSpot with ID {spot.SpotID} was not found.");
        existing.SpotName = spot.SpotName;
        existing.Region = spot.Region;
        existing.HasPiers = spot.HasPiers;
        existing.BoatAccess = spot.BoatAccess;
        dbContext.SaveChanges();
    }

    public void DeleteFishingSpot(int id)
    {
        var existing = dbContext.FishingSpots.FirstOrDefault(s => s.SpotID == id);
        if (existing == null) throw new InvalidOperationException($"FishingSpot with ID {id} was not found.");
        dbContext.FishingSpots.Remove(existing);
        dbContext.SaveChanges();
    }

    public void AddCatchRecord(CatchRecord catchRecord) { dbContext.CatchRecords.Add(catchRecord); dbContext.SaveChanges(); }

    public void UpdateCatchRecord(CatchRecord catchRecord)
    {
        var existing = dbContext.CatchRecords.FirstOrDefault(c => c.CatchID == catchRecord.CatchID);
        if (existing == null) throw new InvalidOperationException($"CatchRecord with ID {catchRecord.CatchID} was not found.");
        existing.UserID = catchRecord.UserID;
        existing.FishID = catchRecord.FishID;
        existing.CatchDate = catchRecord.CatchDate;
        existing.Weight = catchRecord.Weight;
        existing.LengthCm = catchRecord.LengthCm;
        existing.Location = catchRecord.Location;
        dbContext.SaveChanges();
    }

    public void DeleteCatchRecord(int id)
    {
        var existing = dbContext.CatchRecords.FirstOrDefault(c => c.CatchID == id);
        if (existing == null) throw new InvalidOperationException($"CatchRecord with ID {id} was not found.");
        dbContext.CatchRecords.Remove(existing);
        dbContext.SaveChanges();
    }
}