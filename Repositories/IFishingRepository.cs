using FishingBuddy.Models;

namespace FishingBuddy.Repositories;

public interface IFishingRepository
{
    IReadOnlyList<Technique> Techniques { get; }
    IReadOnlyList<Bait> Baits { get; }
    IReadOnlyList<Fish> Fish { get; }
    IReadOnlyList<CatchRecord> CatchRecords { get; }
    IReadOnlyList<FishingLicense> FishingLicenses { get; }
    IReadOnlyList<User> Users { get; }
    IReadOnlyList<FishingSpot> FishingSpots { get; }

    Technique? GetTechniqueById(int id);
    Bait? GetBaitById(int id);
    Fish? GetFishById(int id);
    CatchRecord? GetCatchRecordById(int id);
    FishingLicense? GetFishingLicenseByUserId(int userId);
    User? GetUserById(int id);
    FishingSpot? GetFishingSpotById(int id);
}
