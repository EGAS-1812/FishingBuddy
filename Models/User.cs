using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class User
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public virtual FishingLicense? FishingLicense { get; set; }
    public virtual ICollection<Fish> FavoriteFish { get; set; } = new HashSet<Fish>();
    public virtual ICollection<CatchRecord> CatchRecords { get; set; } = new HashSet<CatchRecord>();

    public User() { }

    public User(int userID, string username, string email, IEnumerable<CatchRecord> catchRecords, IEnumerable<Fish> favouriteFish, FishingLicense? fishingLicense = null)
    {
        UserID = userID;
        Username = username ?? string.Empty;
        Email = email ?? string.Empty;
        FishingLicense = fishingLicense;
        CatchRecords = catchRecords != null ? new List<CatchRecord>(catchRecords) : new List<CatchRecord>();
        FavoriteFish = favouriteFish != null ? new List<Fish>(favouriteFish) : new List<Fish>();
    }
}