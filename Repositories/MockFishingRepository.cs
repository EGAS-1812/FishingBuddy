using FishingBuddy.Models;

namespace FishingBuddy.Repositories;

public class MockFishingRepository : IFishingRepository
{
    public IReadOnlyList<Technique> Techniques { get; }
    public IReadOnlyList<Bait> Baits { get; }
    public IReadOnlyList<Fish> Fish { get; }
    public IReadOnlyList<CatchRecord> CatchRecords { get; }
    public IReadOnlyList<FishingLicense> FishingLicenses { get; }
    public IReadOnlyList<User> Users { get; }
    public IReadOnlyList<FishingSpot> FishingSpots { get; }

    public MockFishingRepository()
    {
        var techniques = new List<Technique>
        {
            new Technique(1, "Spinning", "Varalica se vodi kroz srednji sloj mora u ritmičnim povlačenjima kako bi imitirala ranjenu ribicu.", "https://www.youtube.com/watch?v=7j4m8g0n0G8"),
            new Technique(2, "Casting", "Mamci ili varalice izbacuju se precizno prema rubovima kurenta, a zatim se kontrolirano privlače uz stalni kontakt s priborom.", "https://www.youtube.com/watch?v=8v4JrQ0wq7U"),
            new Technique(3, "Jigging", "Metalni jig spušta se u dublji sloj i podiže naglim trzajima štapa kako bi provocirao grabežljivicu na napad.", "https://www.youtube.com/watch?v=4lQz1sQy2vU"),
            new Technique(4, "Egging", "Specijalizirana skosavica vodi se u serijama trzaja i kraćih pauza kako bi lignja pratila i napala mamac.", "https://www.youtube.com/watch?v=3xU6g3Yx7vM"),
            new Technique(5, "Trolling", "Mamac se vuče iza plovila pri stabilnoj brzini, uz pažljivo praćenje dubine i putanje kroz zonu aktivnosti ribe.", "https://www.youtube.com/watch?v=KX2y6wVgQhI"),
            new Technique(6, "Bottom Fishing", "Sistem se polaže pri dnu, a zadržavanje napetosti na struni i strpljivo čekanje ključni su za pravodobnu kontru.", "https://www.youtube.com/watch?v=Rz7N2mFf2J8")
        };

        var baits = new List<Bait>
        {
            new Bait(1, "Crv", BaitType.Live, "Nakačen na udicu", 3.20m),
            new Bait(2, "Srdela", BaitType.Live, "Zasoljena i omotana koncem", 4.50m),
            new Bait(3, "Varalica", BaitType.Artificial, "Montirana na kopči", 7.90m),
            new Bait(4, "Lignja", BaitType.Live, "Omotana koncem", 8.40m),
            new Bait(5, "Kozica", BaitType.Live, "Provučen najlon kroz tijelo", 5.10m)
        };

        var equipment = new List<Equipment>
        {
            new Equipment(new FReel(3000, FReelType.Spinning), new FRod(2.1m, FRodAction.Medium, 10, 35), new FLine(FLineType.Braided, 0.16m)),
            new Equipment(new FReel(2000, FReelType.Spinning), new FRod(2.1m, FRodAction.MediumLight, 5, 20), new FLine(FLineType.Nylon, 0.20m)),
            new Equipment(new FReel(3000, FReelType.Baitcast), new FRod(2.4m, FRodAction.MediumHeavy, 15, 45), new FLine(FLineType.Braided, 0.18m)),
            new Equipment(new FReel(1000, FReelType.Spinning), new FRod(1.8m, FRodAction.Light, 3, 12), new FLine(FLineType.Nylon, 0.18m)),
            new Equipment(new FReel(3000, FReelType.Surf), new FRod(2.1m, FRodAction.Heavy, 30, 100), new FLine(FLineType.Braided, 0.22m)),
            new Equipment(new FReel(2000, FReelType.Spinning), new FRod(2.1m, FRodAction.Medium, 7, 28), new FLine(FLineType.Nylon, 0.22m)),
            new Equipment(new FReel(3000, FReelType.Troll), new FRod(2.4m, FRodAction.Heavy, 40, 100), new FLine(FLineType.Braided, 0.24m)),
            new Equipment(new FReel(2000, FReelType.Surf), new FRod(2.1m, FRodAction.MediumHeavy, 20, 60), new FLine(FLineType.Braided, 0.20m)),
            new Equipment(new FReel(3000, FReelType.Baitcast), new FRod(2.4m, FRodAction.Heavy, 35, 100), new FLine(FLineType.Braided, 0.26m)),
            new Equipment(new FReel(3000, FReelType.Troll), new FRod(2.4m, FRodAction.Heavy, 40, 100), new FLine(FLineType.Braided, 0.28m)),
            new Equipment(new FReel(2000, FReelType.Fly), new FRod(2.1m, FRodAction.Light, 5, 18), new FLine(FLineType.Nylon, 0.18m)),
            new Equipment(new FReel(2000, FReelType.Spinning), new FRod(2.1m, FRodAction.MediumLight, 8, 25), new FLine(FLineType.Nylon, 0.20m)),
            new Equipment(new FReel(2000, FReelType.Spinning), new FRod(2.1m, FRodAction.Medium, 10, 30), new FLine(FLineType.Braided, 0.16m)),
            new Equipment(new FReel(3000, FReelType.Troll), new FRod(2.4m, FRodAction.Heavy, 50, 100), new FLine(FLineType.Braided, 0.30m)),
            new Equipment(new FReel(1000, FReelType.Spinning), new FRod(1.8m, FRodAction.Light, 2, 10), new FLine(FLineType.Nylon, 0.16m))
        };

        var fish = new List<Fish>
        {
            new Fish(1, "Brancin", Season.Winter, 1, FishFlesh.White, techniques[0], equipment[0]),
            new Fish(2, "Orada", Season.Summer, 2, FishFlesh.White, techniques[1], equipment[1]),
            new Fish(3, "Strijelka", Season.Summer, 1, FishFlesh.White, techniques[2], equipment[2]),
            new Fish(4, "Srdela", Season.Winter, 3, FishFlesh.Blue, techniques[0], equipment[3]),
            new Fish(5, "Škarpina", Season.Summer, 2, FishFlesh.White, techniques[1], equipment[4]),
            new Fish(6, "Kovač", Season.Winter, 1, FishFlesh.White, techniques[2], equipment[5]),
            new Fish(7, "Gof", Season.Summer, 3, FishFlesh.White, techniques[0], equipment[6]),
            new Fish(8, "Kamenjarka", Season.Winter, 2, FishFlesh.White, techniques[1], equipment[7]),
            new Fish(9, "Mačka", Season.Summer, 1, FishFlesh.White, techniques[2], equipment[8]),
            new Fish(10, "Grdobina", Season.Summer, 3, FishFlesh.White, techniques[0], equipment[9]),
            new Fish(11, "Lignja", Season.Winter, 2, FishFlesh.White, techniques[2], equipment[10]),
            new Fish(12, "Oslić", Season.Winter, 2, FishFlesh.White, techniques[0], equipment[11]),
            new Fish(13, "Skuša", Season.Summer, 2, FishFlesh.Blue, techniques[0], equipment[12]),
            new Fish(14, "Tuna", Season.Summer, 3, FishFlesh.Red, techniques[0], equipment[13]),
            new Fish(15, "Inćun", Season.Summer, 1, FishFlesh.Blue, techniques[0], equipment[14])
        };

        var catchRecords = new List<CatchRecord>
        {
            new CatchRecord(1, 1, 1, new DateTime(2023, 6, 1), 2.5, 58, "Rijeka"),
            new CatchRecord(2, 2, 2, new DateTime(2024, 1, 15), 1.2, 37, "Kraljevica"),
            new CatchRecord(3, 3, 3, new DateTime(2024, 3, 20), 4.0, 76, "Bakarac"),
            new CatchRecord(4, 4, 1, new DateTime(2023, 7, 10), 3.5, 66, "Rovinj"),
            new CatchRecord(5, 5, 2, new DateTime(2024, 2, 5), 2.0, 44, "Zadar"),
            new CatchRecord(6, 6, 3, new DateTime(2023, 8, 15), 5.0, 88, "Split"),
            new CatchRecord(7, 1, 4, new DateTime(2023, 9, 1), 0.5, 24, "Dubrovnik"),
            new CatchRecord(8, 2, 5, new DateTime(2024, 4, 10), 1.8, 42, "Šibenik"),
            new CatchRecord(9, 3, 6, new DateTime(2024, 5, 20), 3.0, 61, "Pula")
        };

        var fishingLicenses = new List<FishingLicense>
        {
            new FishingLicense(1, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)),
            new FishingLicense(2, new DateTime(2025, 7, 1), new DateTime(2026, 6, 30)),
            new FishingLicense(3, new DateTime(2024, 5, 1), new DateTime(2025, 4, 30)),
            new FishingLicense(4, new DateTime(2023, 6, 1), new DateTime(2024, 5, 31)),
            new FishingLicense(5, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31)),
            new FishingLicense(6, new DateTime(2023, 1, 1), new DateTime(2023, 12, 31))
        };

        var users = new List<User>
        {
            new User
            {
                UserID = 1,
                Username = "Ivo Ivić",
                Email = "ivicivo@gmail.com",
                FishingLicense = fishingLicenses[0],
                CatchRecords = new List<CatchRecord> { catchRecords[0] },
                FavoriteFish = new List<Fish> { fish[0] }
            },
            new User
            {
                UserID = 2,
                Username = "Marko Markić",
                Email = "marko.markic@gmail.com",
                FishingLicense = fishingLicenses[1],
                CatchRecords = new List<CatchRecord> { catchRecords[1] },
                FavoriteFish = new List<Fish> { fish[1] }
            },
            new User
            {
                UserID = 3,
                Username = "Ana Anić",
                Email = "ana.anic@gmail.com",
                FishingLicense = fishingLicenses[2],
                CatchRecords = new List<CatchRecord> { catchRecords[2] },
                FavoriteFish = new List<Fish> { fish[2] }
            },
            new User
            {
                UserID = 4,
                Username = "Petar Petrić",
                Email = "petar.petric@gmail.com",
                FishingLicense = fishingLicenses[3],
                CatchRecords = new List<CatchRecord> { catchRecords[3] },
                FavoriteFish = new List<Fish> { fish[3] }
            },
            new User
            {
                UserID = 5,
                Username = "Lucija Lučić",
                Email = "lucija.lucic@gmail.com",
                FishingLicense = fishingLicenses[4],
                CatchRecords = new List<CatchRecord> { catchRecords[4] },
                FavoriteFish = new List<Fish> { fish[4] }
            },
            new User
            {
                UserID = 6,
                Username = "Ivan Ivanić",
                Email = "ivan.ivanic@gmail.com",
                FishingLicense = fishingLicenses[5],
                CatchRecords = new List<CatchRecord> { catchRecords[5] },
                FavoriteFish = new List<Fish> { fish[5] }
            }

        };

var fishingSpots = new List<FishingSpot>
{
    new FishingSpot(1, "Rijeka", "Primorsko-goranska", true, true,
        new List<Fish> { fish[0], fish[1], fish[3], fish[12], fish[14] }),

    new FishingSpot(2, "Kraljevica", "Primorsko-goranska", false, true,
        new List<Fish> { fish[0], fish[2], fish[6], fish[12], fish[13] }),

    new FishingSpot(3, "Bakarac", "Primorsko-goranska", true, false,
        new List<Fish> { fish[1], fish[3], fish[12], fish[14], fish[10] }),

    new FishingSpot(4, "Rovinj", "Istarska", false, false,
        new List<Fish> { fish[2], fish[6], fish[13], fish[14], fish[0] }),

    new FishingSpot(5, "Zadar", "Zadarska", true, true,
        new List<Fish> { fish[0], fish[1], fish[3], fish[11], fish[12] }),

    new FishingSpot(6, "Split", "Splitsko-dalmatinska", false, true,
        new List<Fish> { fish[3], fish[13], fish[14], fish[6], fish[2] }),

    new FishingSpot(7, "Dubrovnik", "Dubrovačko-neretvanska", true, false,
        new List<Fish> { fish[4], fish[6], fish[14], fish[0], fish[11] }),

    new FishingSpot(8, "Šibenik", "Šibensko-kninska", false, false,
        new List<Fish> { fish[5], fish[4], fish[11], fish[12], fish[1] }),

    new FishingSpot(9, "Pula", "Istarska", true, true,
        new List<Fish> { fish[0], fish[2], fish[13], fish[14], fish[3] }),

    new FishingSpot(10, "Makarska", "Splitsko-dalmatinska", false, true,
        new List<Fish> { fish[1], fish[3], fish[13], fish[14], fish[6] }),

    new FishingSpot(11, "Korčula", "Dubrovačko-neretvanska", true, false,
        new List<Fish> { fish[4], fish[5], fish[11], fish[6], fish[0] })
};

        Techniques = techniques;
        Baits = baits;
        Fish = fish;
        CatchRecords = catchRecords;
        FishingLicenses = fishingLicenses;
        Users = users;
        FishingSpots = fishingSpots;
    }

    public Technique? GetTechniqueById(int id) => Techniques.FirstOrDefault(t => t.TechniqueID == id);
    public Bait? GetBaitById(int id) => Baits.FirstOrDefault(b => b.BaitID == id);
    public Fish? GetFishById(int id) => Fish.FirstOrDefault(f => f.FishID == id);
    public CatchRecord? GetCatchRecordById(int id) => CatchRecords.FirstOrDefault(c => c.CatchID == id);
    public FishingLicense? GetFishingLicenseByUserId(int userId) => FishingLicenses.FirstOrDefault(l => l.UserID == userId);
    public User? GetUserById(int id) => Users.FirstOrDefault(u => u.UserID == id);
    public FishingSpot? GetFishingSpotById(int id) => FishingSpots.FirstOrDefault(s => s.SpotID == id);
}
