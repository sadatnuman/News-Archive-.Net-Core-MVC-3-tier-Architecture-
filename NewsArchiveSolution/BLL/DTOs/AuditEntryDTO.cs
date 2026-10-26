namespace BLL.DTOs
{
    public class AuditEntryDTO
    {
        public long AuditLogId { get; set; }

        public int? UserId { get; set; }

        public string? UserName { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string? Details { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
