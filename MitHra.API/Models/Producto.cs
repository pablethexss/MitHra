using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MitHra.API.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key] // <--- Marca explícitamente la llave primaria
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // <--- Le dice a EF que SQL Server maneja el auto-incremento
        public int Id { get; set; }
        public string? CodigoBarras { get; set; }

        // 🔍 Código de barras, SKU o código técnico
        public string Codigo { get; set; } = string.Empty;

        // 🏷️ Nombre comercial, insumo o servicio
        public string Nombre { get; set; } = string.Empty;

        // ⚙️ TIPO DE ARTÍCULO: "Fisico" o "Servicio"
        public string Tipo { get; set; } = "Fisico";

        // 📊 CONTROL DE INVENTARIO (Se mantienen en 0 si es "Servicio")
        public int Stock { get; set; } = 0;         // Tu campo original (Stock Actual)
        public int StockMinimo { get; set; } = 0;   // Para gatillar las alertas de desabastecimiento

        // 🍏 GESTIÓN DE PERECIBLES
        public DateTime? FechaCaducidad { get; set; } // Opcional (null para herramientas o servicios)

        // 🔞 RESTRICCIÓN DE LEY (Chile)
        public bool RequiereMayorEdad { get; set; } = false; // True para alcohol/tabaco

        // 💰 FINANZAS Y TRIBUTACIÓN (Tu lógica impecable del SII)
        public decimal PrecioNeto { get; set; }
        public bool EsAfectoIVA { get; set; } = true;

        // Propiedad inteligente (solo lectura) para el precio final al público ($CLP)
        public decimal PrecioVentaPublico
        {
            get
            {
                if (EsAfectoIVA)
                {
                    return Math.Round(PrecioNeto * 1.19m, 0);
                }
                return Math.Round(PrecioNeto, 0);
            }
        }
    }
}
