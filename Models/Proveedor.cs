using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UsuariosAPI.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es requerido.")]
        [MinLength(2,   ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
        [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto del proveedor es requerido.")]
        [MaxLength(200, ErrorMessage = "El contacto no puede superar los 200 caracteres.")]
        public string Contacto { get; set; } = string.Empty;

        // Navegación
        [JsonIgnore]
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
