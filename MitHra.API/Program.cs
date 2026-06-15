using Microsoft.EntityFrameworkCore;
using MitHra.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Registramos nuestro puente hacia SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ACTIVACIÓN CLAVE: Le decimos a .NET que busque y use los controladores
builder.Services.AddControllers();

// Soporte para documentar la API
builder.Services.AddOpenApi();

// CONFIGURACIÓN DE CORS REFORZADA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 🚨 EL ORDEN AQUÍ ES CRÍTICO: CORS DEBE IR ARRIBA DE TODO
app.UseCors("AngularPolicy"); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 3. Mapea las rutas automáticas de los controladores
app.MapControllers();

app.Run();