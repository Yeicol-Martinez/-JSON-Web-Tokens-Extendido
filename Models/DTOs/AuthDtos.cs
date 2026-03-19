using System.ComponentModel.DataAnnotations;

namespace UsuariosAPI.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [MinLength(4)][MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6)][MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required][MinLength(4)][MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required][MinLength(6)][MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required][MinLength(2)][MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required][EmailAddress][MaxLength(200)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public DateTime FechaDeNacimiento { get; set; }
    }

    public class RefreshRequestDto
    {
        [Required] public string AccessToken  { get; set; } = string.Empty;
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string   AccessToken  { get; set; } = string.Empty;
        public string   RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration   { get; set; }
        public string   Username     { get; set; } = string.Empty;
        public int      UsuarioId    { get; set; }
    }

    // ── Estadísticas de productos ────────────────────────────────────────────

    public class ProductoResumenDto
    {
        public ProductoDto? ProductoMasCaro    { get; set; }
        public ProductoDto? ProductoMasBarato  { get; set; }
        public decimal      SumaTotalPrecios   { get; set; }
        public decimal      PrecioPromedio     { get; set; }
    }

    public class ProductoDto
    {
        public int     Id          { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public decimal Precio      { get; set; }
        public int     Stock       { get; set; }
        public string? Categoria   { get; set; }
        public string? Proveedor   { get; set; }
    }
}
