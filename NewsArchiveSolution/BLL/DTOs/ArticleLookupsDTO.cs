namespace BLL.DTOs
{
    public class ArticleLookupsDTO
    {
        public Dictionary<int, string> AuthorNames { get; set; } = new();

        public Dictionary<int, string> CategoryNames { get; set; } = new();

        public Dictionary<int, string?> CategorySlugs { get; set; } = new();

        public HashSet<int>? SavedIds { get; set; }
    }
}
