using MitHra.API.Data;
using MitHra.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MitHra.API.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Si ya hay roles, asumimos que el sistema ya fue inicializado
            if (await context.Set<Role>().AnyAsync()) return;

            // 1. Crear Permisos (Granulares)
            var pCreate = new Permission { Name = "products.create", Description = "Crear productos" };
            var pEdit = new Permission { Name = "products.edit", Description = "Editar productos" };
            var pDelete = new Permission { Name = "products.delete", Description = "Eliminar productos" };

            // 2. Crear Roles
            var adminRole = new Role 
            { 
                Name = Roles.Administrador, 
                Permissions = new List<Permission> { pCreate, pEdit, pDelete } 
            };

            var vendedorRole = new Role 
            { 
                Name = Roles.Vendedor, 
                Permissions = new List<Permission> { pCreate } 
            };

            // 3. Guardar en base de datos
            await context.Set<Role>().AddRangeAsync(adminRole, vendedorRole);
            await context.SaveChangesAsync();
        }
    }
}