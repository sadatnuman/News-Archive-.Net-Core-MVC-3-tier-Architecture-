namespace DAL.EF.Tables;

public partial class Language
{
    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string NativeName { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
}
