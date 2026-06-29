using Microsoft.EntityFrameworkCore;
using MitHra.API.Models;
using System.Linq.Expressions;
using MitHra.API.Services;
using System.Text.Json;

namespace MitHra.API.Data
{
    public class AppDbContext : DbContext
    {
        // Necesitaremos el ITenantService para obtener el ID de la empresa actual
        private readonly ITenantService _tenantService;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) 
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Producto> Productos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de precisión para productos
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioNeto)
                .HasColumnType("decimal(18,2)");

            // 2. Aplicar filtro global a todas las entidades que implementan IEntityWithTenant
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IEntityWithTenant).IsAssignableFrom(entityType.ClrType))
                {
                    // Esto asegura que cada entidad tenga el campo TenantId
                    modelBuilder.Entity(entityType.ClrType).Property<Guid>("TenantId");

                    // Crear la expresión lambda: e => e.TenantId == _tenantService.GetTenantId()
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, "TenantId");
                    var constant = Expression.Constant(_tenantService.GetTenantId());
                    var body = Expression.Equal(property, constant);

                    var filter = Expression.Lambda(body, parameter);
                    entityType.SetQueryFilter(filter);
                }
            }
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Roles)
                .WithMany();

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany();
        }

        // 3. Este método se encarga de asignar el TenantId automáticamente al guardar
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();
            var tenantId = _tenantService.GetTenantId();
            var currentUser = _tenantService.GetCurrentUserEmail(); // Asegúrate de tener este método
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                // 1. Lógica de BaseEntity (Auditoría automática de campos y Soft Delete)
                if (entry.Entity is BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedAt = now;
                            baseEntity.CreatedBy = currentUser;
                            break;
                        case EntityState.Modified:
                            baseEntity.UpdatedAt = now;
                            baseEntity.UpdatedBy = currentUser;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified; // Convertir en actualización para Soft Delete
                            baseEntity.IsDeleted = true;
                            baseEntity.UpdatedAt = now;
                            baseEntity.UpdatedBy = currentUser;
                            break;
                    }
                }

                // 2. Asignar TenantId
                if (entry.Entity is IEntityWithTenant tenantEntity && entry.State == EntityState.Added)
                {
                    tenantEntity.TenantId = tenantId;
                }

                // 3. Preparar logs de auditoría (omitiendo la propia tabla de auditoría)
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var audit = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    ChangedAt = now,
                    TenantId = tenantId
                };

                if (entry.State == EntityState.Modified)
                {
                    var changes = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => new { Old = p.OriginalValue, New = p.CurrentValue });

                    audit.OldValues = JsonSerializer.Serialize(changes.ToDictionary(k => k.Key, v => v.Value.Old));
                    audit.NewValues = JsonSerializer.Serialize(changes.ToDictionary(k => k.Key, v => v.Value.New));
                }
                else if (entry.State == EntityState.Added)
                {
                    audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));
                }

                auditEntries.Add(audit);
            }

            // 4. Guardar cambios principales
            var result = await base.SaveChangesAsync(cancellationToken);

            // 5. Guardar logs de auditoría si existen
            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}