using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class FishingLicense
{
    [Key]
    public int UserID { get; set; }

    public DateTime BeginDate { get; set; }
    public DateTime ExpirationDate { get; set; }

    public virtual User User { get; set; } = null!;

    public FishingLicense() { }

    public FishingLicense(int userID, DateTime beginDate, DateTime expirationDate)
    {
        UserID = userID;
        BeginDate = beginDate;
        ExpirationDate = expirationDate;
    }

    public bool IsValidOn(DateTime date)
    {
        var currentDate = date.Date;
        return currentDate >= BeginDate.Date && currentDate <= ExpirationDate.Date;
    }
}