namespace DAL.EF.Tables;

public partial class Bookmark
{
    public int UserId { get; set; }

    public int ArticleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
}
