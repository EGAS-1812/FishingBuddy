using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class UserUpsertViewModel : IValidatableObject
{
    public int UserID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public bool HasFishingLicense { get; set; }

    public DateTime? LicenseBeginDate { get; set; }

    public DateTime? LicenseExpirationDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasFishingLicense)
        {
            yield break;
        }

        if (!LicenseBeginDate.HasValue)
        {
            yield return new ValidationResult(
                "Početak dozvole je obavezan kada korisnik ima ribolovnu dozvolu.",
                [nameof(LicenseBeginDate)]);
        }

        if (!LicenseExpirationDate.HasValue)
        {
            yield return new ValidationResult(
                "Istek dozvole je obavezan kada korisnik ima ribolovnu dozvolu.",
                [nameof(LicenseExpirationDate)]);
        }

        if (LicenseBeginDate.HasValue && LicenseExpirationDate.HasValue && LicenseExpirationDate.Value.Date < LicenseBeginDate.Value.Date)
        {
            yield return new ValidationResult(
                "Datum isteka dozvole ne može biti prije datuma početka.",
                [nameof(LicenseExpirationDate)]);
        }
    }
}