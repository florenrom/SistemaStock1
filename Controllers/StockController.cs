using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.ViewModels.Stock;
using Microsoft.AspNetCore.Authorization;

namespace NuevoSistemaNat.Controllers
{
    [Authorize(Roles = "Administrador,Encargado,Operador,Consulta")]
    public class StockController : Controller
    {
        private readonly AppDbContext _context;

        public StockController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.PrActivo)
                .Include(p => p.Categoria)
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Deposito)
                .OrderBy(p => p.PrNombre)
                .ToListAsync();

            var modelo = productos.Select(p => new StockConsultaViewModel
            {
                IdProducto = p.Id,
                Codigo = p.PrCodigo,
                Producto = p.PrNombre,
                Categoria = p.Categoria != null
                    ? p.Categoria.CaNombre
                    : "Sin categoría",
                StockMinimo = p.StockMinimo,

                StockTotal = p.Stocks
                    .Where(s => s.Deposito != null && s.Deposito.CaActivo)
                    .Sum(s => s.Cantidad),

                Depositos = p.Stocks
                    .Where(s => s.Deposito != null
                        && s.Deposito.CaActivo
                        && s.Cantidad != 0)
                    .OrderBy(s => s.Deposito!.Nombre)
                    .Select(s => new StockPorDepositoViewModel
                    {
                        Deposito = s.Deposito!.Nombre,
                        Cantidad = s.Cantidad
                    })
                    .ToList()
            }).ToList();

            return View(modelo);
        }
    }
}
