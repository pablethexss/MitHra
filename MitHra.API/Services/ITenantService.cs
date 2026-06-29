namespace MitHra.API.Services
{
    public interface ITenantService
    {
        Guid GetTenantId();
        string GetCurrentUserEmail(); // Nuevo método para la auditoría
    }
}