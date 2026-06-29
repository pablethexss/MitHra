namespace MitHra.API.Models
{
    // Define la acción granular: ej: "producto.crear", "factura.anular"
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
    }
}