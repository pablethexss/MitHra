namespace MitHra.API.Models.Dtos
{
    // Usamos 'record' porque es inmutable y perfecto para transferir datos
    public record UsuarioRegistroDto(string Email, string Nombre, int Nivel);
}