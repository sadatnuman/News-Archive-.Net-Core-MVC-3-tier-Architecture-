namespace DAL.EF.Tables;

public partial class ArticleWord
{
    public string Word { get; set; } = null!;

    public int ArticleId { get; set; }

    public virtual Article Article { get; set; } = null!;
}
