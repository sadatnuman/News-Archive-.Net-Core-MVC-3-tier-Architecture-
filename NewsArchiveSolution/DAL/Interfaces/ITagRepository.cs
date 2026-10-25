using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Dictionary<int, int> CountPublished(string? language = null);

        List<Tag> GetPopular(int count, string? language = null);

        List<Tag> GetForArticle(int articleId);

        bool SetArticleTags(int articleId, IEnumerable<int> tagIds);

        List<Article> GetArticles(int tagId, string? language, int page, int pageSize, out int total);

        bool NameExists(string name, int excludeTagId);

        bool SlugExists(string slug, int excludeTagId);

        List<Tag> GetWithoutSlug();
    }
}
