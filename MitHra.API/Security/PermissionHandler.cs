using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MitHra.API.Data;
using MitHra.API.Security;

namespace MitHra.API.Security
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly AppDbContext _context;

        public PermissionHandler(AppDbContext context) => _context = context;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // 1. Obtener el email o ID desde el token (ClaimTypes.Name)
            var userEmail = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (userEmail == null) return;

            // 2. Buscar al usuario y sus roles/permisos en la BD de Mitra
            var hasPermission = await _context.Usuarios
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .Where(u => u.Email == userEmail)
                .SelectMany(u => u.Roles)
                .SelectMany(r => r.Permissions)
                .AnyAsync(p => p.Name == requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}