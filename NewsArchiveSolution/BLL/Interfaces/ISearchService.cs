using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ISearchService
    {
        SearchSuggestionsDTO Suggest(string? query, string language, int storyCount = 6);
    }
}
