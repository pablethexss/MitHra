using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MitHra.API.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _tenantId;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
            // Lógica existente para el Tenant
            var tenantIdString = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
            if (Guid.TryParse(tenantIdString, out var guid))
            {
                _tenantId = guid;
            }
        }

        public Guid GetTenantId() => _tenantId;

        public string GetCurrentUserEmail()
        {
            // Buscamos el claim que identifica al usuario (normalmente ClaimTypes.Name o NameIdentifier)
            // Si no hay usuario logueado, devolvemos "System" por seguridad.
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            return email ?? "System";
        }
    }
}