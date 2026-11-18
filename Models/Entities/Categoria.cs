using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.Entities
{
    public class Categoria : BaseEntity
    {
        [Required,StringLength(80)]
        public string CaNombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string CADescripcion { get; set; } = string.Empty;


        //Navegacion
        public ICollection<Producto>? Productos { get; set; } = new List<Producto>();

    }
}
