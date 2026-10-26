using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal class ArticleRepo : Repository<Article>, IArticleRepository
    {
        public ArticleRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        private IQueryable<Article> Published(string? language = null)
        {
            var query = db.Articles.AsNoTracking().Where(a => a.StatusId == ArticleStatusIds.Published);
            return string.IsNullOrEmpty(language) ? query : query.Where(a => a.LanguageCode == language);
        }

        public override List<Article> Get()
        {
            return db.Articles.AsNoTracking().OrderByDescending(a => a.PublishDate).ToList();
        }

        public List<Article> GetByCategoryId(int categoryId, string? language = null)
        {
            return Published(language)
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

        public List<Article> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total, string? language = null)
        {
            var query = Published(language);
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

        public List<Article> GetByPeriod(DateTime from, DateTime toExclusive, string? language, int page, int pageSize, out int total)
        {
            var query = Published(language).Where(a => a.PublishDate >= from && a.PublishDate < toExclusive);
            total = query.Count();
            return query.OrderByDescending(a => a.PublishDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public List<(int Year, int Month, int Count)> CountByMonth(string? language = null)
        {
            return Published(language)
                .GroupBy(a => new { a.PublishDate.Year, a.PublishDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .AsEnumerable()
                .Select(x => (x.Year, x.Month, x.Count))
                .ToList();
        }

        public Dictionary<int, int> CountByDay(int year, int month, string? language = null)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);
            return Published(language)
                .Where(a => a.PublishDate >= from && a.PublishDate < to)
                .GroupBy(a => a.PublishDate.Day)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Day, x => x.Count);
        }

        public List<Article> GetOnThisDay(int month, int day, int beforeYear, int count, string? language = null)
        {
            return Published(language)
                .Where(a => a.PublishDate.Month == month && a.PublishDate.Day == day && a.PublishDate.Year < beforeYear)
                .OrderByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<Article> GetBreaking(string? language = null)
        {
            return Published(language)
                .Where(a => a.IsBreakingNews)
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        public bool HasBreakingNews(string? language = null)
        {
            return Published(language).Any(a => a.IsBreakingNews);
        }

        public List<Article> GetTrending(int count, string? language = null)
        {
            return Published(language)
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

        public List<Article> GetLatest(int count, string? language = null)
        {
            return Published(language)
                .OrderByDescending(a => a.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<Article> GetRecommendationCandidates(string language, IEnumerable<int> excludeIds, IEnumerable<int> categoryIds,
            IEnumerable<int> tagIds, IEnumerable<int> authorIds, int relatedLimit, int popularLimit)
        {
            var excluded = excludeIds.Distinct().ToList();
            var categories = categoryIds.Distinct().ToList();
            var tags = tagIds.Distinct().ToList();
            var authors = authorIds.Distinct().ToList();

            var related = Published(language)
                .Where(a => !excluded.Contains(a.ArticleId)
                    && (categories.Contains(a.CategoryId) || authors.Contains(a.AuthorId) || a.Tags.Any(t => tags.Contains(t.TagId))))
                .OrderByDescending(a => a.PublishDate)
                .Take(relatedLimit)
                .Include(a => a.Tags)
                .ToList();

            var known = related.Select(a => a.ArticleId).Concat(excluded).ToList();
            related.AddRange(Published(language)
                .Where(a => !known.Contains(a.ArticleId))
                .OrderByDescending(a => a.ViewsCount)
                .ThenByDescending(a => a.PublishDate)
                .Take(popularLimit)
                .Include(a => a.Tags)
                .ToList());
            return related;
        }

        public List<Article> GetManyWithTags(IEnumerable<int> articleIds)
        {
            var ids = articleIds.Distinct().ToList();
            return db.Articles.AsNoTracking()
                .Where(a => ids.Contains(a.ArticleId))
                .Include(a => a.Tags)
                .ToList();
        }

        public List<Article> GetByAuthor(int authorId)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
                .ToList();
        }

        public List<Article> GetPublishedByAuthor(int authorId, string? language, int page, int pageSize, out int total)
        {
            var query = Published(language).Where(a => a.AuthorId == authorId);
            total = query.Count();
            return query.OrderByDescending(a => a.PublishDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public Dictionary<int, int> CountPublishedByAuthor(string? language = null)
        {
            return Published(language)
                .GroupBy(a => a.AuthorId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }

        public int PublishDue(DateTime now)
        {
            return db.Articles
                .Where(a => a.StatusId == ArticleStatusIds.Scheduled && a.ScheduledAt != null && a.ScheduledAt <= now)
                .ExecuteUpdate(s => s
                    .SetProperty(a => a.StatusId, ArticleStatusIds.Published)
                    .SetProperty(a => a.PublishDate, a => a.ScheduledAt!.Value)
                    .SetProperty(a => a.ScheduledAt, (DateTime?)null));
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
                    LanguageCode = a.LanguageCode,
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
