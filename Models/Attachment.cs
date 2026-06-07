using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FishingBuddy.Models;

public class Attachment
{
    [Key]
    public int AttachmentID { get; set; }

    public int CatchRecordID { get; set; }

    [ForeignKey(nameof(CatchRecordID))]
    public CatchRecord CatchRecord { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
