using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IArticleLookupService
    {
        ArticleLookupsDTO Build(IEnumerable<ArticleDTO> articles, int? userId, string language);
    }
}
