using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class ReadingHistoryRepo : IReadingHistoryRepository
    {
        private readonly NewsArchiveDbContext db;

        public ReadingHistoryRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        private IQueryable<ReadingHistory> ForUser(int userId)
        {
            return db.ReadingHistory.Where(h => h.UserId == userId);
        }

        private int Touch(int userId, int articleId, DateTime readAt)
        {
            return ForUser(userId)
                .Where(h => h.ArticleId == articleId)
                .ExecuteUpdate(s => s
                    .SetProperty(h => h.LastReadAt, readAt)
                    .SetProperty(h => h.ReadCount, h => h.ReadCount + 1));
        }

        public void Record(int userId, int articleId, DateTime readAt)
        {
            if (Touch(userId, articleId, readAt) > 0) return;

            db.ReadingHistory.Add(new ReadingHistory
            {
                UserId = userId,
                ArticleId = articleId,
                FirstReadAt = readAt,
                LastReadAt = readAt,
                ReadCount = 1
            });
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                Touch(userId, articleId, readAt);
            }
        }

        public void Trim(int userId, int keep)
        {
            var stale = ForUser(userId)
                .OrderByDescending(h => h.LastReadAt)
                .Skip(keep)
                .Select(h => h.ArticleId)
                .ToList();
            if (stale.Count > 0)
                ForUser(userId).Where(h => stale.Contains(h.ArticleId)).ExecuteDelete();
        }

        public List<int> GetRecentArticleIds(int userId, int count)
        {
            return ForUser(userId).AsNoTracking()
                .OrderByDescending(h => h.LastReadAt)
                .Take(count)
                .Select(h => h.ArticleId)
                .ToList();
        }

        public List<ReadingHistory> GetEntries(int userId, int page, int pageSize, out int total)
        {
            var query = ForUser(userId).AsNoTracking()
                .Where(h => h.Article.StatusId == ArticleStatusIds.Published);
            total = query.Count();
            return query.OrderByDescending(h => h.LastReadAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Include(h => h.Article)
                        .ToList();
        }

        public bool Remove(int userId, int articleId)
        {
            return ForUser(userId).Where(h => h.ArticleId == articleId).ExecuteDelete() > 0;
        }

        public int Clear(int userId)
        {
            return ForUser(userId).ExecuteDelete();
        }

        public int Count(int userId)
        {
            return ForUser(userId).Count();
        }
    }
}
