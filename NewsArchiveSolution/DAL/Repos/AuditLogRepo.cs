using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class AuditLogRepo : IAuditLogRepository
    {
        private readonly NewsArchiveDbContext db;

        public AuditLogRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        public void Add(AuditLog entry)
        {
            db.AuditLogs.Add(entry);
            db.SaveChanges();
            db.Entry(entry).State = EntityState.Detached;
        }

        public List<AuditLog> Get(string? entityType, int? userId, int page, int pageSize, out int total)
        {
            var query = db.AuditLogs.AsNoTracking();
            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);
            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            total = query.Count();
            return query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.AuditLogId)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Include(a => a.User)
                        .ToList();
        }

        public int PurgeBefore(DateTime cutoff)
        {
            return db.AuditLogs.Where(a => a.CreatedAt < cutoff).ExecuteDelete();
        }
    }
}
