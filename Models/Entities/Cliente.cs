using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.Entities
{
    public class Cliente : BaseEntity
    {
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(120, ErrorMessage = "El contacto no puede superar los 120 caracteres.")]
        [Display(Name = "Contacto")]
        public string? Contacto { get; set; }

        [StringLength(30, ErrorMessage = "El documento no puede superar los 30 caracteres.")]
        [Display(Name = "Documento")]
        public string? Documento { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(300, ErrorMessage = "Las observaciones no pueden superar los 300 caracteres.")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
