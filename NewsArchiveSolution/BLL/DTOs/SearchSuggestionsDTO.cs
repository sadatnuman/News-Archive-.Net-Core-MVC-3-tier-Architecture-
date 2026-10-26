namespace BLL.DTOs
{
    public class SearchSuggestionsDTO
    {
        public string Query { get; set; } = string.Empty;

        public List<ArticleDTO> Stories { get; set; } = new();

        public List<TagDTO> Topics { get; set; } = new();

        public List<CategoryDTO> Sections { get; set; } = new();

        public List<AuthorDTO> Authors { get; set; } = new();

        public bool IsEmpty => Stories.Count == 0 && Topics.Count == 0 && Sections.Count == 0 && Authors.Count == 0;
    }
}
