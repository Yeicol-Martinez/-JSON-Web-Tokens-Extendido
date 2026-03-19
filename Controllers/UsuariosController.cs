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
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuariosController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Usuarios.ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            return u == null ? NotFound(new { mensaje = $"Usuario {id} no encontrado." }) : Ok(u);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            if (!int.TryParse(User.FindFirst("userId")?.Value, out int userId))
                return Unauthorized();
            var u = await _context.Usuarios.FindAsync(userId);
            return u == null ? NotFound() : Ok(u);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == usuario.Correo.ToLower()))
                return BadRequest(new { mensaje = $"El correo '{usuario.Correo}' ya está en uso." });
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Usuario dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound(new { mensaje = $"Usuario {id} no encontrado." });
            if (await _context.Usuarios.AnyAsync(x => x.Correo.ToLower() == dto.Correo.ToLower() && x.Id != id))
                return BadRequest(new { mensaje = "El correo ya está en uso." });
            u.Nombre = dto.Nombre; u.Correo = dto.Correo; u.FechaDeNacimiento = dto.FechaDeNacimiento;
            await _context.SaveChangesAsync();
            return Ok(u);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound(new { mensaje = $"Usuario {id} no encontrado." });
            _context.Usuarios.Remove(u);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Usuario {id} eliminado." });
        }
    }
}
