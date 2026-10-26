using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class CommentRepo : Repository<Comment>, ICommentRepository
    {
        public CommentRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public override Comment? Get(int id)
        {
            return db.Comments.AsNoTracking().FirstOrDefault(c => c.CommentId == id);
        }

        public List<Comment> GetByArticleId(int articleId, bool includeHidden = false)
        {
            return db.Comments.AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.ArticleId == articleId && (includeHidden || !c.IsHidden))
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }

        public int CountVisible(int articleId)
        {
            return db.Comments.Count(c => c.ArticleId == articleId && !c.IsHidden);
        }

        public List<ReportReason> GetReasons()
        {
            return db.ReportReasons.AsNoTracking().OrderBy(r => r.ReasonId).ToList();
        }

        public bool AddReport(int commentId, int userId, int reasonId, string? note, DateTime createdAt)
        {
            if (db.CommentReports.Any(r => r.CommentId == commentId && r.UserId == userId)) return false;
            db.CommentReports.Add(new CommentReport { CommentId = commentId, UserId = userId, ReasonId = reasonId, Note = note, CreatedAt = createdAt });
            try
            {
                return db.SaveChanges() > 0;
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                return false;
            }
        }

        public int CountReports(int commentId)
        {
            return db.CommentReports.Count(r => r.CommentId == commentId);
        }

        public HashSet<int> GetReportedIds(int userId, IEnumerable<int> commentIds)
        {
            var ids = commentIds.Distinct().ToList();
            return db.CommentReports.AsNoTracking()
                .Where(r => r.UserId == userId && ids.Contains(r.CommentId))
                .Select(r => r.CommentId)
                .ToHashSet();
        }

        public bool SetHidden(int commentId, bool hidden, int? moderatorId, DateTime? moderatedAt)
        {
            return db.Comments
                .Where(c => c.CommentId == commentId)
                .ExecuteUpdate(s => s
                    .SetProperty(c => c.IsHidden, hidden)
                    .SetProperty(c => c.ModeratedById, moderatorId)
                    .SetProperty(c => c.ModeratedAt, moderatedAt)) > 0;
        }

        public int ClearReports(int commentId)
        {
            return db.CommentReports.Where(r => r.CommentId == commentId).ExecuteDelete();
        }

        private static IQueryable<Comment> WithDetails(IQueryable<Comment> query)
        {
            return query
                .Include(c => c.User)
                .Include(c => c.Article)
                .Include(c => c.ModeratedBy)
                .Include(c => c.Reports).ThenInclude(r => r.Reason)
                .Include(c => c.Reports).ThenInclude(r => r.User)
                .AsSplitQuery();
        }

        public List<Comment> GetReported(int page, int pageSize, out int total)
        {
            var query = db.Comments.AsNoTracking().Where(c => c.Reports.Any());
            total = query.Count();
            return WithDetails(query
                    .OrderByDescending(c => c.Reports.Count())
                    .ThenByDescending(c => c.Reports.Max(r => r.CreatedAt))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .ToList();
        }

        public List<Comment> GetHidden(int page, int pageSize, out int total)
        {
            var query = db.Comments.AsNoTracking().Where(c => c.IsHidden);
            total = query.Count();
            return WithDetails(query
                    .OrderByDescending(c => c.ModeratedAt ?? c.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .ToList();
        }

        public int CountReported()
        {
            return db.Comments.Count(c => c.Reports.Any());
        }
    }
}
