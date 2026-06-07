using System.Net;
using System.Net.Http.Json;
using FishingBuddy.Data;
using FishingBuddy.Dtos;
using FishingBuddy.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FishingBuddy.IntegrationTests;

public class ApiCrudIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiCrudIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TechniqueApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/techniques", new TechniqueUpsertDto { TechniqueName = string.Empty });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/techniques", new TechniqueUpsertDto
        {
            TechniqueName = "Jigging",
            PerformanceNote = "Brza i precizna metoda",
            TutorialUrl = "https://example.com/jigging"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TechniqueDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/techniques/{created!.TechniqueID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/techniques/{created.TechniqueID}", new TechniqueUpsertDto
        {
            TechniqueName = "Jigging Updated",
            PerformanceNote = "Ažurirano",
            TutorialUrl = "https://example.com/jigging-updated"
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/techniques/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/techniques/{created.TechniqueID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task BaitApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/baits", new BaitUpsertDto { BaitName = string.Empty });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/baits", new BaitUpsertDto
        {
            BaitName = "Crv",
            BaitType = BaitType.Live,
            PreparationMethod = "Prirodno",
            AveragePriceEur = 2.5m
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<BaitDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/baits/{created!.BaitID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/baits/{created.BaitID}", new BaitUpsertDto
        {
            BaitName = "Crv Plus",
            BaitType = BaitType.Live,
            PreparationMethod = "Hlađeno",
            AveragePriceEur = 3m
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/baits/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/baits/{created.BaitID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FishApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();
        var (baitId, techniqueId) = await SeedFishDependenciesAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = string.Empty,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/fish", new FishUpsertDto
        {
            SpeciesName = "Šaran",
            CatchSeason = Season.Spring,
            FleshColor = FishFlesh.White,
            FavouriteBaitID = baitId,
            PreferredMethodID = techniqueId,
            Equipment = new EquipmentDto
            {
                ReelSize = 3000,
                ReelType = FReelType.Spinning,
                RodLengthMeters = 2.7m,
                RodAction = FRodAction.Medium,
                RodMinWeightGrams = 10,
                RodMaxWeightGrams = 40,
                LineType = FLineType.Nylon,
                LineThicknessMm = 0.25m
            }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FishDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/fish/{created!.FishID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/fish/{created.FishID}", new FishUpsertDto
        {
            SpeciesName = "Šaran XXL",
            CatchSeason = Season.Summer,
            FleshColor = FishFlesh.White,
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
    public async Task FishingSpotApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/fishing-spots", new FishingSpotUpsertDto
        {
            SpotName = string.Empty,
            Region = string.Empty
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/fishing-spots", new FishingSpotUpsertDto
        {
            SpotName = "Jarun",
            Region = "Zagreb",
            HasPiers = true,
            BoatAccess = false
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FishingSpotDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/fishing-spots/{created!.SpotID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/fishing-spots/{created.SpotID}", new FishingSpotUpsertDto
        {
            SpotName = "Jarun - Istok",
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
    public async Task UserApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/users", new UserUpsertDto
        {
            Username = string.Empty,
            Email = "not-an-email"
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/users", new UserUpsertDto
        {
            Username = "test-user",
            Email = "test-user@fishingbuddy.hr",
            LicenseBeginDate = DateTime.UtcNow.Date,
            LicenseExpirationDate = DateTime.UtcNow.Date.AddYears(1)
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/users/{created!.UserID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created.UserID}", new UserUpsertDto
        {
            Username = "updated-user",
            Email = "updated-user@fishingbuddy.hr"
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/users/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created.UserID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FishingLicenseApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();
        var userId = await SeedUserOnlyAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/fishing-licenses", new FishingLicenseUpsertDto
        {
            UserID = userId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var createResponse = await _client.PostAsJsonAsync("/api/fishing-licenses", new FishingLicenseUpsertDto
        {
            UserID = userId,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddMonths(6)
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<FishingLicenseDto>();
        created.Should().NotBeNull();

        var getById = await _client.GetAsync($"/api/fishing-licenses/{created!.UserID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/fishing-licenses/{created.UserID}", new FishingLicenseUpsertDto
        {
            UserID = created.UserID,
            BeginDate = DateTime.UtcNow.Date,
            ExpirationDate = DateTime.UtcNow.Date.AddYears(1)
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missing = await _client.GetAsync("/api/fishing-licenses/99999");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleteResponse = await _client.DeleteAsync($"/api/fishing-licenses/{created.UserID}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CatchRecordApi_ShouldSupportCrud_NotFound_AndValidation()
    {
        await ResetDatabaseAsync();
        var (userId, fishId) = await SeedCatchRecordDependenciesAsync();

        var invalidCreate = await _client.PostAsJsonAsync("/api/catch-records", new CatchRecordUpsertDto
        {
            UserID = userId,
            FishID = fishId,
            CatchDate = DateTime.UtcNow,
            Weight = -1,
            LengthCm = 10,
            Location = "Sava"
        });
        invalidCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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

        var getById = await _client.GetAsync($"/api/catch-records/{created!.CatchID}");
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"/api/catch-records/{created.CatchID}", new CatchRecordUpsertDto
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

    private Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    private async Task<(int baitId, int techniqueId)> SeedFishDependenciesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var bait = new Bait { BaitName = "Kukuruz", BaitType = BaitType.Artificial, PreparationMethod = "Mix", AveragePriceEur = 1.2m };
        var technique = new Technique { TechniqueName = "Spinning", PerformanceNote = "", TutorialUrl = "https://example.com/spinning" };

        db.Baits.Add(bait);
        db.Techniques.Add(technique);
        await db.SaveChangesAsync();

        return (bait.BaitID, technique.TechniqueID);
    }

    private async Task<int> SeedUserOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var user = new User { Username = "license-user", Email = "license-user@fishingbuddy.hr" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.UserID;
    }

    private async Task<(int userId, int fishId)> SeedCatchRecordDependenciesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FishingBuddyDbContext>();

        var bait = new Bait { BaitName = "Crv", BaitType = BaitType.Live, PreparationMethod = "", AveragePriceEur = 1.0m };
        var technique = new Technique { TechniqueName = "Feeder", PerformanceNote = "", TutorialUrl = "https://example.com/feeder" };
        db.Baits.Add(bait);
        db.Techniques.Add(technique);
        await db.SaveChangesAsync();

        var fish = new Fish
        {
            SpeciesName = "Som",
            CatchSeason = Season.Summer,
            FavouriteBaitID = bait.BaitID,
            PreferredMethodID = technique.TechniqueID,
            FleshColor = FishFlesh.White,
            Equipment = new Equipment
            {
                FReel = new FReel(4000, FReelType.Spinning),
                FRod = new FRod(3.0m, FRodAction.Heavy, 30, 80),
                FLine = new FLine(FLineType.Braided, 0.35m)
            }
        };

        var user = new User { Username = "record-user", Email = "record-user@fishingbuddy.hr" };
        db.Fish.Add(fish);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (user.UserID, fish.FishID);
    }
}
