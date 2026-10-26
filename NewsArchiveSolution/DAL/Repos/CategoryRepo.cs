using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class CategoryRepo : Repository<Category>, ICategoryRepository
    {
        public CategoryRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public override List<Category> Get()
        {
            return db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();
        }

        public bool HasArticles(int categoryId)
        {
            return db.Articles.Any(a => a.CategoryId == categoryId);
        }

        public bool SlugExists(string slug, int excludeCategoryId)
        {
            return db.Categories.Any(c => c.Slug == slug && c.CategoryId != excludeCategoryId);
        }

        public bool NameExists(string name, int excludeCategoryId)
        {
            return db.Categories.Any(c => (c.Name == name || c.NameBn == name) && c.CategoryId != excludeCategoryId);
        }

        public int CountArticles(int categoryId)
        {
            return db.Articles.Count(a => a.CategoryId == categoryId);
        }

        public Dictionary<int, int> CountPublished(string? language)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.StatusId == ArticleStatusIds.Published && (language == null || a.LanguageCode == language))
                .GroupBy(a => a.CategoryId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }

        public bool DeleteMovingArticles(int categoryId, int targetCategoryId)
        {
            if (categoryId == targetCategoryId || !db.Categories.Any(c => c.CategoryId == targetCategoryId))
                return false;

            using var transaction = db.Database.BeginTransaction();
            db.Articles
                .Where(a => a.CategoryId == categoryId)
                .ExecuteUpdate(s => s.SetProperty(a => a.CategoryId, targetCategoryId));
            var deleted = db.Categories.Where(c => c.CategoryId == categoryId).ExecuteDelete() > 0;
            transaction.Commit();
            return deleted;
        }

        public List<Category> GetWithoutSlug()
        {
            return db.Categories.Where(c => c.Slug == null).ToList();
        }
    }
}
