using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Data;

public static class DbInitializer
{
    public static void Seed(FishingBuddyDbContext dbContext)
    {
        var mockRepository = new MockFishingRepository();

        SyncTechniques(dbContext, mockRepository.Techniques);
        SyncBaits(dbContext, mockRepository.Baits);
        SyncFish(dbContext, mockRepository.Fish);
        SyncUsers(dbContext, mockRepository.Users);
        SyncFishingLicenses(dbContext, mockRepository.FishingLicenses);
        SyncCatchRecords(dbContext, mockRepository.CatchRecords);
        SyncFishingSpots(dbContext, mockRepository.FishingSpots);

        dbContext.SaveChanges();

        SyncUserFavoriteFish(dbContext, mockRepository.Users);
        SyncFishingSpotFish(dbContext, mockRepository.FishingSpots);

        dbContext.SaveChanges();
    }

    private static void SyncTechniques(FishingBuddyDbContext dbContext, IEnumerable<Technique> sourceTechniques)
    {
        foreach (var source in sourceTechniques)
        {
            var target = dbContext.Techniques.FirstOrDefault(t => t.TechniqueID == source.TechniqueID);
            if (target == null)
            {
                dbContext.Techniques.Add(new Technique
                {
                    TechniqueID = source.TechniqueID,
                    TechniqueName = source.TechniqueName,
                    PerformanceNote = source.PerformanceNote,
                    TutorialUrl = source.TutorialUrl
                });

                continue;
            }

            target.TechniqueName = source.TechniqueName;
            target.PerformanceNote = source.PerformanceNote;
            target.TutorialUrl = source.TutorialUrl;
        }
    }

    private static void SyncBaits(FishingBuddyDbContext dbContext, IEnumerable<Bait> sourceBaits)
    {
        foreach (var source in sourceBaits)
        {
            var target = dbContext.Baits.FirstOrDefault(b => b.BaitID == source.BaitID);
            if (target == null)
            {
                dbContext.Baits.Add(new Bait
                {
                    BaitID = source.BaitID,
                    BaitName = source.BaitName,
                    BaitType = source.BaitType,
                    PreparationMethod = source.PreparationMethod,
                    AveragePriceEur = source.AveragePriceEur
                });

                continue;
            }

            target.BaitName = source.BaitName;
            target.BaitType = source.BaitType;
            target.PreparationMethod = source.PreparationMethod;
            target.AveragePriceEur = source.AveragePriceEur;
        }
    }

    private static void SyncFish(FishingBuddyDbContext dbContext, IEnumerable<Fish> sourceFish)
    {
        foreach (var source in sourceFish)
        {
            var target = dbContext.Fish.FirstOrDefault(f => f.FishID == source.FishID);
            if (target == null)
            {
                dbContext.Fish.Add(new Fish
                {
                    FishID = source.FishID,
                    SpeciesName = source.SpeciesName,
                    CatchSeason = source.CatchSeason,
                    FavouriteBaitID = source.FavouriteBaitID,
                    FleshColor = source.FleshColor,
                    PreferredMethodID = source.PreferredMethodID,
                    Equipment = CloneEquipment(source.Equipment)
                });

                continue;
            }

            target.SpeciesName = source.SpeciesName;
            target.CatchSeason = source.CatchSeason;
            target.FavouriteBaitID = source.FavouriteBaitID;
            target.FleshColor = source.FleshColor;
            target.PreferredMethodID = source.PreferredMethodID;
            target.Equipment = CloneEquipment(source.Equipment);
        }
    }

    private static void SyncUsers(FishingBuddyDbContext dbContext, IEnumerable<User> sourceUsers)
    {
        foreach (var source in sourceUsers)
        {
            var target = dbContext.Users.FirstOrDefault(u => u.UserID == source.UserID);
            if (target == null)
            {
                dbContext.Users.Add(new User
                {
                    UserID = source.UserID,
                    Username = source.Username,
                    Email = source.Email
                });

                continue;
            }

            target.Username = source.Username;
            target.Email = source.Email;
        }
    }

    private static void SyncFishingLicenses(FishingBuddyDbContext dbContext, IEnumerable<FishingLicense> sourceLicenses)
    {
        foreach (var source in sourceLicenses)
        {
            var target = dbContext.FishingLicenses.FirstOrDefault(l => l.UserID == source.UserID);
            if (target == null)
            {
                dbContext.FishingLicenses.Add(new FishingLicense
                {
                    UserID = source.UserID,
                    BeginDate = source.BeginDate,
                    ExpirationDate = source.ExpirationDate
                });

                continue;
            }

            target.BeginDate = source.BeginDate;
            target.ExpirationDate = source.ExpirationDate;
        }
    }

    private static void SyncCatchRecords(FishingBuddyDbContext dbContext, IEnumerable<CatchRecord> sourceCatchRecords)
    {
        foreach (var source in sourceCatchRecords)
        {
            var target = dbContext.CatchRecords.FirstOrDefault(c => c.CatchID == source.CatchID);
            if (target == null)
            {
                dbContext.CatchRecords.Add(new CatchRecord
                {
                    CatchID = source.CatchID,
                    UserID = source.UserID,
                    FishID = source.FishID,
                    CatchDate = source.CatchDate,
                    Weight = source.Weight,
                    LengthCm = source.LengthCm,
                    Location = source.Location
                });

                continue;
            }

            target.UserID = source.UserID;
            target.FishID = source.FishID;
            target.CatchDate = source.CatchDate;
            target.Weight = source.Weight;
            target.LengthCm = source.LengthCm;
            target.Location = source.Location;
        }
    }

    private static void SyncFishingSpots(FishingBuddyDbContext dbContext, IEnumerable<FishingSpot> sourceFishingSpots)
    {
        foreach (var source in sourceFishingSpots)
        {
            var target = dbContext.FishingSpots.FirstOrDefault(s => s.SpotID == source.SpotID);
            if (target == null)
            {
                dbContext.FishingSpots.Add(new FishingSpot
                {
                    SpotID = source.SpotID,
                    SpotName = source.SpotName,
                    Region = source.Region,
                    HasPiers = source.HasPiers,
                    BoatAccess = source.BoatAccess
                });

                continue;
            }

            target.SpotName = source.SpotName;
            target.Region = source.Region;
            target.HasPiers = source.HasPiers;
            target.BoatAccess = source.BoatAccess;
        }
    }

    private static void SyncUserFavoriteFish(FishingBuddyDbContext dbContext, IEnumerable<User> sourceUsers)
    {
        foreach (var source in sourceUsers)
        {
            var target = dbContext.Users
                .Include(u => u.FavoriteFish)
                .First(u => u.UserID == source.UserID);

            target.FavoriteFish.Clear();

            foreach (var favoriteFish in source.FavoriteFish)
            {
                var fish = dbContext.Fish.First(f => f.FishID == favoriteFish.FishID);
                target.FavoriteFish.Add(fish);
            }
        }
    }

    private static void SyncFishingSpotFish(FishingBuddyDbContext dbContext, IEnumerable<FishingSpot> sourceFishingSpots)
    {
        foreach (var source in sourceFishingSpots)
        {
            var target = dbContext.FishingSpots
                .Include(s => s.MostLikelyCatch)
                .First(s => s.SpotID == source.SpotID);

            target.MostLikelyCatch.Clear();

            foreach (var fishAtSpot in source.MostLikelyCatch)
            {
                var fish = dbContext.Fish.First(f => f.FishID == fishAtSpot.FishID);
                target.MostLikelyCatch.Add(fish);
            }
        }
    }

    private static Equipment CloneEquipment(Equipment source)
    {
        return new Equipment(
            new FReel(source.FReel.Size, source.FReel.Type),
            new FRod(source.FRod.LengthMeters, source.FRod.Action, source.FRod.MinWeightRatingGrams, source.FRod.MaxWeightRatingGrams),
            new FLine(source.FLine.Type, source.FLine.ThicknessMm));
    }
}