namespace DAL.EF.Tables;

public partial class ReadingHistory
{
    public int UserId { get; set; }

    public int ArticleId { get; set; }

    public DateTime FirstReadAt { get; set; }

    public DateTime LastReadAt { get; set; }

    public int ReadCount { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
}
