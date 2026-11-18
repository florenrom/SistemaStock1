using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuevoSistemaNat.Models.Entities
{
    public class Deposito : BaseEntity
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
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
