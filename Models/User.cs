namespace FishingBuddy.Models;

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Fish> FavoriteFish { get; set; } = new List<Fish>();
    public List<CatchRecord> CatchRecords { get; set; } = new List<CatchRecord>();

    public User() { }

    public User(int userID, string username, string email, IEnumerable <CatchRecord> catchRecords, IEnumerable <Fish> favouriteFish)
    {
        UserID = userID;
        Username = username ?? string.Empty;
        Email = email ?? string.Empty;
        CatchRecords = catchRecords != null ? new List<CatchRecord>(catchRecords) : new List<CatchRecord>();
        FavoriteFish = favouriteFish != null ? new List<Fish>(favouriteFish) : new List<Fish>();
    }
}