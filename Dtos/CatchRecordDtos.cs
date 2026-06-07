using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class AttachmentDto
{
    public int AttachmentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CatchRecordDto
{
    public int CatchID { get; set; }
    public DateTime CatchDate { get; set; }
    public double Weight { get; set; }
    public double LengthCm { get; set; }
    public string Location { get; set; } = string.Empty;
    public UserSummaryDto? User { get; set; }
    public FishSummaryDto? Fish { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class CatchRecordUpsertDto
{
    [Range(1, int.MaxValue)]
    public int UserID { get; set; }

    [Range(1, int.MaxValue)]
    public int FishID { get; set; }

    public DateTime CatchDate { get; set; }

    [Range(0, 10_000)]
    public double Weight { get; set; }

    [Range(0, 10_000)]
    public double LengthCm { get; set; }

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;
}
