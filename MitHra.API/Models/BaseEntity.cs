namespace MitHra.API.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; } // Mantenemos int para compatibilidad en Mitra
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false; // Implementamos Soft Delete en Mitra
    }
}