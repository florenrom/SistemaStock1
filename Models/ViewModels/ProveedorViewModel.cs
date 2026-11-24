using System.ComponentModel.DataAnnotations;

namespace NuevoSistemaNat.Models.ViewModels
{
    public class ProveedorViewModel
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(120,ErrorMessage = "Máximo 120 caracteres")]
        [Display(Name = "Nombre / Razón Social")]
        public string Nombre { get; set; } = string.Empty;

        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El documento debe contener entre 7 y 8 números.")]
        public string? Documento { get; set; }

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [EmailAddress, StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string? Email { get; set; }

        [Display(Name = "Teléfono")]
        [Phone, StringLength(30, ErrorMessage = "Máximo 30 caracteres")]
        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;
    }
}

