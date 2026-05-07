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

    void AddTechnique(Technique technique);
    void UpdateTechnique(Technique technique);
    void DeleteTechnique(int id);

    void AddBait(Bait bait);
    void UpdateBait(Bait bait);
    void DeleteBait(int id);

    void AddFish(Fish fish);
    void UpdateFish(Fish fish);
    void DeleteFish(int id);

    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(int id);

    void AddFishingSpot(FishingSpot spot);
    void UpdateFishingSpot(FishingSpot spot);
    void DeleteFishingSpot(int id);

    void AddCatchRecord(CatchRecord catchRecord);
    void UpdateCatchRecord(CatchRecord catchRecord);
    void DeleteCatchRecord(int id);
}
