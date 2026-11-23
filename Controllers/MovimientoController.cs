using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.Enum;
using NuevoSistemaNat.Models.ViewModels;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;


[Authorize(Roles = "Administrador,Encargado,Operador")]
public class MovimientoController : Controller
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MovimientoController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: MOVIMIENTOS
    public async Task<IActionResult> Index(int? idProducto, TipoMovimiento? tipo)    
    {
        var query = _context.Movimientos
            .Include(m => m.Producto)
            .Include(m => m.DepositoOrigen)
            .Include(m => m.DepositoDestino)
            .Include(m => m.Proveedor)
            .Include(m => m.Cliente)
            .AsQueryable();
        if (idProducto.HasValue) query = query.Where(m => m.idProducto ==idProducto);
    
        if (tipo.HasValue) query = query.Where(m => m.Tipo == tipo);
        var productos = await _context.Productos
            .AsNoTracking()
            .OrderBy(p => p.PrNombre)
            .ToListAsync();
        ViewBag.Productos = new SelectList(productos, "Id", "PrNombre");

        var movimientos = await query.OrderByDescending(m => m.Fecha).Take(200).ToListAsync();

        var model = _mapper.Map<List<MovimientoViewModel>>(movimientos);
        return View(model);
    }
    #region Entrada
    //----------------Registrar Entradas------------------------------
    //GET : MOVIMIENTOS/Entrada
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Entrada()
    {
        var modelo = new OperacionStockEntradaViewModel
        {
            Productos = await LlenarProductos(),
            Depositos = await LlenarDepositos(),
            Proveedores = await LlenarProveedores(),
        };
        return View(modelo);
    }

    //post: MovimientoController/Entrada
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Entrada(OperacionStockEntradaViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Iniciamos la transacción (AjustarStock y Movimiento)
            using var transaction = await
           _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Modifica las entidades en memoria
                await AjustarStock(model.idProducto, model.idDepositoOrigen,
               model.Cantidad);
                // 2. Crea el registro del movimiento
                var movimiento = new Movimiento
                {
                    Tipo = TipoMovimiento.Entrada,
                    idProducto = model.idProducto!.Value,
                    idDepositoOrigen = model.idDepositoOrigen!.Value,
                    Cantidad = model.Cantidad,
                    idProveedor = model.idProveedor,
                    Observaciones = model.Observaciones,
                    Fecha = DateTime.Now,
                    UsuarioRegistro = User.Identity?.Name ?? "Sistema"
                };
                _context.Movimientos.Add(movimiento);

                // 3. Envía TODO junto a la base de datos (Stock + Movimiento)
                await _context.SaveChangesAsync();
                // 4. Si llegó acá sin errores, confirma los cambios de forma permanente
                 await transaction.CommitAsync();
                TempData["OK"] = "Entrada de stock registrada correctamente.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException)
            {
                // No hace falta hacer Rollback explícito; al salir del 'using var transaction', se deshace solo.
            TempData["Error"] = "Ocurrió un error al registrar la entrada de stock.";
            }
            
        }
        // Si el modelo no es válido, volver a llenar los select lists y retornar la vista
        model.Productos = await LlenarProductos();
        model.Depositos = await LlenarDepositos();
        model.Proveedores = await LlenarProveedores();
        return View(model);
    }
    #endregion Entrada
    #region Salida
    //----------------Registrar Salidas------------------------------
    //GET : MOVIMIENTOS/Salida
    public async Task<IActionResult> Salida()
    {
        var modelo = new OperacionStockSalidaViewModel
        {
            Productos = await LlenarProductos(),
            Depositos = await LlenarDepositos(),
            Clientes = await LlenarClientes(),
        };
        return View(modelo);
    }

    //post: MovimientoController/Salida
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salida(OperacionStockSalidaViewModel model)
    {
        if (ModelState.IsValid)
        {
            var stockActual = await ObtenerStock(model.idProducto,
           model.idDepositoOrigen);
            if (stockActual < model.Cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible para realizar la salida.";
            model.Productos = await LlenarProductos();
                model.Depositos = await LlenarDepositos();
                model.Clientes = await LlenarClientes();
                return View(model);
            }
            else
            {
                // Iniciamos la transacción (AjustarStock y Movimiento)
                using var transaction = await
               _context.Database.BeginTransactionAsync();
                try
                {
                    // Pasamos la cantidad en negativo para restar del stock
                    await AjustarStock(model.idProducto, model.idDepositoOrigen,
                    -model.Cantidad);
                    var movimiento = new Movimiento
                    {
                        Tipo = TipoMovimiento.Salida,
                        idProducto = model.idProducto!.Value,
                        idDepositoOrigen = model.idDepositoOrigen!.Value,
                        Cantidad = model.Cantidad,
                        idCliente = model.idCliente!.Value,
                        Observaciones = model.Observaciones,
                        Fecha = DateTime.Now,
                        UsuarioRegistro = User.Identity?.Name ?? "Sistema"
                    };
                    _context.Movimientos.Add(movimiento);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                   
                TempData["OK"] = "Salida de stock registrada correctamente.";
                return RedirectToAction("Index");
                }
                catch (InvalidOperationException)
                {
                    TempData["Error"] = "Ocurrió un error al registrar la salida de stock.";
                }
            }
        }
        // Si el modelo no es válido, volver a llenar los select lists y retornar la vista
        model.Productos = await LlenarProductos();
        model.Depositos = await LlenarDepositos();
        model.Clientes = await LlenarClientes();
        return View(model);
    }
    #endregion Salida
    #region Transferencia
    //----------------Registrar Tranferencias------------------------------
    //GET : MOVIMIENTOS/Transferencia
    public async Task<IActionResult> Transferencia()
    {
        var model = new TransferenciaViewModel
        {
            Productos = await ObtenerProductos(),
            Depositos = await ObtenerDepositos(),
        };
        return View(model);

    }
    // POST: MOVIMIENTOS/Transferencia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transferencia(TransferenciaViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Validar que no se transfiera al mismo depósito
                if (model.idDepositoOrigen == model.idDepositoDestino)
                    throw new InvalidOperationException("El depósito de origen y destino no pueden ser el mismo.");
                   
                // Validar stock en el origen ANTES de abrir la transacción
                   var stockOrigen = await ObtenerStock(model.idProducto, model.idDepositoOrigen);
                if (stockOrigen < model.Cantidad)
                    throw new InvalidOperationException(
                    $"Stock insuficiente en origen. Disponible:{ stockOrigen }");
            }
            catch (InvalidOperationException ex)
            { // CAPTURA ESPECÍFICA: Capturamos el mensaje creado
                TempData["Error"] = ex.Message;
                model.Productos = await ObtenerProductos();
                model.Depositos = await ObtenerDepositos();
                return View(model);
            }
            using var transaction = await
           _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Restar del depósito origen y registrar su salida
                await AjustarStock(model.idProducto, model.idDepositoOrigen, -
               model.Cantidad);
 
                var movimientoSalida = new Movimiento
            {
                    Tipo = TipoMovimiento.TransferenciaSalida,
                    idProducto = model.idProducto!.Value,
                    idDepositoOrigen = model.idDepositoOrigen,
                    idDepositoDestino = model.idDepositoDestino,
                    Cantidad = model.Cantidad,
                    Observaciones = model.Observaciones,
                    Fecha = DateTime.Now,
                    UsuarioRegistro = User.Identity?.Name ?? "Sistema"
                };
                _context.Add(movimientoSalida);

                // 2. Sumar al depósito destino y registrar su entrada
                await AjustarStock(model.idProducto, model.idDepositoDestino,
               model.Cantidad);
                var movimientoEntrada = new Movimiento
                {
                    Tipo = TipoMovimiento.TransferenciaEntrada,
                    idProducto = model.idProducto!.Value,
                    idDepositoOrigen = model.idDepositoOrigen,
                    idDepositoDestino = model.idDepositoDestino,
                    Cantidad = model.Cantidad,
                    Observaciones = model.Observaciones,
                    Fecha = DateTime.Now,
                    UsuarioRegistro = User.Identity?.Name ?? "Sistema"
                };
                _context.Add(movimientoEntrada);
                // 3. Guardar ambos movimientos y actualizar stocks juntos
                await _context.SaveChangesAsync();
                // 4. Confirmar todo el bloque
                await transaction.CommitAsync();
                TempData["OK"] = $"Transferencia de {model.Cantidad} unidades registrada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // El rollback es automático al salir del using en caso de error
                 TempData["Error"] = "Ocurrió un error inesperado al registrar la transferencia.";
            }
        }
        // Si falló, recargamos las listas para la vista
        model.Productos = await ObtenerProductos();
        model.Depositos = await ObtenerDepositos();
        return View(model);
    }
    #endregion Transferencia
    #region Funciones
    //FUNCIONES
    private async Task<List<SelectListItem>> ObtenerProductos(int? idSeleccionado =
    null)
    {
        var productos = await _context.Productos
        .AsNoTracking()
        .OrderBy(c => c.PrNombre)
        .ToListAsync();
        return new SelectList(productos, "Id", "PrNombre", idSeleccionado).ToList();
    }
    private async Task<List<SelectListItem>> ObtenerDepositos(int? idSeleccionado = null)
    {
        var depositos = await _context.Depositos
        .AsNoTracking()
        .OrderBy(c => c.Nombre)
        .ToListAsync();

        return new SelectList(depositos, "Id", "Nombre", idSeleccionado).ToList();
    }
    private async Task<List<SelectListItem>> ObtenerProveedores(int? idSeleccionado = null)
    {
        var proveedores = await _context.Proveedores
        .AsNoTracking()
        .OrderBy(c => c.Nombre)
        .ToListAsync();
        return new SelectList(proveedores, "Id", "Nombre", idSeleccionado).ToList();
    }
    private async Task<List<SelectListItem>> ObetenerCliente(int? idSeleccionado = null)
    {
        var clientes = await _context.Clientes
        .AsNoTracking()
        .OrderBy(c => c.Nombre)
        .ToListAsync();
        return new SelectList(clientes, "Id", "Nombre", idSeleccionado).ToList();
    }
    //Métodos 
    private async Task<List<SelectListItem>> LlenarProductos()
    {
        return await ObtenerProductos();
    }
    private async Task<List<SelectListItem>> LlenarDepositos()
    {
        return await ObtenerDepositos();
    }
    private async Task<List<SelectListItem>> LlenarProveedores()
    {
        return await ObtenerProveedores();
    }
    private async Task<List<SelectListItem>> LlenarClientes()
    {
        return await ObetenerCliente();
    }

    // Método privado: crea o actualiza el registro StockDeposito
    private async Task AjustarStock(int? idProducto, int? idDeposito, int
   Cant)
    {
        // Buscamos si ya existe el registro de stock para ese producto en ese depósito
         var stock = await _context.StockDepositos
         .FirstOrDefaultAsync(s => s.idProducto == idProducto
         && s.idDeposito == idDeposito);
        if (stock == null)
        {
            // Si no existe (por ejemplo, primera entrada a un depósito nuevo)
            stock = new StockDeposito
            {
                idProducto = idProducto!.Value,
                idDeposito = idDeposito!.Value,
                Cantidad = 0
            };
            _context.StockDepositos.Add(stock);
        }
        // Si existe, modificamos la cantidad
        stock.Cantidad += Cant;
        // NOTA: No llamamos a _context.SaveChangesAsync() acá adentro,dejamos que el método principal del controlador lo haga junto con los movimientos.
        if (stock.Cantidad < 0)
            throw new InvalidOperationException("El stock no puede ser negativo.");
    }
    private async Task<int> ObtenerStock(int? idProducto, int? idDeposito)
    {
        var stock = await _context.StockDepositos
        .FirstOrDefaultAsync(x => x.idProducto == idProducto
        && x.idDeposito == idDeposito);
        if (stock != null)
        {
            return stock.Cantidad;
        }
        else
        {
            return 0;
        }
    }

    #endregion Funciones
}
