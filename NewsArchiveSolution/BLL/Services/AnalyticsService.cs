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
        public int GetTotalArticles() => articleRepo.Get().Count;
        public int GetTotalUsers() => userRepo.Get().Count;
        public List<ArticleDTO> GetMostViewedArticles(int count)
        {
            return articleRepo.Get().OrderByDescending(a => a.ViewsCount).Take(count)
                .Select(a => new ArticleDTO {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    ViewsCount = a.ViewsCount
                }).ToList();
        }
        public List<(string Category, int Count)> GetArticlesPerCategory()
        {
            var articles = articleRepo.Get();
            var categories = categoryRepo.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            return articles.GroupBy(a => a.CategoryId)
                .Select(g => (categories.TryGetValue(g.Key, out var name) ? name : "Unknown", g.Count()))
                .ToList();
        }
    }
}
