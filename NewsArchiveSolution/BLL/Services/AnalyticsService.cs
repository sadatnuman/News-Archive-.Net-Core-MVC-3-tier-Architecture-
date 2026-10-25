using BLL.DTOs;
using DAL.Repos;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class AnalyticsService
    {
        private readonly ArticleRepo articleRepo;
        private readonly UserRepo userRepo;
        private readonly CategoryRepo categoryRepo;
        public AnalyticsService(ArticleRepo articleRepo, UserRepo userRepo, CategoryRepo categoryRepo)
        {
            this.articleRepo = articleRepo;
            this.userRepo = userRepo;
            this.categoryRepo = categoryRepo;
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
