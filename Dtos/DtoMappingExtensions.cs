using FishingBuddy.Models;

namespace FishingBuddy.Dtos;

public static class DtoMappingExtensions
{
    public static TechniqueDto ToDto(this Technique entity) => new()
    {
        TechniqueID = entity.TechniqueID,
        TechniqueName = entity.TechniqueName,
        PerformanceNote = entity.PerformanceNote,
        TutorialUrl = entity.TutorialUrl
    };

    public static BaitDto ToDto(this Bait entity) => new()
    {
        BaitID = entity.BaitID,
        BaitName = entity.BaitName,
        BaitType = entity.BaitType,
        PreparationMethod = entity.PreparationMethod,
        AveragePriceEur = entity.AveragePriceEur
    };

    public static FishDto ToDto(this Fish entity) => new()
    {
        FishID = entity.FishID,
        SpeciesName = entity.SpeciesName,
        CatchSeason = entity.CatchSeason,
        FleshColor = entity.FleshColor,
        FavouriteBait = entity.FavouriteBait == null ? null : new BaitSummaryDto
        {
            BaitID = entity.FavouriteBait.BaitID,
            BaitName = entity.FavouriteBait.BaitName
        },
        PreferredMethod = entity.PreferredMethod == null ? null : new TechniqueSummaryDto
        {
            TechniqueID = entity.PreferredMethod.TechniqueID,
            TechniqueName = entity.PreferredMethod.TechniqueName
        },
        Equipment = entity.Equipment.ToDto()
    };

    public static EquipmentDto ToDto(this Equipment equipment) => new()
    {
        ReelSize = equipment.FReel.Size,
        ReelType = equipment.FReel.Type,
        RodLengthMeters = equipment.FRod.LengthMeters,
        RodAction = equipment.FRod.Action,
        RodMinWeightGrams = equipment.FRod.MinWeightRatingGrams,
        RodMaxWeightGrams = equipment.FRod.MaxWeightRatingGrams,
        LineType = equipment.FLine.Type,
        LineThicknessMm = equipment.FLine.ThicknessMm
    };

    public static FishingSpotDto ToDto(this FishingSpot entity) => new()
    {
        SpotID = entity.SpotID,
        SpotName = entity.SpotName,
        Region = entity.Region,
        HasPiers = entity.HasPiers,
        BoatAccess = entity.BoatAccess,
        MostLikelyCatch = entity.MostLikelyCatch
            .Select(f => new FishSummaryDto { FishID = f.FishID, SpeciesName = f.SpeciesName })
            .ToList()
    };

    public static FishingLicenseDto ToDto(this FishingLicense entity) => new()
    {
        UserID = entity.UserID,
        BeginDate = entity.BeginDate,
        ExpirationDate = entity.ExpirationDate
    };

    public static UserDto ToDto(this User entity) => new()
    {
        UserID = entity.UserID,
        Username = entity.Username,
        Email = entity.Email,
        FishingLicense = entity.FishingLicense?.ToDto(),
        FavoriteFish = entity.FavoriteFish
            .Select(f => new FishSummaryDto { FishID = f.FishID, SpeciesName = f.SpeciesName })
            .ToList()
    };

    public static CatchRecordDto ToDto(this CatchRecord entity) => new()
    {
        CatchID = entity.CatchID,
        CatchDate = entity.CatchDate,
        Weight = entity.Weight,
        LengthCm = entity.LengthCm,
        Location = entity.Location,
        User = entity.User == null ? null : new UserSummaryDto
        {
            UserID = entity.User.UserID,
            Username = entity.User.Username,
            Email = entity.User.Email
        },
        Fish = entity.Fish == null ? null : new FishSummaryDto
        {
            FishID = entity.Fish.FishID,
            SpeciesName = entity.Fish.SpeciesName
        },
        Attachments = entity.Attachments
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new AttachmentDto
            {
                AttachmentID = a.AttachmentID,
                FileName = a.FileName,
                FilePath = a.FilePath,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToList()
    };
}
