using System.ComponentModel.DataAnnotations;

namespace UsuariosAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [MinLength(2,   ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [MaxLength(200, ErrorMessage = "El correo no puede superar los 200 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida.")]
        public DateTime FechaDeNacimiento { get; set; }
    }
}
