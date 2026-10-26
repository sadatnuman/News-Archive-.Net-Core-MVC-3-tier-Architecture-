using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuditService
    {
        void Record(string action, string entityType, int? entityId = null, string? details = null, int? actorId = null);

        List<AuditEntryDTO> Get(string? entityType, int? userId, int page, int pageSize, out int total);

        int PurgeOld();
    }
}
