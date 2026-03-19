using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Data;
using UsuariosAPI.Models;
using UsuariosAPI.Models.DTOs;

namespace UsuariosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductosController(AppDbContext context) => _context = context;

        // ── CRUD ─────────────────────────────────────────────────────────────

        // GET /api/productos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Select(p => new ProductoDto
                {
                    Id        = p.Id,
                    Nombre    = p.Nombre,
                    Precio    = p.Precio,
                    Stock     = p.Stock,
                    Categoria = p.Categoria!.Nombre,
                    Proveedor = p.Proveedor!.Nombre
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET /api/productos/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id        = p.Id,
                    Nombre    = p.Nombre,
                    Precio    = p.Precio,
                    Stock     = p.Stock,
                    Categoria = p.Categoria!.Nombre,
                    Proveedor = p.Proveedor!.Nombre
                })
                .FirstOrDefaultAsync();

            return p == null ? NotFound(new { mensaje = $"Producto {id} no encontrado." }) : Ok(p);
        }

        // POST /api/productos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Producto producto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == producto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = $"La categoría con ID {producto.IdCategoria} no existe." });

            bool proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == producto.IdProveedor);
            if (!proveedorExiste)
                return BadRequest(new { mensaje = $"El proveedor con ID {producto.IdProveedor} no existe." });

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }

        // PUT /api/productos/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Producto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var p = await _context.Productos.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = $"Producto {id} no encontrado." });

            bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = $"La categoría con ID {dto.IdCategoria} no existe." });

            bool proveedorExiste = await _context.Proveedores.AnyAsync(pr => pr.Id == dto.IdProveedor);
            if (!proveedorExiste)
                return BadRequest(new { mensaje = $"El proveedor con ID {dto.IdProveedor} no existe." });

            p.Nombre      = dto.Nombre;
            p.Precio      = dto.Precio;
            p.Stock       = dto.Stock;
            p.IdCategoria = dto.IdCategoria;
            p.IdProveedor = dto.IdProveedor;

            await _context.SaveChangesAsync();
            return Ok(p);
        }

        // DELETE /api/productos/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Productos.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = $"Producto {id} no encontrado." });
            _context.Productos.Remove(p);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Producto '{p.Nombre}' eliminado." });
        }

        // ── ENDPOINTS DE AGREGACIÓN ──────────────────────────────────────────

        /// <summary>
        /// GET /api/productos/resumen
        /// Devuelve: producto más caro, más barato, suma total y precio promedio.
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .ToListAsync();

            if (!productos.Any())
                return Ok(new { mensaje = "No hay productos registrados." });

            // Expresiones Lambda para las agregaciones
            var masCaro   = productos.MaxBy(p => p.Precio);
            var masBarato = productos.MinBy(p => p.Precio);
            var suma      = productos.Sum(p => p.Precio);
            var promedio  = productos.Average(p => p.Precio);

            var resumen = new ProductoResumenDto
            {
                ProductoMasCaro = new ProductoDto
                {
                    Id        = masCaro!.Id,
                    Nombre    = masCaro.Nombre,
                    Precio    = masCaro.Precio,
                    Stock     = masCaro.Stock,
                    Categoria = masCaro.Categoria?.Nombre,
                    Proveedor = masCaro.Proveedor?.Nombre
                },
                ProductoMasBarato = new ProductoDto
                {
                    Id        = masBarato!.Id,
                    Nombre    = masBarato.Nombre,
                    Precio    = masBarato.Precio,
                    Stock     = masBarato.Stock,
                    Categoria = masBarato.Categoria?.Nombre,
                    Proveedor = masBarato.Proveedor?.Nombre
                },
                SumaTotalPrecios = suma,
                PrecioPromedio   = Math.Round(promedio, 2)
            };

            return Ok(resumen);
        }

        /// <summary>
        /// GET /api/productos/por-categoria/{idCategoria}
        /// Devuelve todos los productos de una categoría específica.
        /// </summary>
        [HttpGet("por-categoria/{idCategoria:int}")]
        public async Task<IActionResult> GetPorCategoria(int idCategoria)
        {
            bool existe = await _context.Categorias.AnyAsync(c => c.Id == idCategoria);
            if (!existe)
                return NotFound(new { mensaje = $"La categoría con ID {idCategoria} no existe." });

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.IdCategoria == idCategoria)           // Lambda filter
                .Select(p => new ProductoDto
                {
                    Id        = p.Id,
                    Nombre    = p.Nombre,
                    Precio    = p.Precio,
                    Stock     = p.Stock,
                    Categoria = p.Categoria!.Nombre,
                    Proveedor = p.Proveedor!.Nombre
                })
                .ToListAsync();

            return Ok(new
            {
                categoria  = (await _context.Categorias.FindAsync(idCategoria))!.Nombre,
                total      = productos.Count,
                productos
            });
        }

        /// <summary>
        /// GET /api/productos/por-proveedor/{idProveedor}
        /// Devuelve todos los productos de un proveedor específico.
        /// </summary>
        [HttpGet("por-proveedor/{idProveedor:int}")]
        public async Task<IActionResult> GetPorProveedor(int idProveedor)
        {
            bool existe = await _context.Proveedores.AnyAsync(p => p.Id == idProveedor);
            if (!existe)
                return NotFound(new { mensaje = $"El proveedor con ID {idProveedor} no existe." });

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.IdProveedor == idProveedor)           // Lambda filter
                .Select(p => new ProductoDto
                {
                    Id        = p.Id,
                    Nombre    = p.Nombre,
                    Precio    = p.Precio,
                    Stock     = p.Stock,
                    Categoria = p.Categoria!.Nombre,
                    Proveedor = p.Proveedor!.Nombre
                })
                .ToListAsync();

            return Ok(new
            {
                proveedor = (await _context.Proveedores.FindAsync(idProveedor))!.Nombre,
                total     = productos.Count,
                productos
            });
        }

        /// <summary>
        /// GET /api/productos/cantidad-total
        /// Devuelve la cantidad total de productos registrados.
        /// </summary>
        [HttpGet("cantidad-total")]
        public async Task<IActionResult> GetCantidadTotal()
        {
            var total = await _context.Productos.CountAsync();      // Lambda Count
            return Ok(new { cantidadTotal = total });
        }
    }
}
