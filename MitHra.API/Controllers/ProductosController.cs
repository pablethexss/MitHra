using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MitHra.API.Data;
using MitHra.API.Models;

namespace MitHra.API.Controllers
{
    // Esto le dice a .NET que este archivo responderá en la ruta: http://localhost:XXXX/api/productos
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos nuestro puente a la base de datos
        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/productos (Trae la lista completa del inventario)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            var lista = await _context.Productos.ToListAsync();
            return Ok(lista);
        }

        // 2. POST: api/productos (Guarda un producto nuevo en SQL Server)
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // Agregamos el producto al set de Entity Framework
            _context.Productos.Add(producto);
            
            // Confirmamos y guardamos físicamente en SQL Server
            await _context.SaveChangesAsync();

            // Devolvemos el producto creado con el ID que le asignó la base de datos automáticamente
            return CreatedAtAction(nameof(GetProductos), new { id = producto.Id }, producto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] Producto producto)
        {
            // 1. Validar que el ID de la URL coincida con el del objeto
            if (id != producto.Id)
            {
                return BadRequest("El ID del producto no coincide.");
            }

            // 2. Buscar si existe en la base de datos
            var productoExistente = await _context.Productos.FindAsync(id);
            if (productoExistente == null)
            {
                return NotFound();
            }

            // 3. Actualizar propiedades (puedes usar AutoMapper o hacerlo manual)
            productoExistente.Codigo = producto.Codigo;
            productoExistente.Nombre = producto.Nombre;
            productoExistente.Tipo = producto.Tipo;
            productoExistente.Stock = producto.Stock;
            productoExistente.StockMinimo = producto.StockMinimo;
            productoExistente.PrecioNeto = producto.PrecioNeto;
            productoExistente.EsAfectoIVA = producto.EsAfectoIVA;
            productoExistente.RequiereMayorEdad = producto.RequiereMayorEdad;
            productoExistente.FechaCaducidad = producto.FechaCaducidad;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al actualizar en la base de datos.");
            }

            return NoContent(); // 204 significa "Éxito, no hay nada que devolver"
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent(); // 204: Todo salió bien y no hay nada que devolver
        }
    }    
}