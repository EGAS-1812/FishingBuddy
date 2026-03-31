using System;
using System.Collections.Generic;
using System.Linq;
using FishingBuddy.Models;

internal class FBuddy
{
    private static void Main()
    {
        var techniques = new List<Technique>
        {
            new Technique(1, "Spinning"),
            new Technique(2, "Casting"),
            new Technique(3, "Jigging")
        };

        var baits = new List<Bait>
        {
            new Bait(1, "Crv", BaitType.Live, "Nakačen na udicu"),
            new Bait(2, "Srdela", BaitType.Live, "Zasoljena i omotana koncem"),
            new Bait(3, "Varalica", BaitType.Artificial, "Montirana na kopču")
        };

        var fish = new List<Fish>
        {
            new Fish(1, "Brancin", Season.Winter, 1, FishFlesh.White, techniques[0]),
            new Fish(2, "Orada", Season.Summer, 2, FishFlesh.White, techniques[1]),
            new Fish(3, "Šaran", Season.Spring, 1, FishFlesh.White, techniques[2])
        };

        var catchRecords = new List<CatchRecord>
        {
            new CatchRecord(1, 1, 1, new DateTime(2023, 6, 1), 2.5, "Rijeka"),
            new CatchRecord(2, 2, 2, new DateTime(2024, 1, 15), 1.2, "Kraljevica"),
            new CatchRecord(3, 3, 3, new DateTime(2024, 3, 20), 4.0, "Bakarac")
        };

        var users = new List<User>
        {
            new User { UserID = 1, Username = "Ribar1", Email = "r1@example.com", CatchRecords = new List<CatchRecord>{ catchRecords[0] }, FavoriteFish = new List<Fish>{ fish[0] } },
            new User { UserID = 2, Username = "Ribar2", Email = "r2@example.com", CatchRecords = new List<CatchRecord>{ catchRecords[1] }, FavoriteFish = new List<Fish>{ fish[1] } },
            new User { UserID = 3, Username = "Ribar3", Email = "r3@example.com", CatchRecords = new List<CatchRecord>{ catchRecords[2] }, FavoriteFish = new List<Fish>{ fish[2] } }
        };

        var spots = new List<FishingSpot>
        {
            new FishingSpot(1, "Rijeka", "Primorsko-goranska", hasPiers: true, boatAccess: true, mostLikelyCatch: fish),
            new FishingSpot(2, "Kraljevica", "Primorsko-goranska", hasPiers: false, boatAccess: true, mostLikelyCatch: new List<Fish>{ fish[0], fish[2] }),
            new FishingSpot(3, "Bakarac", "Primorsko-goranska", hasPiers: true, boatAccess: false, mostLikelyCatch: new List<Fish>{ fish[1] })
        };

        /* Ispis zakomentiran za potrebe labosa:
        Console.WriteLine("Techniques:");
        foreach (var t in techniques)
            Console.WriteLine($"  {t.TechniqueID}: {t.TechniqueName}");

        Console.WriteLine("\nBaits:");
        foreach (var b in baits)
            Console.WriteLine($"  {b.BaitID}: {b.BaitName} ({b.BaitType}) - {b.PreparationMethod}");

        Console.WriteLine("\nFishes:");
        foreach (var f in fishes)
            Console.WriteLine($"  {f.FishID}: {f.SpeciesName}, Season={f.CatchSeason}, Flesh={f.FleshColor}, Preferred={f.PreferredMethod?.TechniqueName}");

        Console.WriteLine("\nCatch records:");
        foreach (var c in catchRecords)
            Console.WriteLine($"  {c.CatchID}: User={c.UserID}, Fish={c.FishID}, Date={c.CatchDate:d}, Weight={c.Weight}kg, Loc={c.Location}");

        Console.WriteLine("\nUsers:");
        foreach (var u in users)
            Console.WriteLine($"  {u.UserID}: {u.Username}, Email={u.Email}, FavoriteFishCount={u.FavoriteFish?.Count}, CatchCount={u.CatchRecords?.Count}");

        Console.WriteLine("\nFishing spots:");
        foreach (var s in spots)
            Console.WriteLine($"  {s.SpotID}: {s.SpotName} ({s.Region}) Piers={s.HasPiers}, Boat={s.BoatAccess}, LikelyCatch={s.MostLikelyCatch?.Count}");

        Console.WriteLine("\nEXIT WITH ANY KEY");
        Console.ReadKey();
        */

        // Primjer LINQ upita nad listom 'fishes':
        // Nalazimo sve ribe koje se love ljeti, sortiramo po imenu i pretvaramo u listu.
        var summerFishes = fish
            .Where(f => f.CatchSeason == Season.Summer)
            .OrderBy(f => f.SpeciesName)
            .ToList();

        var usersWithHeavyCatch =
            (from user in users
             from catchRecord in user.CatchRecords
             where catchRecord.Weight > 2.0
             select user)
            .DistinctBy(u => u.UserID)
            .ToList();

        //summerFishes.ForEach(f => Console.WriteLine($"{f.FishID}: {f.SpeciesName} ({f.CatchSeason})"));
        usersWithHeavyCatch.ForEach(u => Console.WriteLine($"{u.Username} caught fish heavier than 2 kg."));
    }
}