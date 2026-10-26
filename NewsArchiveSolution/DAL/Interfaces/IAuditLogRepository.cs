using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IAuditLogRepository
    {
        void Add(AuditLog entry);

        List<AuditLog> Get(string? entityType, int? userId, int page, int pageSize, out int total);

        int PurgeBefore(DateTime cutoff);
    }
}
