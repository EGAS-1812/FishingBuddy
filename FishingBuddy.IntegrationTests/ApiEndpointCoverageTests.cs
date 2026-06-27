using System.Net;
using System.Net.Http.Json;
using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FishingBuddy.IntegrationTests;

public class ApiEndpointCoverageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiEndpointCoverageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BaitApi_ShouldFilterAndRejectDeleteWhenReferenced()
    {
        await ResetDatabaseAsync();

        var liveBaitId = await SeedBaitAsync("Crv", BaitType.Live, "Prirodno", 2.5m);
        await SeedBaitAsync("Kukuruz", BaitType.Artificial, "Mix", 1.2m);
        var techniqueId = await SeedTechniqueAsync("Feeder", "Spora tehnika", "https://example.com/feeder");
        await SeedFishAsync(liveBaitId, techniqueId, "Som", Season.Summer, FishFlesh.White);

        var filteredByQuery = await _client.GetFromJsonAsync<List<BaitDto>>("/api/baits?q=Crv");
        filteredByQuery.Should().NotBeNull();
        filteredByQuery.Should().HaveCount(1);
        filteredByQuery![0].BaitName.Should().Be("Crv");

        var filteredByType = await _client.GetFromJsonAsync<List<BaitDto>>("/api/baits?baitType=Artificial");
        filteredByType.Should().NotBeNull();
        filteredByType.Should().HaveCount(1);
        filteredByType![0].BaitName.Should().Be("Kukuruz");

        var invalidCreate = await _client.PostAsJsonAsync("/api/baits", new BaitUpsertDto { BaitName = string.Empty });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/baits", new BaitUpsertDto
        {
            BaitName = "Worm Plus",
            BaitType = BaitType.Live,
            PreparationMethod = "Hlađeno",
            AveragePriceEur = 3m
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<BaitDto>();
        created.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/baits/{created!.BaitID}", new BaitUpsertDto
        {
            BaitName = "Worm Plus Updated",
            BaitType = BaitType.Live,
            PreparationMethod = "Suho",
            AveragePriceEur = 3.5m
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/baits/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var conflictDelete = await _client.DeleteAsync($"/api/baits/{liveBaitId}");
        conflictDelete.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var deleteResponse = await _client.DeleteAsync($"/api/baits/{created.BaitID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TechniqueApi_ShouldFilterAndRejectDeleteWhenReferenced()
    {
        await ResetDatabaseAsync();

        var spinningId = await SeedTechniqueAsync("Spinning", "Brza i aktivna tehnika", "https://example.com/spinning");
        await SeedTechniqueAsync("Jigging", "Za dubinu", "https://example.com/jigging");
        var baitId = await SeedBaitAsync("Kukuruz", BaitType.Artificial, "Mix", 1.2m);
        await SeedFishAsync(baitId, spinningId, "Šaran", Season.Spring, FishFlesh.Red);

        var filtered = await _client.GetFromJsonAsync<List<TechniqueDto>>("/api/techniques?q=Spin");
        filtered.Should().NotBeNull();
        filtered.Should().HaveCount(1);
        filtered![0].TechniqueName.Should().Be("Spinning");

        var invalidCreate = await _client.PostAsJsonAsync("/api/techniques", new TechniqueUpsertDto { TechniqueName = string.Empty });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/techniques", new TechniqueUpsertDto
        {
            TechniqueName = "Trolling",
            PerformanceNote = "Spora tehnika",
            TutorialUrl = "https://example.com/trolling"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TechniqueDto>();
        created.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/techniques/{created!.TechniqueID}", new TechniqueUpsertDto
        {
            TechniqueName = "Trolling Updated",
            PerformanceNote = "Ažurirano",
            TutorialUrl = "https://example.com/trolling-updated"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/techniques/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var conflictDelete = await _client.DeleteAsync($"/api/techniques/{spinningId}");
        conflictDelete.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var deleteResponse = await _client.DeleteAsync($"/api/techniques/{created.TechniqueID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FishApi_ShouldFilterAndValidateReferences()
    {
        await ResetDatabaseAsync();

        var baitId = await SeedBaitAsync("Crv", BaitType.Live, "Prirodno", 1.0m);
        var techniqueId = await SeedTechniqueAsync("Feeder", "Spora precizna metoda", "https://example.com/feeder");
        await SeedFishAsync(baitId, techniqueId, "Som", Season.Summer, FishFlesh.White);
        await SeedFishAsync(baitId, techniqueId, "Šaran", Season.Spring, FishFlesh.Red);

        var filteredByQuery = await _client.GetFromJsonAsync<List<FishDto>>("/api/fish?q=Som");
        filteredByQuery.Should().NotBeNull();
        filteredByQuery.Should().HaveCount(1);
        filteredByQuery![0].SpeciesName.Should().Be("Som");

        var filteredBySeason = await _client.GetFromJsonAsync<List<FishDto>>("/api/fish?season=Spring");
        filteredBySeason.Should().NotBeNull();
        filteredBySeason.Should().HaveCount(1);
        filteredBySeason![0].SpeciesName.Should().Be("Šaran");

        var filteredByFlesh = await _client.GetFromJsonAsync<List<FishDto>>("/api/fish?fleshColor=White");
        filteredByFlesh.Should().NotBeNull();
        filteredByFlesh.Should().HaveCount(1);
        filteredByFlesh![0].SpeciesName.Should().Be("Som");

        var invalidCreate = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = string.Empty,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var invalidBait = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = "Smuđ",
            CatchSeason = Season.Autumn,
            FleshColor = FishFlesh.White,
            FavouriteBaitID = 99999,
            PreferredMethodID = techniqueId,
            Equipment = CreateEquipment()
        });
        invalidBait.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var invalidTechnique = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = "Smuđ",
            CatchSeason = Season.Autumn,
            FleshColor = FishFlesh.White,
            FavouriteBaitID = baitId,
            PreferredMethodID = 99999,
            Equipment = CreateEquipment()
        });
        invalidTechnique.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = "Smuđ",
            CatchSeason = Season.Autumn,
            FleshColor = FishFlesh.White,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId,
            Equipment = CreateEquipment()
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FishDto>();
        created.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/fish/{created!.FishID}", new FishUpsertDto
        {
            SpeciesName = "Smuđ Updated",
            CatchSeason = Season.Winter,
            FleshColor = FishFlesh.Red,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId,
            Equipment = new EquipmentDto
            {
                ReelSize = 4000,
                ReelType = FReelType.Spinning,
                RodLengthMeters = 3.0m,
                RodAction = FRodAction.Heavy,
                RodMinWeightGrams = 20,
                RodMaxWeightGrams = 60,
                LineType = FLineType.Braided,
                LineThicknessMm = 0.30m
            }
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/fish/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/fish/{created.FishID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FishingSpotApi_ShouldFilterCollection()
    {
        await ResetDatabaseAsync();

        await SeedFishingSpotAsync("Jarun", "Zagreb", true, false);
        await SeedFishingSpotAsync("Savica", "Zagreb", false, true);
        await SeedFishingSpotAsync("Drava", "Osijek", false, true);

        var filteredByQuery = await _client.GetFromJsonAsync<List<FishingSpotDto>>("/api/fishing-spots?q=Jarun");
        filteredByQuery.Should().NotBeNull();
        filteredByQuery.Should().HaveCount(1);
        filteredByQuery![0].SpotName.Should().Be("Jarun");

        var filteredByRegion = await _client.GetFromJsonAsync<List<FishingSpotDto>>("/api/fishing-spots?region=Zagreb");
        filteredByRegion.Should().NotBeNull();
        filteredByRegion.Should().HaveCount(2);

        var invalidCreate = await _client.PostAsJsonAsync("/api/fishing-spots", new FishingSpotUpsertDto
        {
            SpotName = string.Empty,
            Region = string.Empty
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/fishing-spots", new FishingSpotUpsertDto
        {
            SpotName = "Bundek",
            Region = "Zagreb",
            HasPiers = true,
            BoatAccess = false
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FishingSpotDto>();
        created.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/fishing-spots/{created!.SpotID}", new FishingSpotUpsertDto
        {
            SpotName = "Bundek East",
            Region = "Zagreb",
            HasPiers = true,
            BoatAccess = true
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/fishing-spots/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/fishing-spots/{created.SpotID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UserApi_ShouldFilterAndManageFishingLicenseLifecycle()
    {
        await ResetDatabaseAsync();

        await SeedUserAsync("alpha-user", "alpha-user@fishingbuddy.hr");
        await SeedUserAsync("beta-user", "beta-user@fishingbuddy.hr");

        var filtered = await _client.GetFromJsonAsync<List<UserDto>>("/api/users?q=alpha");
        filtered.Should().NotBeNull();
        filtered.Should().HaveCount(1);
        filtered![0].Username.Should().Be("alpha-user");

        var invalidCreate = await _client.PostAsJsonAsync("/api/users", new UserUpsertDto
        {
            Username = string.Empty,
            Email = "not-an-email"
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/users", new UserUpsertDto
        {
            Username = "licensed-user",
            Email = "licensed-user@fishingbuddy.hr",
            LicenseBeginDate = DateTime.UtcNow.Date,
            LicenseExpirationDate = DateTime.UtcNow.Date.AddYears(1)
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        created.Should().NotBeNull();
        created!.FishingLicense.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created.UserID}", new UserUpsertDto
        {
            Username = "licensed-user-updated",
            Email = "licensed-user-updated@fishingbuddy.hr"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getById = await _client.GetFromJsonAsync<UserDto>($"/api/users/{created.UserID}");
        getById.Should().NotBeNull();
        getById!.FishingLicense.Should().BeNull();

        var missing = await _client.GetAsync("/api/users/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created.UserID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FishingLicenseApi_ShouldFilterAndHandleConflictAndRouteMismatch()
    {
        await ResetDatabaseAsync();

        var validUserId = await SeedUserAsync("valid-license-user", "valid-license-user@fishingbuddy.hr");
        var expiredUserId = await SeedUserAsync("expired-license-user", "expired-license-user@fishingbuddy.hr");

        await SeedFishingLicenseAsync(validUserId, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(30));
        await SeedFishingLicenseAsync(expiredUserId, DateTime.UtcNow.Date.AddMonths(-3), DateTime.UtcNow.Date.AddMonths(-1));

        var validToday = await _client.GetFromJsonAsync<List<FishingLicenseDto>>("/api/fishing-licenses?validToday=true");
        validToday.Should().NotBeNull();
        validToday.Should().HaveCount(1);
        validToday![0].UserID.Should().Be(validUserId);

        var duplicateCreate = await _client.PostAsJsonAsync("/api/fishing-licenses", new FishingLicenseUpsertDto
        {
            UserID = validUserId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddMonths(6)
        });
        duplicateCreate.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var invalidRange = await _client.PostAsJsonAsync("/api/fishing-licenses", new FishingLicenseUpsertDto
        {
            UserID = expiredUserId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        invalidRange.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updateMismatch = await _client.PutAsJsonAsync($"/api/fishing-licenses/{validUserId}", new FishingLicenseUpsertDto
        {
            UserID = expiredUserId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddMonths(1)
        });
        updateMismatch.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var updateResponse = await _client.PutAsJsonAsync($"/api/fishing-licenses/{validUserId}", new FishingLicenseUpsertDto
        {
            UserID = validUserId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddYears(1)
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/fishing-licenses/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/fishing-licenses/{validUserId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CatchRecordApi_ShouldFilterAndValidateForeignKeys()
    {
        await ResetDatabaseAsync();

        var (userId, fishId) = await SeedCatchRecordDependenciesAsync();
        await SeedCatchRecordAsync(userId, fishId, "Sava", DateTime.UtcNow.Date.AddDays(-1), 2.5, 45);
        await SeedCatchRecordAsync(userId, fishId, "Drava", DateTime.UtcNow.Date, 3.2, 48);

        var filteredByQuery = await _client.GetFromJsonAsync<List<CatchRecordDto>>("/api/catch-records?q=Drava");
        filteredByQuery.Should().NotBeNull();
        filteredByQuery.Should().HaveCount(1);
        filteredByQuery![0].Location.Should().Be("Drava");

        var filteredByUser = await _client.GetFromJsonAsync<List<CatchRecordDto>>($"/api/catch-records?userId={userId}");
        filteredByUser.Should().NotBeNull();
        filteredByUser.Should().HaveCount(2);

        var filteredByFish = await _client.GetFromJsonAsync<List<CatchRecordDto>>($"/api/catch-records?fishId={fishId}");
        filteredByFish.Should().NotBeNull();
        filteredByFish.Should().HaveCount(2);

        var invalidUser = await _client.PostAsJsonAsync("/api/catch-records", new CatchRecordUpsertDto
        {
            UserID = 99999,
            FishID = fishId,
            CatchDate = DateTime.UtcNow,
            Weight = 2.5,
            LengthCm = 45,
            Location = "Sava"
        });
        invalidUser.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var invalidFish = await _client.PostAsJsonAsync("/api/catch-records", new CatchRecordUpsertDto
        {
            UserID = userId,
            FishID = 99999,
            CatchDate = DateTime.UtcNow,
            Weight = 2.5,
            LengthCm = 45,
            Location = "Sava"
        });
        invalidFish.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/catch-records", new CatchRecordUpsertDto
        {
            UserID = userId,
            FishID = fishId,
            CatchDate = DateTime.UtcNow,
            Weight = 2.5,
            LengthCm = 45,
            Location = "Sava"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CatchRecordDto>();
        created.Should().NotBeNull();

        var updateResponse = await _client.PutAsJsonAsync($"/api/catch-records/{created!.CatchID}", new CatchRecordUpsertDto
        {
            UserID = userId,
            FishID = fishId,
            CatchDate = DateTime.UtcNow,
            Weight = 3.2,
            LengthCm = 48,
            Location = "Drava"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/catch-records/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/catch-records/{created.CatchID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static EquipmentDto CreateEquipment() => new()
    {
        ReelSize = 3000,
        ReelType = FReelType.Spinning,
        RodLengthMeters = 2.7m,
        RodAction = FRodAction.Medium,
        RodMinWeightGrams = 10,
        RodMaxWeightGrams = 40,
        LineType = FLineType.Nylon,
        LineThicknessMm = 0.25m
    };

    private Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    private async Task<int> SeedTechniqueAsync(string name, string performanceNote, string tutorialUrl)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var technique = new Technique
        {
            TechniqueName = name,
            PerformanceNote = performanceNote,
            TutorialUrl = tutorialUrl
        };

        db.Techniques.Add(technique);
        await db.SaveChangesAsync();
        return technique.TechniqueID;
    }

    private async Task<int> SeedBaitAsync(string name, BaitType baitType, string preparationMethod, decimal averagePriceEur)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var bait = new Bait
        {
            BaitName = name,
            BaitType = baitType,
            PreparationMethod = preparationMethod,
            AveragePriceEur = averagePriceEur
        };

        db.Baits.Add(bait);
        await db.SaveChangesAsync();
        return bait.BaitID;
    }

    private async Task<int> SeedFishAsync(int baitId, int techniqueId, string speciesName, Season season, FishFlesh fleshColor)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var fish = new Fish
        {
            SpeciesName = speciesName,
            CatchSeason = season,
            FleshColor = fleshColor,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId,
            Equipment = new Equipment
            {
                FReel = new FReel(4000, FReelType.Spinning),
                FRod = new FRod(3.0m, FRodAction.Heavy, 30, 80),
                FLine = new FLine(FLineType.Braided, 0.35m)
            }
        };

        db.Fish.Add(fish);
        await db.SaveChangesAsync();
        return fish.FishID;
    }

    private async Task<int> SeedFishingSpotAsync(string spotName, string region, bool hasPiers, bool boatAccess)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var spot = new FishingSpot
        {
            SpotName = spotName,
            Region = region,
            HasPiers = hasPiers,
            BoatAccess = boatAccess
        };

        db.FishingSpots.Add(spot);
        await db.SaveChangesAsync();
        return spot.SpotID;
    }

    private async Task<int> SeedUserAsync(string username, string email, DateTime? licenseBeginDate = null, DateTime? licenseExpirationDate = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var user = new User
        {
            Username = username,
            Email = email
        };

        if (licenseBeginDate.HasValue && licenseExpirationDate.HasValue)
        {
            user.FishingLicense = new FishingLicense
            {
                BeginDate = licenseBeginDate.Value,
                ExpirationDate = licenseExpirationDate.Value
            };
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (user.FishingLicense != null)
        {
            user.FishingLicense.UserID = user.UserID;
            await db.SaveChangesAsync();
        }

        return user.UserID;
    }

    private async Task<int> SeedFishingLicenseAsync(int userId, DateTime beginDate, DateTime expirationDate)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var license = new FishingLicense
        {
            UserID = userId,
            BeginDate = beginDate,
            ExpirationDate = expirationDate
        };

        db.FishingLicenses.Add(license);
        await db.SaveChangesAsync();
        return license.UserID;
    }

    private async Task<int> SeedCatchRecordAsync(int userId, int fishId, string location, DateTime? catchDate = null, double weight = 2.5, double lengthCm = 45)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var record = new CatchRecord
        {
            UserID = userId,
            FishID = fishId,
            CatchDate = catchDate ?? DateTime.UtcNow,
            Weight = weight,
            LengthCm = lengthCm,
            Location = location
        };

        db.CatchRecords.Add(record);
        await db.SaveChangesAsync();
        return record.CatchID;
    }

    private async Task<(int userId, int fishId)> SeedCatchRecordDependenciesAsync()
    {
        var baitId = await SeedBaitAsync("Crv", BaitType.Live, "Prirodno", 1.0m);
        var techniqueId = await SeedTechniqueAsync("Feeder", "Spora precizna metoda", "https://example.com/feeder");
        var fishId = await SeedFishAsync(baitId, techniqueId, "Som", Season.Summer, FishFlesh.White);
        var userId = await SeedUserAsync("record-user", "record-user@fishingbuddy.hr");

        return (userId, fishId);
    }
}
