using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Data;
using UsuariosAPI.Models;

namespace UsuariosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProveedoresController(AppDbContext context) => _context = context;

        // GET /api/proveedores
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Proveedores.ToListAsync());

        // GET /api/proveedores/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            return p == null ? NotFound(new { mensaje = $"Proveedor {id} no encontrado." }) : Ok(p);
        }

        // POST /api/proveedores
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
        }

        // PUT /api/proveedores/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Proveedor dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = $"Proveedor {id} no encontrado." });

            p.Nombre   = dto.Nombre;
            p.Contacto = dto.Contacto;
            await _context.SaveChangesAsync();
            return Ok(p);
        }

        // DELETE /api/proveedores/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = $"Proveedor {id} no encontrado." });

            bool tieneProductos = await _context.Productos.AnyAsync(x => x.IdProveedor == id);
            if (tieneProductos)
                return BadRequest(new { mensaje = "No se puede eliminar el proveedor porque tiene productos asociados." });

            _context.Proveedores.Remove(p);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Proveedor '{p.Nombre}' eliminado." });
        }
    }
}
