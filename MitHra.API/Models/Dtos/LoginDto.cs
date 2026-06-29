namespace MitHra.API.Models.Dtos
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Es importante incluir el Tenant al que intenta acceder para que 
        // Vediovis valide si tiene permiso para entrar a Mitra desde esa organización
        public string TenantIdentifier { get; set; } = string.Empty; 
        public string? Audience { get; set; }
    }
}