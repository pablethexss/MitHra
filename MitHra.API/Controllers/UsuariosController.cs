using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MitHra.API.Data;
using MitHra.API.Models;
using System.Security.Claims;

namespace MitHra.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> Registrar([FromBody] Usuario dto)
        {
            // El 'userIdVediovis' ahora viene del Token oficial, no de una búsqueda local forzada
            var userIdVediovis = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdVediovis)) return Unauthorized();

            // Sincronización JIT: Si no existe, lo creamos
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UserIdVediovis == dto.UserIdVediovis);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    UserIdVediovis = dto.UserIdVediovis,
                    Email = dto.Email,
                    Nombre = dto.Nombre,
                    // Roles y TenantId vienen del DTO enviado por el Admin o del Token
                    Roles = dto.Roles,
                    Activo = true,
                    TenantId = dto.TenantId 
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            return Ok(usuario);
        }
    }
}