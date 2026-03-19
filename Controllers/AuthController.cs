using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Data;
using UsuariosAPI.Models;
using UsuariosAPI.Models.DTOs;
using UsuariosAPI.Services;

namespace UsuariosAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext     _context;
        private readonly ITokenService    _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration   _config;

        public AuthController(AppDbContext context, ITokenService tokenService,
            IPasswordService passwordService, IConfiguration config)
        {
            _context         = context;
            _tokenService    = tokenService;
            _passwordService = passwordService;
            _config          = config;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.CuentasUsuario.AnyAsync(c => c.Username.ToLower() == dto.Username.ToLower()))
                return BadRequest(new { mensaje = $"El username '{dto.Username}' ya está en uso." });

            if (await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == dto.Correo.ToLower()))
                return BadRequest(new { mensaje = $"El correo '{dto.Correo}' ya está en uso." });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var usuario = new Usuario { Nombre = dto.Nombre, Correo = dto.Correo, FechaDeNacimiento = dto.FechaDeNacimiento };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var refreshDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
                var cuenta = new CuentaUsuario
                {
                    Username           = dto.Username,
                    PasswordHash       = _passwordService.Hash(dto.Password),
                    UsuarioId          = usuario.Id,
                    RefreshToken       = _tokenService.GenerateRefreshToken(),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays)
                };
                _context.CuentasUsuario.Add(cuenta);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var (accessToken, expiration) = _tokenService.GenerateAccessToken(cuenta);
                return Ok(new AuthResponseDto { AccessToken = accessToken, RefreshToken = cuenta.RefreshToken!, Expiration = expiration, Username = cuenta.Username, UsuarioId = usuario.Id });
            }
            catch { await tx.RollbackAsync(); throw; }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cuenta = await _context.CuentasUsuario.Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Username.ToLower() == dto.Username.ToLower());

            if (cuenta == null || !_passwordService.Verify(dto.Password, cuenta.PasswordHash))
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            var (accessToken, expiration) = _tokenService.GenerateAccessToken(cuenta);
            var refreshDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            cuenta.RefreshToken       = _tokenService.GenerateRefreshToken();
            cuenta.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto { AccessToken = accessToken, RefreshToken = cuenta.RefreshToken, Expiration = expiration, Username = cuenta.Username, UsuarioId = cuenta.UsuarioId });
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            System.Security.Claims.ClaimsPrincipal principal;
            try { principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken); }
            catch { return Unauthorized(new { mensaje = "Access token inválido." }); }

            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { mensaje = "Token sin identidad válida." });

            var cuenta = await _context.CuentasUsuario.Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Username.ToLower() == username.ToLower());

            if (cuenta == null || cuenta.RefreshToken != dto.RefreshToken || cuenta.RefreshTokenExpiry <= DateTime.UtcNow)
                return Unauthorized(new { mensaje = "Refresh token inválido o expirado." });

            var (newAccessToken, expiration) = _tokenService.GenerateAccessToken(cuenta);
            var refreshDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            cuenta.RefreshToken       = _tokenService.GenerateRefreshToken();
            cuenta.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto { AccessToken = newAccessToken, RefreshToken = cuenta.RefreshToken, Expiration = expiration, Username = cuenta.Username, UsuarioId = cuenta.UsuarioId });
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
        {
            var cuenta = await _context.CuentasUsuario.FirstOrDefaultAsync(c => c.RefreshToken == dto.RefreshToken);
            if (cuenta != null) { cuenta.RefreshToken = null; cuenta.RefreshTokenExpiry = null; await _context.SaveChangesAsync(); }
            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }
    }
}
