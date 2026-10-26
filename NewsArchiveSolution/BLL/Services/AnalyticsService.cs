using BLL.DTOs;
using BLL.Interfaces;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IArticleRepository articleRepo;
        private readonly IUserRepository userRepo;
        private readonly ICategoryRepository categoryRepo;

        public AnalyticsService(IDataAccessFactory factory)
        {
            articleRepo = factory.GetArticleRepository();
            userRepo = factory.GetUserRepository();
            categoryRepo = factory.GetCategoryRepository();
        }

        public int GetTotalArticles() => articleRepo.Count();

        public int GetTotalUsers() => userRepo.Count();

        public List<ArticleDTO> GetMostViewedArticles(int count)
        {
            return articleRepo.GetMostViewed(count)
                .Select(a => new ArticleDTO {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Slug = a.Slug,
                    LanguageCode = a.LanguageCode,
                    ViewsCount = a.ViewsCount
                }).ToList();
        }

        public List<(string Category, int Count)> GetArticlesPerCategory()
        {
            var counts = articleRepo.CountPerCategory();
            var categories = categoryRepo.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            return counts
                .Select(c => (categories.TryGetValue(c.Key, out var name) ? name : "Unknown", c.Value))
                .ToList();
        }
    }
}
