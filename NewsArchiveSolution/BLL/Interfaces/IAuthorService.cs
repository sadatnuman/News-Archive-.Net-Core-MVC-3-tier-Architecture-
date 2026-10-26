using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuthorService
    {
        List<AuthorDTO> GetAuthors(string? language = null);

        AuthorDTO? GetAuthor(int userId, string? language = null);

        List<ArticleDTO> GetArticles(int userId, string? language, int page, int pageSize, out int total);
    }
}
