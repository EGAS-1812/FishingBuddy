using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class UserDto
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public FishingLicenseDto? FishingLicense { get; set; }
    public List<FishSummaryDto> FavoriteFish { get; set; } = new();
}

public class UserUpsertDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime? LicenseBeginDate { get; set; }
    public DateTime? LicenseExpirationDate { get; set; }
}
