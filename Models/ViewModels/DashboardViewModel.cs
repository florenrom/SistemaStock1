using NuevoSistemaNat.Models.Enum;

namespace NuevoSistemaNat.Models.ViewModels
{
    public class DashboardViewModel
    {
        // ---- KPIs GENERALES ----
        public decimal ValorTotalInventario { get; set; }
        public int TotalProductosActivos { get; set; }
        public int TotalProveedoresActivos { get; set; }
        public int TotalClientesActivos { get; set; }
        public int TotalDepositosActivos { get; set; }
        public int AlertasBajoStock { get; set; }

        // ---- MOVIMIENTOS DE HOY ----
        public int MovimientosHoy { get; set; }
        public int EntradasHoy { get; set; }
        public int SalidasHoy { get; set; }
        public int TransferenciasHoy { get; set; }

        // Valor estimado de las salidas de hoy
        public decimal ValorVentasHoy { get; set; }

        // ---- MOVIMIENTOS DE LOS ÚLTIMOS 7 DÍAS ----
        public int EntradasUltimos7Dias { get; set; }
        public int SalidasUltimos7Dias { get; set; }
        public int TransferenciasUltimos7Dias { get; set; }

        // ---- TABLAS ----
        public List<AlertaReposicion> AlertasReposicion { get; set; } = new();

        public List<StockPorDepositoViewModel> StockPorDeposito { get; set; } = new();

        public List<MovimientoRecienteViewModel> UltimosMovimientos { get; set; } = new();

        // ---- GRÁFICOS ----
        public List<StockProducto> TopMayorStock { get; set; } = new();

        public List<StockProducto> TopMenorStock { get; set; } = new();
    }

    // ---- ALERTAS DE REPOSICIÓN ----
    public class AlertaReposicion
    {
        public int idProducto { get; set; }
        public string Producto { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal StockTotal { get; set; }
        public decimal StockMinimo { get; set; }

        public bool SinStock => StockTotal <= 0;

        public string Estado => SinStock ? "Sin stock" : "Bajo stock";
    }

    // ---- STOCK DE PRODUCTOS ----
    public class StockProducto
    {
        public string Producto { get; set; } = "";
        public decimal StockTotal { get; set; }
    }

    // ---- STOCK POR DEPÓSITO ----
    public class StockPorDepositoViewModel
    {
        public string NombreDeposito { get; set; } = "";
        public int TotalUnidades { get; set; }
    }

    // ---- MOVIMIENTOS RECIENTES ----
    public class MovimientoRecienteViewModel
    {
        public int Id { get; set; }
        public string Producto { get; set; } = "";
        public TipoMovimiento Tipo { get; set; }
        public int Cantidad { get; set; }
        public string Deposito { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}

