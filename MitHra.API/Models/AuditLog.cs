namespace MitHra.API.Models
{
    public class AuditLog : IEntityWithTenant
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Guid TenantId { get; set; } // ¡Aislado por empresa!
    }
}