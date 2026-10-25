namespace DAL.EF.Tables;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameBn { get; set; }

    public string? Slug { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
