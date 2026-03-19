using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsuariosAPI.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido.")]
        [MinLength(2,   ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
        [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        // FK Proveedor
        [Required(ErrorMessage = "El proveedor es requerido.")]
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        // FK Categoría
        [Required(ErrorMessage = "La categoría es requerida.")]
        public int IdCategoria { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
