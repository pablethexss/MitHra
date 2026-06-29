using Microsoft.EntityFrameworkCore;
using MitHra.API.Data;
using MitHra.API.Services; // <--- AGREGA ESTA LÍNEA
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MitHra.API.Security;
var builder = WebApplication.CreateBuilder(args);
// 1. Registramos nuestro puente hacia SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// 2. ACTIVACIÓN CLAVE: Le decimos a .NET que busque y use los controladores
var vediovisUrl = builder.Configuration["VediovisSettings:BaseUrl"];
var audience = builder.Configuration["VediovisSettings:Audience"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
       options.Authority = vediovisUrl; // URL donde vive Vediovis
       // Desactiva la validación de HTTPS solo si estás en desarrollo local
        options.RequireHttpsMetadata = false;
        options.Audience = audience; // El nombre de este servicio en el IdP
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtSettings:Key"] 
                    ?? throw new InvalidOperationException("La clave JWT no está configurada en UserSecrets.")
                )
            )
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Esto imprimirá en la consola de MitHra exactamente por qué falló
                Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddHttpClient<IVediovisService, VediovisService>();
builder.Services.AddAuthorization(options =>
{
    // Ejemplo: Crear una política llamada "CanCreateProducts"
    options.AddPolicy("CanCreateProducts", policy => 
        policy.Requirements.Add(new PermissionRequirement("products.create")));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddAuthorization(options =>
{
    // Política: El nivel del usuario debe ser menor o igual a 2 (Owner o Admin)
    options.AddPolicy("RequiereAdministracion", policy => 
        policy.RequireAssertion(context =>
        {
            var nivelClaim = context.User.FindFirst("Nivel")?.Value;
            return int.TryParse(nivelClaim, out int nivel) && nivel <= 2;
        }));

    // Política: El nivel del usuario debe ser menor o igual a 3 (Vendedores pueden ver inventario)
    options.AddPolicy("RequiereOperacion", policy => 
        policy.RequireAssertion(context =>
        {
            var nivelClaim = context.User.FindFirst("Nivel")?.Value;
            return int.TryParse(nivelClaim, out int nivel) && nivel <= 3;
        }));
});
// Soporte para documentar la API
builder.Services.AddOpenApi();
// CONFIGURACIÓN DE CORS REFORZADA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMitHraFront", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Cambia esto por la URL real de tu front (Angular)
              .AllowAnyMethod()
              .AllowAnyHeader() // Indispensable para permitir X-Tenant-Id
              .AllowCredentials(); // Necesario si usas cookies o auth de sesión
    });
});
var app = builder.Build();
// 🚨 EL ORDEN AQUÍ ES CRÍTICO: CORS DEBE IR ARRIBA DE TODO
app.UseCors("AllowMitHraFront"); 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // 1. Asegurar que las migraciones estén aplicadas a la BD
        await context.Database.MigrateAsync();
        
        // 2. Ejecutar el semillado de datos de seguridad
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al intentar migrar o sembrar la base de datos.");
    }
}
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (authHeader != null)
    {
        Console.WriteLine($"[DEBUG] Header Authorization detectado: {authHeader.Substring(0, 20)}...");
    }
    else
    {
        Console.WriteLine("[DEBUG] No se detectó Header Authorization en la petición.");
    }
    await next();
});
app.UseAuthentication(); 
app.UseAuthorization();
app.UseHttpsRedirection();
// 3. Mapea las rutas automáticas de los controladores
app.MapControllers();
app.Run();