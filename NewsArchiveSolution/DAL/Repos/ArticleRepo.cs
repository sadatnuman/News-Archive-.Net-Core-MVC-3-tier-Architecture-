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

        private IQueryable<Article> Published()
        {
            return db.Articles.AsNoTracking().Where(a => a.StatusId == ArticleStatusIds.Published);
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
            return Published()
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
                .FirstOrDefault(a => a.ArticleId == id);
        }

        public void IncrementViewCount(int articleId)
        {
            db.Articles
                .Where(a => a.ArticleId == articleId && a.StatusId == ArticleStatusIds.Published)
                .ExecuteUpdate(s => s.SetProperty(a => a.ViewsCount, a => a.ViewsCount + 1));
        }

        public List<Article> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total)
        {
            var query = Published();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title.Contains(keyword) || a.Summary.Contains(keyword) || a.Content.Contains(keyword));
            if (from.HasValue)
                query = query.Where(a => a.PublishDate >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.PublishDate < to.Value.Date.AddDays(1));

            total = query.Count();
            return query.OrderByDescending(a => a.PublishDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public List<Article> GetBreaking()
        {
            return Published()
                .Where(a => a.IsBreakingNews)
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        public bool HasBreakingNews()
        {
            return Published().Any(a => a.IsBreakingNews);
        }

        public List<Article> GetTrending(int count)
        {
            return Published()
                .OrderByDescending(a => a.ViewsCount + a.Comments.Count() * 2)
                .ThenByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<Article> GetMostViewed(int count)
        {
            return Published()
                .OrderByDescending(a => a.ViewsCount)
                .Take(count)
                .ToList();
        }

        public List<Article> GetLatest(int count)
        {
            return Published()
                .OrderByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<Article> GetRecommended(int articleId, int count)
        {
            var target = db.Articles.AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .Select(a => new { a.CategoryId, TagIds = a.Tags.Select(t => t.TagId).ToList() })
                .FirstOrDefault();
            if (target == null) return new List<Article>();

            var result = Published()
                .Where(a => a.ArticleId != articleId && a.CategoryId == target.CategoryId)
                .OrderByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();

            if (result.Count < count && target.TagIds.Count > 0)
            {
                var excluded = result.Select(a => a.ArticleId).Append(articleId).ToList();
                result.AddRange(Published()
                    .Where(a => !excluded.Contains(a.ArticleId) && a.Tags.Any(t => target.TagIds.Contains(t.TagId)))
                    .OrderByDescending(a => a.PublishDate)
                    .Take(count - result.Count)
                    .ToList());
            }

            if (result.Count < count)
            {
                var excluded = result.Select(a => a.ArticleId).Append(articleId).ToList();
                result.AddRange(Published()
                    .Where(a => !excluded.Contains(a.ArticleId))
                    .OrderByDescending(a => a.ViewsCount)
                    .ThenByDescending(a => a.PublishDate)
                    .Take(count - result.Count)
                    .ToList());
            }
            return result;
        }

        public List<Article> GetByAuthor(int authorId)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
                .ToList();
        }

        public List<Article> GetByStatus(int statusId)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.StatusId == statusId)
                .OrderBy(a => a.UpdatedAt ?? a.CreatedAt)
                .ToList();
        }

        public List<Article> GetPublishedForSitemap()
        {
            return Published()
                .OrderByDescending(a => a.PublishDate)
                .Select(a => new Article
                {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Slug = a.Slug,
                    PublishDate = a.PublishDate,
                    UpdatedAt = a.UpdatedAt,
                    ImagePath = a.ImagePath,
                    Summary = a.Summary,
                    Content = string.Empty
                })
                .ToList();
        }

        public List<Article> GetWithoutSlug()
        {
            return db.Articles.Where(a => a.Slug == null).ToList();
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public int Count()
        {
            return db.Articles.Count();
        }

        public int CountByStatus(int statusId)
        {
            return db.Articles.Count(a => a.StatusId == statusId);
        }

        public int GetCommentCount(int articleId)
        {
            return db.Comments.Count(c => c.ArticleId == articleId);
        }

        public Dictionary<int, int> CountPerCategory()
        {
            return Published()
                .GroupBy(a => a.CategoryId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }
    }
}
