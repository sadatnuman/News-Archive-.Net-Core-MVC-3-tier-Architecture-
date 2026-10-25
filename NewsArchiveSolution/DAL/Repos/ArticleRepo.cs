using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repos
{
    public class ArticleRepo
    {
        NewsArchiveDbContext db;
        public ArticleRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(Article a)
        {
            db.Articles.Add(a);
            return db.SaveChanges() > 0;
        }
        public Article? Get(int id)
        {
            return db.Articles.Find(id);
        }
        public List<Article> Get()
        {
            return db.Articles.AsNoTracking().OrderByDescending(a => a.PublishDate).ToList();
        }
        public bool Update(Article a)
        {
            var exobj = Get(a.ArticleId);
            if (exobj == null) return false;
            db.Entry(exobj).CurrentValues.SetValues(a);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            if (exobj == null) return false;
            db.Articles.Remove(exobj);
            return db.SaveChanges() > 0;
        }

        public List<Article> GetByCategoryId(int categoryId)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        public Article? GetWithDetails(int id)
        {
            return db.Articles.AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Tags)
                .Include(a => a.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefault(a => a.ArticleId == id);
        }

        public void IncrementViewCount(int articleId)
        {
            db.Articles
                .Where(a => a.ArticleId == articleId)
                .ExecuteUpdate(s => s.SetProperty(a => a.ViewsCount, a => a.ViewsCount + 1));
        }

        public List<Article> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total)
        {
            var query = db.Articles.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title.Contains(keyword) || a.Summary.Contains(keyword) || a.Content.Contains(keyword));
            if (from.HasValue)
                query = query.Where(a => a.PublishDate >= from.Value);
            if (to.HasValue)
                // "to" is a date picked by the user, so include that whole day
                query = query.Where(a => a.PublishDate < to.Value.Date.AddDays(1));

            total = query.Count();
            return query.OrderByDescending(a => a.PublishDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public List<Article> GetBreaking()
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.IsBreakingNews)
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        public bool HasBreakingNews()
        {
            return db.Articles.Any(a => a.IsBreakingNews);
        }

        // Trending score: one point per view, two points per comment
        public List<Article> GetTrending(int count)
        {
            return db.Articles.AsNoTracking()
                .OrderByDescending(a => a.ViewsCount + a.Comments.Count() * 2)
                .ThenByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<Article> GetMostViewed(int count)
        {
            return db.Articles.AsNoTracking()
                .OrderByDescending(a => a.ViewsCount)
                .Take(count)
                .ToList();
        }

        // Same category first, then articles sharing a tag, then most popular
        public List<Article> GetRecommended(int articleId, int count)
        {
            var target = db.Articles.AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .Select(a => new { a.CategoryId, TagIds = a.Tags.Select(t => t.TagId).ToList() })
                .FirstOrDefault();
            if (target == null) return new List<Article>();

            var result = db.Articles.AsNoTracking()
                .Where(a => a.ArticleId != articleId && a.CategoryId == target.CategoryId)
                .OrderByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();

            if (result.Count < count && target.TagIds.Count > 0)
            {
                var excluded = result.Select(a => a.ArticleId).Append(articleId).ToList();
                result.AddRange(db.Articles.AsNoTracking()
                    .Where(a => !excluded.Contains(a.ArticleId) && a.Tags.Any(t => target.TagIds.Contains(t.TagId)))
                    .OrderByDescending(a => a.PublishDate)
                    .Take(count - result.Count)
                    .ToList());
            }

            if (result.Count < count)
            {
                var excluded = result.Select(a => a.ArticleId).Append(articleId).ToList();
                result.AddRange(db.Articles.AsNoTracking()
                    .Where(a => !excluded.Contains(a.ArticleId))
                    .OrderByDescending(a => a.ViewsCount)
                    .ThenByDescending(a => a.PublishDate)
                    .Take(count - result.Count)
                    .ToList());
            }
            return result;
        }

        public int Count()
        {
            return db.Articles.Count();
        }

        public int GetCommentCount(int articleId)
        {
            return db.Comments.Count(c => c.ArticleId == articleId);
        }

        public Dictionary<int, int> CountPerCategory()
        {
            return db.Articles
                .GroupBy(a => a.CategoryId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }
    }
}
