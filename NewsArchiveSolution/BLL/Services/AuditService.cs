using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository repo;
        private readonly IActorContext actor;

        public AuditService(IDataAccessFactory factory, IActorContext actor)
        {
            repo = factory.GetAuditLogRepository();
            this.actor = actor;
        }

        public void Record(string action, string entityType, int? entityId = null, string? details = null, int? actorId = null)
        {
            var ip = actor.IpAddress;
            repo.Add(new AuditLog
            {
                UserId = actorId ?? actor.UserId,
                Action = action.Length > 50 ? action[..50] : action,
                EntityType = entityType.Length > 30 ? entityType[..30] : entityType,
                EntityId = entityId,
                Details = AuditRules.Clip(details),
                IpAddress = ip != null && ip.Length > 45 ? ip[..45] : ip,
                CreatedAt = DateTime.Now
            });
        }

        public List<AuditEntryDTO> Get(string? entityType, int? userId, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 30;
            var type = AuditEntities.All.FirstOrDefault(t => string.Equals(t, entityType, StringComparison.OrdinalIgnoreCase));
            return repo.Get(type, userId, page, pageSize, out total)
                .Select(a => new AuditEntryDTO
                {
                    AuditLogId = a.AuditLogId,
                    UserId = a.UserId,
                    UserName = a.User?.Name,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    CreatedAt = a.CreatedAt
                })
                .ToList();
        }

        public int PurgeOld()
        {
            return repo.PurgeBefore(DateTime.Now.AddDays(-AuditRules.RetentionDays));
        }
    }
}
