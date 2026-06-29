using System.ComponentModel.DataAnnotations;

namespace MitHra.API.Models
{
    public class Usuario : BaseEntity, IEntityWithTenant 
    {
        public Guid UserIdVediovis { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public Guid TenantId { get; set; }
        
        // Relación Many-to-Many: Un usuario puede tener varios roles
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}