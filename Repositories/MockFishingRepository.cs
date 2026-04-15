using FishingBuddy.Models;

namespace FishingBuddy.Repositories;

public class MockFishingRepository : IFishingRepository
{
    public IReadOnlyList<Technique> Techniques { get; }
    public IReadOnlyList<Bait> Baits { get; }
    public IReadOnlyList<Fish> Fish { get; }
    public IReadOnlyList<CatchRecord> CatchRecords { get; }
    public IReadOnlyList<User> Users { get; }
    public IReadOnlyList<FishingSpot> FishingSpots { get; }

    public MockFishingRepository()
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
            new Bait(3, "Varalica", BaitType.Artificial, "Montirana na kopči")
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
            new User
            {
                UserID = 1,
                Username = "Ribar1",
                Email = "r1@example.com",
                CatchRecords = new List<CatchRecord> { catchRecords[0] },
                FavoriteFish = new List<Fish> { fish[0] }
            },
            new User
            {
                UserID = 2,
                Username = "Ribar2",
                Email = "r2@example.com",
                CatchRecords = new List<CatchRecord> { catchRecords[1] },
                FavoriteFish = new List<Fish> { fish[1] }
            },
            new User
            {
                UserID = 3,
                Username = "Ribar3",
                Email = "r3@example.com",
                CatchRecords = new List<CatchRecord> { catchRecords[2] },
                FavoriteFish = new List<Fish> { fish[2] }
            }
        };

        var fishingSpots = new List<FishingSpot>
        {
            new FishingSpot(1, "Rijeka", "Primorsko-goranska", hasPiers: true, boatAccess: true, mostLikelyCatch: fish),
            new FishingSpot(2, "Kraljevica", "Primorsko-goranska", hasPiers: false, boatAccess: true, mostLikelyCatch: new List<Fish> { fish[0], fish[2] }),
            new FishingSpot(3, "Bakarac", "Primorsko-goranska", hasPiers: true, boatAccess: false, mostLikelyCatch: new List<Fish> { fish[1] }),
            new FishingSpot(4, "Pazin", "Istarska", hasPiers: false, boatAccess: false, mostLikelyCatch: new List<Fish> { fish[2] }),
            new FishingSpot(5, "Zadar", "Zadarska", hasPiers: true, boatAccess: true, mostLikelyCatch: new List<Fish> { fish[0], fish[1] })
        };

        Techniques = techniques;
        Baits = baits;
        Fish = fish;
        CatchRecords = catchRecords;
        Users = users;
        FishingSpots = fishingSpots;
    }

    public Technique? GetTechniqueById(int id) => Techniques.FirstOrDefault(t => t.TechniqueID == id);
    public Bait? GetBaitById(int id) => Baits.FirstOrDefault(b => b.BaitID == id);
    public Fish? GetFishById(int id) => Fish.FirstOrDefault(f => f.FishID == id);
    public CatchRecord? GetCatchRecordById(int id) => CatchRecords.FirstOrDefault(c => c.CatchID == id);
    public User? GetUserById(int id) => Users.FirstOrDefault(u => u.UserID == id);
    public FishingSpot? GetFishingSpotById(int id) => FishingSpots.FirstOrDefault(s => s.SpotID == id);
}
