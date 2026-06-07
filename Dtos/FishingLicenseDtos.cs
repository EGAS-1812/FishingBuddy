using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class FishingLicenseDto
{
    public int UserID { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class FishingLicenseUpsertDto
{
    [Range(1, int.MaxValue)]
    public int UserID { get; set; }

    public DateTime BeginDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}
