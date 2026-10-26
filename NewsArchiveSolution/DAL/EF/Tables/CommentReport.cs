namespace DAL.EF.Tables;

public partial class CommentReport
{
    public int CommentId { get; set; }

    public int UserId { get; set; }

    public int ReasonId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Comment Comment { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ReportReason Reason { get; set; } = null!;
}
