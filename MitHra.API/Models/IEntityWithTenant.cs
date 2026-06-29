namespace MitHra.API.Models
{
    public interface IEntityWithTenant
    {
        Guid TenantId { get; set; }
    }
}