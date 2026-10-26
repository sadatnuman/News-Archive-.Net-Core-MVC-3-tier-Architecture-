using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class BookmarkRepo : IBookmarkRepository
    {
        private readonly NewsArchiveDbContext db;

        public BookmarkRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        public bool Add(int userId, int articleId)
        {
            if (Exists(userId, articleId)) return true;
            db.Bookmarks.Add(new Bookmark { UserId = userId, ArticleId = articleId, CreatedAt = DateTime.Now });
            try
            {
                return db.SaveChanges() > 0;
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                return Exists(userId, articleId);
            }
        }

        public bool Remove(int userId, int articleId)
        {
            return db.Bookmarks.Where(b => b.UserId == userId && b.ArticleId == articleId).ExecuteDelete() > 0;
        }

        public bool Exists(int userId, int articleId)
        {
            return db.Bookmarks.Any(b => b.UserId == userId && b.ArticleId == articleId);
        }

        public int Count(int userId)
        {
            return db.Bookmarks.Count(b => b.UserId == userId);
        }

        public HashSet<int> GetArticleIds(int userId, IEnumerable<int> articleIds)
        {
            var ids = articleIds.Distinct().ToList();
            return db.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId && ids.Contains(b.ArticleId))
                .Select(b => b.ArticleId)
                .ToHashSet();
        }

        public List<Article> GetArticles(int userId, int page, int pageSize, out int total)
        {
            var query = db.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId && b.Article.StatusId == ArticleStatusIds.Published);
            total = query.Count();
            return query.OrderByDescending(b => b.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(b => b.Article)
                        .ToList();
        }
    }
}
