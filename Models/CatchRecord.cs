using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FishingBuddy.Models;

public class CatchRecord
{
    [Key]
    public int CatchID { get; set; }

    public int UserID { get; set; }
    public int FishID { get; set; }

    public DateTime CatchDate { get; set; }

    [Range(0, 10_000)]
    public double Weight { get; set; }

    [Range(0, 10_000)]
    public double LengthCm { get; set; }

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [ForeignKey(nameof(UserID))]
    [ValidateNever]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(FishID))]
    [ValidateNever]
    public virtual Fish Fish { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<Attachment> Attachments { get; set; } = new HashSet<Attachment>();

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