using System.Net.Http.Json;
using System.Text.Json;
using MitHra.API.Models.Dtos;

namespace MitHra.API.Services
{
    public class VediovisService : IVediovisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public VediovisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<VediovisAuthResponse?> ValidarUsuario(LoginDto loginDto)
        {
            var url = $"{_config["VediovisSettings:BaseUrl"]}/api/auth/login";
            
            // 1. Creamos el mensaje de solicitud
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            
            // 2. Serializamos el cuerpo manualmente
            request.Content = JsonContent.Create(loginDto);
            
            // 3. ¡AQUÍ ESTÁ LA CLAVE! Añadimos el header manualmente
            // Usamos el TenantIdentifier que viene en el DTO
            request.Headers.Add("X-Tenant-Id", loginDto.TenantIdentifier);

            // 4. Enviamos la solicitud construida
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                // Opcional: loguear el error de la respuesta para saber qué pasó exactamente
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error de Vediovis: {error}");
                return null;
            }
            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] JSON recibido de Vediovis: {jsonString}");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var authResponse = JsonSerializer.Deserialize<VediovisAuthResponse>(jsonString, options);
            return await response.Content.ReadFromJsonAsync<VediovisAuthResponse>();
        }
    }
}