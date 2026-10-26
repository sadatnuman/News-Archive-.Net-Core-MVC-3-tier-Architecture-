namespace DAL.EF.Tables;

public partial class ReportReason
{
    public int ReasonId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CommentReport> Reports { get; set; } = new List<CommentReport>();
}
