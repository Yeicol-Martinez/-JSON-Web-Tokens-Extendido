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
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriasController(AppDbContext context) => _context = context;

        // GET /api/categorias
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Categorias.ToListAsync());

        // GET /api/categorias/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.Categorias.FindAsync(id);
            return c == null ? NotFound(new { mensaje = $"Categoría {id} no encontrada." }) : Ok(c);
        }

        // POST /api/categorias
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Categoria categoria)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                return BadRequest(new { mensaje = $"La categoría '{categoria.Nombre}' ya existe." });

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
        }

        // PUT /api/categorias/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Categoria dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var c = await _context.Categorias.FindAsync(id);
            if (c == null) return NotFound(new { mensaje = $"Categoría {id} no encontrada." });

            if (await _context.Categorias.AnyAsync(x => x.Nombre.ToLower() == dto.Nombre.ToLower() && x.Id != id))
                return BadRequest(new { mensaje = $"Ya existe una categoría con el nombre '{dto.Nombre}'." });

            c.Nombre = dto.Nombre;
            await _context.SaveChangesAsync();
            return Ok(c);
        }

        // DELETE /api/categorias/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Categorias.FindAsync(id);
            if (c == null) return NotFound(new { mensaje = $"Categoría {id} no encontrada." });

            bool tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
                return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });

            _context.Categorias.Remove(c);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Categoría '{c.Nombre}' eliminada." });
        }
    }
}
