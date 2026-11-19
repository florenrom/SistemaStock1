using NuevoSistemaNat.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuevoSistemaNat.Models.Entities
{
    public class Producto : BaseEntity
    {

        [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
        [Display(Name = "Código")]
        public string? PrCodigo { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        [Display(Name = "Nombre del Producto")]
        public string PrNombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        [Display(Name = "Descripción")]
        public string PrDescripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una unidad de medida.")]
        [Display(Name = "Unidad de Medida")]
        public UnidadMedida UnidadDeMedida { get; set; }

        [Required(ErrorMessage = "El precio de costo es obligatorio.")]
        [Range(typeof(decimal), "0.01", "10000000", ErrorMessage = "El precio de costo debe estar entre 0,01 y 10.000.000.")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio de Costo")]
        public decimal PrPrecioCosto { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio.")]
        [Range(typeof(decimal), "0.01", "10000000", ErrorMessage = "El precio de venta debe estar entre 0,01 y 10.000.000.")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio de Venta")]
        public decimal PrPrecioVenta { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock Mínimo")]
        public int StockMinimo { get; set; } = 5;

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad en stock no puede ser negativa.")]
        [Display(Name = "Cantidad en Stock")]
        public int CantidadStock { get; set; } = 0;

        [Display(Name = "Producto Activo")]
        public bool PrActivo { get; set; } = true;

        // Relaciones
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

        public ICollection<StockDeposito> Stocks { get; set; } = new List<StockDeposito>();

        // Propiedad calculada: Stock total en todos los depósitos
        [NotMapped]
        [Display(Name = "Stock Total")]
        public int StockTotal => Stocks.Sum(s => s.Cantidad);

        // Propiedad calculada: indica si está por debajo del stock mínimo
        [NotMapped]
        [Display(Name = "Bajo Stock Mínimo")]
        public bool BajoStockMinimo => StockTotal < StockMinimo;

        // Clave foránea
        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [ForeignKey("Categoria")]
        [Display(Name = "Categoría")]
        public int CaId { get; set; }

        public virtual Categoria? Categoria { get; set; }

    }
}
