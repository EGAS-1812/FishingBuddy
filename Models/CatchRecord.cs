using System;

namespace FishingBuddy.Models;

public class CatchRecord
{
    public int CatchID { get; set; }
    public int UserID { get; set; }
    public int FishID { get; set; }
    public DateTime CatchDate { get; set; }
    public double Weight { get; set; }
    public double LengthCm { get; set; }
    public string Location { get; set; } = string.Empty;

    public CatchRecord() { }

    public CatchRecord(int catchID, int userID, int fishID, DateTime catchDate, double weight, double lengthCm, string location)
    {
        CatchID = catchID;
        UserID = userID;
        FishID = fishID;
        CatchDate = catchDate;
        Weight = weight;
        LengthCm = lengthCm;
        Location = location ?? string.Empty;
    }
}