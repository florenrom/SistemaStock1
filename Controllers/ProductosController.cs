using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NuevoSistemaNat.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;
        public ProductosController(AppDbContext context)
        {
            _context = context;
        }
        // GET: PRODUCTOS nuevos
        public async Task<IActionResult> Index(string buscar, int? buscarCategoria)
        {
            var productos = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                productos = productos.Where(p => p.PrNombre.Contains(buscar) ||
               p.PrCodigo!.Contains(buscar));

            if (buscarCategoria.HasValue)
                productos = productos.Where(p => p.CaId == buscarCategoria.Value);

            ViewBag.Categorias = new SelectList
            (await _context.Categorias.ToListAsync(), "id", "Nombre");

            // Mapeo manual de Lista de Entidades -> Lista de ViewModels
            var listaViewModels = productos.Select(p => new ProductoViewModel
            {
                id = p.Id,
                Codigo = p.PrCodigo,
                Nombre = p.PrNombre,
                Descripcion = p.CADescripcion,
                PrecioCosto = p.PrPrecioCosto,
                PrecioVenta = p.PrPrecioVenta,
                StockMinimo = p.StockMinimo,
                CategoriaNombre = p.Categoria!.CaNombre,
                UnidadDeMedida = p.UnidadDeMedida
            }).ToList();
            return View(listaViewModels);
        }
        // GET: PRODUCTOS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }
            // Mapeo manual de Entidad -> ViewModel
            var model = new ProductoViewModel
            {
                id = producto.Id,
                Nombre = producto.PrNombre,
                Descripcion = producto.PrDescripcion,
                PrecioCosto = producto.PrPrecioCosto,
                PrecioVenta = producto.PrPrecioVenta,
                StockMinimo = producto.StockMinimo,
                CategoriaNombre = producto.Categoria?.CaNombre,
                UnidadDeMedida = producto.UnidadDeMedida,
                Activo = producto.PrActivo,
                CreatedAt = producto.createAt,
                UpdatedAt = producto.updateAt,
            };
            return View(model);
        }


        // GET: PRODUCTOS/Create
        public async Task<IActionResult> CreateAsync()
         {
            var model = new ProductoViewModel
            {
                Categorias = await _context.Categorias
                .Where(c =>c.CaActivo)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CaNombre
                })
                .ToListAsync()
            };
            return View(model);
        }
        private async Task<List<SelectListItem>> ObtenerCategorias(int? idSeleccionado = null)
        {
            var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.CaNombre)
            .ToListAsync();
            return new SelectList(categorias, "id", "Nombre", idSeleccionado).ToList();
        }

        // POST: PRODUCTOS/Create
        // To protect from overposting attacks, enable the specific properties you want to bindto.
         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(ProductoViewModel model)
         {
            if (ModelState.IsValid)
            {
                // 1. Validar que no exista en la base de datos (ignorando mayúsculas/minúsculas)
                bool existeProducto = await _context.Productos
                .AnyAsync(c => c.PrCodigo!.ToLower() == model.Codigo!.ToLower() &&
               c.PrNombre.ToLower() == model.Nombre.ToLower());


                if (existeProducto)
                {
                    // 2. Agregar el error al ModelState para que se muestre en la vista
                    ModelState.AddModelError("Nombre", "Ya existe un producto con ese codigo y nombre.");

                    // Retorna la misma vista con el modelo para no perder los datos ingresados
                    return View(model);
                }
                var producto = new Producto
                {
                    PrCodigo = model.Codigo,
                    PrNombre = model.Nombre,
                    CADescripcion = model.Descripcion,
                    PrPrecioCosto = model.PrecioCosto,
                    PrPrecioVenta = model.PrecioVenta,
                    StockMinimo = model.StockMinimo,
                    Id = model.idCategoria!.Value,
                    UnidadDeMedida = model.UnidadDeMedida
                };

                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            model.Categorias = await _context.Categorias.Where(c =>c.CaActivo)
                .Select(c=> new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CaNombre
                })
                .ToListAsync();
            {
                return View(model);
            }
         }
         // GET: PRODUCTOS/Edit/5
         public async Task<IActionResult> Edit(int? id)
         {
            if (id == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            // Mapeo manual de Entidad -> ViewModel (para rellenar el formulario)
            var model = new ProductoViewModel
            {
                id = producto.Id,
                Codigo = producto.PrCodigo,
                Nombre = producto.PrNombre,
                Descripcion = producto.CADescripcion,
                PrecioCosto = producto.PrPrecioCosto,
                PrecioVenta = producto.PrPrecioVenta,
                StockMinimo = producto.StockMinimo,
                idCategoria = producto.Id,
                UnidadDeMedida = producto.UnidadDeMedida
            };
            model.Categorias = await _context.Categorias.Where(c =>c.CaActivo)
                .Select(c=> new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CaNombre,
                    Selected = (c.Id == model.idCategoria)
                }) 
                .ToListAsync();

            return View(model);
         }

        // POST: PRODUCTOS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ProductoViewModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var producto = await _context.Productos.FindAsync(id);
                    // Actualizar las propiedades del producto con los valores del modelo
                    producto.PrCodigo = model.Codigo;
                    producto.PrNombre = model.Nombre;
                    producto.CADescripcion = model.Descripcion;
                    producto.PrPrecioCosto = model.PrecioCosto;
                    producto.PrPrecioVenta = model.PrecioVenta;
                    producto.StockMinimo = model.StockMinimo;
                    producto.Id = model.idCategoria!.Value;
                    producto.UnidadDeMedida = model.UnidadDeMedida;
                    producto.PrActivo = model.Activo;
                    producto.updateAt = DateTime.UtcNow;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Ok"] = "Producto actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(model.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            model.Categorias = await _context.Categorias.Where(c =>c.CaActivo)
                .Select(c=> new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CaNombre,
                    Selected = (c.Id == model.idCategoria)
                }) 
                .ToListAsync();
            return View(model);

        }
        // GET: PRODUCTOS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos
            .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

        // Mapeo manual de Entidad -> ViewModel
        var model = new ProductoViewModel
        {
            id = producto.Id,
            Codigo = producto.PrCodigo,
            Nombre = producto.PrNombre,
            Descripcion = producto.CADescripcion,
            PrecioCosto = producto.PrPrecioCosto,
            PrecioVenta = producto.PrPrecioVenta,
            StockMinimo = producto.StockMinimo,
            idCategoria = producto.Id,
            CategoriaNombre = producto.Categoria?.CaNombre,
            UnidadDeMedida = producto.UnidadDeMedida
        };
        return View(model);
        }
        // POST: PRODUCTOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var producto = await _context.Productos.FindAsync(id);
                //_context.Productos.Include(c =>c.Movimientos).FirstOrDefaultAsync(c => c.Id == id);
            if (producto != null)
            {
                //IMPORTANTE!!! Luego validaremos si no tiene movimientos asociados
                _context.Productos.Remove(producto);
            }
            await _context.SaveChangesAsync();
            // Mensaje de éxito
            TempData["Ok"] = "Producto eliminado con éxito.";
            return RedirectToAction(nameof(Index));
        }
        private bool ProductoExists(int? id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }

}
    
