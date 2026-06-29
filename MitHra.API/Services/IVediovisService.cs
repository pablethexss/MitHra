using System.Text.Json.Serialization;
using MitHra.API.Models.Dtos;
namespace MitHra.API.Services
{
    public interface IVediovisService
    {
        // Valida contra el IdP y devuelve el objeto con el Token y los datos del Tenant
        Task<VediovisAuthResponse?> ValidarUsuario(LoginDto loginDto);
    }

    public class VediovisAuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("tenantType")]
        public string TenantType { get; set; } = "Basica";

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new();
}
}