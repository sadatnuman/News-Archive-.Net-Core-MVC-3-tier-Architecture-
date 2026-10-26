using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ITagService
    {
        List<TagDTO> Get();

        List<TagDTO> GetWithCounts(string? language = null);

        TagDTO? Get(int id);

        List<TagDTO> GetPopular(int count, string? language = null);

        List<TagDTO> GetForArticle(int articleId);

        List<ArticleDTO> GetArticles(int tagId, string? language, int page, int pageSize, out int total);

        bool NameExists(string? name, int tagId);

        bool Create(TagDTO tag);

        bool Update(TagDTO tag);

        bool Delete(int id);

        bool SetArticleTags(int articleId, IEnumerable<int>? tagIds);

        void EnsureSlugs();
    }
}
