using BLL.DTOs;
using BLL.Interfaces;

namespace BLL.Services
{
    public class ArticleLookupService : IArticleLookupService
    {
        private readonly ICategoryService categoryService;
        private readonly IUserService userService;
        private readonly IBookmarkService bookmarkService;

        public ArticleLookupService(ICategoryService categoryService, IUserService userService, IBookmarkService bookmarkService)
        {
            this.categoryService = categoryService;
            this.userService = userService;
            this.bookmarkService = bookmarkService;
        }

        public ArticleLookupsDTO Build(IEnumerable<ArticleDTO> articles, int? userId, string language)
        {
            var list = articles.ToList();
            var categories = categoryService.Get();
            return new ArticleLookupsDTO
            {
                AuthorNames = userService.GetNames(list.Select(a => a.AuthorId)),
                CategoryNames = categories.ToDictionary(c => c.CategoryId, c => c.LocalName(language)),
                CategorySlugs = categories.ToDictionary(c => c.CategoryId, c => c.Slug),
                SavedIds = userId.HasValue ? bookmarkService.GetSavedIds(userId.Value, list.Select(a => a.ArticleId)) : null
            };
        }
    }
}
