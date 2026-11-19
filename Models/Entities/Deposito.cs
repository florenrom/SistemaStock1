using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuevoSistemaNat.Models.Entities
{
    public class Deposito : BaseEntity
    {
        [Required(ErrorMessage = "El nombre del depósito es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        [Display(Name = "Nombre del depósito")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La ubicación no puede superar los 200 caracteres.")]
        [Display(Name = "Ubicación")]
        public string? Ubicacion { get; set; }

        // Stock de cada producto en este depósito
        public ICollection<StockDeposito> Stocks { get; set; } = new
       List<StockDeposito>();

        // Indicar explícitamente a qué navegación de Movimiento corresponde cada colección
        [InverseProperty("DepositoOrigen")]
        public ICollection<Movimiento> MovimientosOrigen { get; set; } = new
        List<Movimiento>();

        [InverseProperty("DepositoDestino")]
        public ICollection<Movimiento> MovimientosDestino { get; set; } = new
        List<Movimiento>();
    }
}
