using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.Entities
{
    public class Categoria : BaseEntity
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres.")]
        [Display(Name = "Nombre de la categoría")]
        public string CaNombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        [Display(Name = "Descripción")]
        public string CADescripcion { get; set; } = string.Empty;


        //Navegacion
        public ICollection<Producto>? Productos { get; set; } = new List<Producto>();

    }
}
