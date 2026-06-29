using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MitHra.API.Data;
using MitHra.API.Models;
using MitHra.API.Models.Dtos;
using MitHra.API.Services;
using System.IdentityModel.Tokens.Jwt; // Necesario para decodificar
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MitHra.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IVediovisService _vediovisService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config; // 1. Declarar la variable

        public AuthController(IVediovisService vediovisService, AppDbContext context, IConfiguration config)
        {
            _vediovisService = vediovisService;
            _context = context;
            _config = config;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _vediovisService.ValidarUsuario(loginDto);
            
            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token)) 
                return Unauthorized("Credenciales inválidas.");

            // 1. Decodificar y validar el token para extraer los Claims
            var key = _config["JwtSettings:Key"] ?? throw new InvalidOperationException("La clave JWT no está configurada.");
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // Importante para que no expire antes de tiempo
            };

            var principal = tokenHandler.ValidateToken(authResponse.Token, validationParameters, out _);
            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"[DEBUG] Claim encontrado: {claim.Type} = {claim.Value}");
            }
            // 2. Extraer los Claims (NameId es el UserId estándar en JWT)
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            var tenantIdString = principal.FindFirst("TenantId")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value 
             ?? principal.FindFirst(ClaimTypes.Name)?.Value;
            var rol = principal.FindFirst(ClaimTypes.Role)?.Value;
            var UserNameT = principal.FindFirst("NameUser")?.Value;

            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(tenantIdString))
                return BadRequest("El token no contiene la información necesaria (UserId o TenantId).");

            if (!Guid.TryParse(userIdString, out Guid userIdGuid) || !Guid.TryParse(tenantIdString, out Guid tenantIdGuid))
                return BadRequest("Formato de ID inválido en los claims del token.");

            // 3. Sincronización JIT (Just-In-Time) usando los datos extraídos del Token
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UserIdVediovis == userIdGuid);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    UserIdVediovis = userIdGuid,
                    TenantId = tenantIdGuid,
                    Nombre = UserNameT ?? string.Empty,
                    Activo = true,
                    Email = email ?? string.Empty,
                };
                var rolesEnToken = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                foreach (var roleName in rolesEnToken)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                    if (role != null)
                    {
                        usuario.Roles.Add(role);
                    }
                }   
                _context.Usuarios.Add(usuario);
            }
            else
            {
                usuario.TenantId = tenantIdGuid;
            }

            await _context.SaveChangesAsync();
            
            return Ok(new { token = authResponse.Token });
        }
    }
}