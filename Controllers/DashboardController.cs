using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.Enum;
using NuevoSistemaNat.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace NuevoSistemaNat.Controllers
{
    [Authorize(Roles = "Administrador,Encargado,Operador,Consulta")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Consulta"))
            {
                return RedirectToAction("Index", "Stock");
            }

            var model = new DashboardViewModel();

            // =====================================
            // 1. KPIs GENERALES
            // =====================================

            model.TotalProductosActivos = await _context.Productos.CountAsync(p => p.PrActivo);

            model.TotalProveedoresActivos =await _context.Proveedores.CountAsync(p => p.CaActivo);

            model.TotalClientesActivos = await _context.Clientes.CountAsync(c => c.CaActivo);

            model.TotalDepositosActivos = await _context.Depositos.CountAsync(d => d.CaActivo);

            // Valor total del inventario según el precio de costo
            model.ValorTotalInventario = await _context.StockDepositos.SumAsync(s =>
                  (decimal?)(s.Producto != null ? s.Cantidad * s.Producto.PrPrecioCosto : 0m))
                ?? 0m;

            // =====================================
            // 2. MOVIMIENTOS DE HOY
            // =====================================

            DateTime hoy = DateTime.Today;
            DateTime manana = hoy.AddDays(1);

            var movimientosHoy = _context.Movimientos
                .Where(m => m.Fecha >= hoy && m.Fecha < manana);

            model.MovimientosHoy = await movimientosHoy.CountAsync();

            model.EntradasHoy = await movimientosHoy.CountAsync(m => m.Tipo == TipoMovimiento.Entrada);

            model.SalidasHoy = await movimientosHoy.CountAsync(m => m.Tipo == TipoMovimiento.Salida);

            // Se cuenta solamente TransferenciaSalida
            // para no contar dos veces cada transferencia.
            model.TransferenciasHoy =await movimientosHoy.CountAsync(
                  m => m.Tipo == TipoMovimiento.TransferenciaSalida);

            // Valor estimado de las salidas de hoy
            model.ValorVentasHoy = await movimientosHoy
                .Where(m => m.Tipo == TipoMovimiento.Salida)
                .SumAsync(m =>
                    (decimal?)(m.Producto != null
                        ? m.Cantidad * m.Producto.PrPrecioVenta
                        : 0m))
                ?? 0m;

            // =====================================
            // 3. MOVIMIENTOS DE LOS ÚLTIMOS 7 DÍAS
            // =====================================

            DateTime haceSeisDias = hoy.AddDays(-6);

            var movimientosSemana = _context.Movimientos
                .Where(m =>
                    m.Fecha >= haceSeisDias &&
                    m.Fecha < manana);

            model.EntradasUltimos7Dias = await movimientosSemana.CountAsync(m => m.Tipo == TipoMovimiento.Entrada);

            model.SalidasUltimos7Dias = await movimientosSemana.CountAsync(m => m.Tipo == TipoMovimiento.Salida);

            model.TransferenciasUltimos7Dias = await movimientosSemana.CountAsync(m => m.Tipo == TipoMovimiento.TransferenciaSalida);

            // =====================================
            // 4. STOCK TOTAL POR PRODUCTO
            // =====================================

            var stockProductos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.PrActivo)
                .Select(p => new
                {
                    p.Id,
                    Producto = p.PrNombre,

                    Categoria = p.Categoria != null
                        ? p.Categoria.CaNombre
                        : "Sin categoría",

                    p.StockMinimo,

                    StockTotal = _context.StockDepositos
                        .Where(s => s.idProducto == p.Id)
                        .Sum(s => (int?)s.Cantidad) ?? 0
                })
                .ToListAsync();

            // =====================================
            // 5. ALERTAS DE REPOSICIÓN
            // =====================================

            // var stockProductos = await _context.Productos

            var alertas = stockProductos
                .Where(p => p.StockTotal <= p.StockMinimo)
                .OrderBy(p => p.StockTotal)
                .Select(p => new AlertaReposicion
                {
                    idProducto = p.Id,
                    Producto = p.Producto,
                    Categoria = p.Categoria,
                    StockTotal = p.StockTotal,
                    StockMinimo = p.StockMinimo
                })
                .ToList();

            model.AlertasReposicion = alertas
                .Take(15)
                .ToList();

            model.AlertasBajoStock = alertas.Count;

            // =====================================
            // 6. GRÁFICOS DE STOCK
            // =====================================

            model.TopMayorStock = stockProductos
                .OrderByDescending(p => p.StockTotal)
                .Take(5)
                .Select(p => new StockProducto
                {
                    Producto = p.Producto,
                    StockTotal = p.StockTotal
                })
                .ToList();

            model.TopMenorStock = stockProductos
                .OrderBy(p => p.StockTotal)
                .Take(5)
                .Select(p => new StockProducto
                {
                    Producto = p.Producto,
                    StockTotal = p.StockTotal
                })
                .ToList();

            // =====================================
            // 7. STOCK POR DEPÓSITO
            // =====================================

            model.StockPorDeposito = await _context.Depositos
                .AsNoTracking()
                .Where(d => d.CaActivo)
                .Select(d => new StockPorDepositoViewModel
                {
                    NombreDeposito = d.Nombre,

                    TotalUnidades = _context.StockDepositos
                        .Where(s => s.idDeposito == d.Id)
                        .Sum(s => (int?)s.Cantidad) ?? 0
                })
                .ToListAsync();

            // =====================================
            // 8. ÚLTIMOS MOVIMIENTOS
            // =====================================

            //model.ValorVentasHoy

            model.UltimosMovimientos = await _context.Movimientos
                .AsNoTracking()
                .OrderByDescending(m => m.Fecha)
                .Take(10)
                .Select(m => new MovimientoRecienteViewModel
                {
                    Id = m.Id,

                    Producto = m.Producto != null
                        ? m.Producto.PrNombre
                        : "Producto no disponible",

                    Tipo = m.Tipo,
                    Cantidad = m.Cantidad,
                    Fecha = m.Fecha,

                    Deposito =
                        m.Tipo == TipoMovimiento.TransferenciaSalida ||
                        m.Tipo == TipoMovimiento.TransferenciaEntrada
                            ? (m.DepositoOrigen != null
                                ? m.DepositoOrigen.Nombre
                                : "Sin origen")
                              + " → " +
                              (m.DepositoDestino != null
                                ? m.DepositoDestino.Nombre
                                : "Sin destino")
                            : (m.DepositoOrigen != null
                                ? m.DepositoOrigen.Nombre
                                : "Sin depósito")
                })
                .ToListAsync();

            return View(model);
        }
    }
}
