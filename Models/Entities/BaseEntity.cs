using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.Entities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string CADescripcion { get; set; } = string.Empty;
        public bool CaActivo { get; set; } = true;

        public DateTime createAt { get; set; } = DateTime.UtcNow;
        public DateTime? updateAt { get; set; }

    }
}
