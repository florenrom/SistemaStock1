using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.ViewModels
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre de la Categoría")]
        public string CaNombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripcion no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string CADescripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;


        [Display(Name = "Fecha de Creación")]
        public DateTime createAt { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Actualización")]
        public DateTime? updateAt { get; set; }

        [Display(Name = "Cantidad de Productos")]
        public int cantidadProductos { get; set; } = 0;

    }
}
