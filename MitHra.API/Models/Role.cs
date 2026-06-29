namespace MitHra.API.Models
{
    // Define el grupo: ej: "Vendedor" tiene varios permisos
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}