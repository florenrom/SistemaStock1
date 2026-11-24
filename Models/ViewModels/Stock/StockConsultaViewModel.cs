namespace NuevoSistemaNat.Models.ViewModels.Stock
{
    public class StockConsultaViewModel
    {
        public int IdProducto { get; set; }
        public string? Codigo { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int StockMinimo { get; set; }
        public int StockTotal { get; set; }

        public List<StockPorDepositoViewModel> Depositos { get; set; } = new();
    }
}
