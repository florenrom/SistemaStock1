
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;


[Authorize(Roles = "Administrador,Encargado")]
public class CategoriaController : Controller
{
    private readonly AppDbContext _context;

    public CategoriaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: CATEGORIAS
    public async Task<IActionResult> Index()    
    {
        // Traemos las categorías de la base de datos
        var categorias = await _context.Categorias.ToListAsync();

        // Mapeo manual de Lista de Entidades -> Lista de ViewModels
        var listaViewModels = categorias.Select(c => new CategoriaViewModel
        {
            Id = c.Id,
            CaNombre = c.CaNombre,
            CADescripcion = c.CADescripcion,
            Activo = c.CaActivo
        }).ToList();

        return View(listaViewModels);
    }

    // GET: CATEGORIAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
       
        var categoria = await _context.Categorias
            .Include(c => c.Productos) // Incluimos la colección de productos relacionados
            .FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();

        // Mapeo manual de Entidad -> ViewModel
        var model = new CategoriaViewModel
        {
            Id = categoria.Id,
            CaNombre = categoria.CaNombre,
            CADescripcion = categoria.CADescripcion,
            Activo = categoria.CaActivo,
            createAt = categoria.createAt,
            updateAt = categoria.updateAt,
            cantidadProductos = categoria.Productos.Count
        };

        return View(model);
    }

    // GET: CATEGORIAS/Create
    public IActionResult Create()
    {
        return View(new CategoriaViewModel());
    }

    // POST: CATEGORIAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 1. Validar que el nombre no exista en la base de datos (ignorando mayúsculas / minúsculas)
            bool existeCategoria = await _context.Categorias
                .AnyAsync(c => c.CaNombre.ToLower() == model.CaNombre.ToLower());
            if (existeCategoria)
            {
                // 2. Agregar el error al ModelState para que se muestre en la vista
                ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");
                // Retorna la misma vista con el modelo para no perder los datos ingresados
                return View(model);
            }
            // Mapeo manual de ViewModel -> Entidad (para guardar en la DB)
            var nuevaCategoria = new Categoria
            {
                CaNombre = model.CaNombre,
                CADescripcion = model.CADescripcion,
                CaActivo = model.Activo
                // Nota: 'id' no se asigna porque la base de datos lo generasolo(Identity)
            };

            _context.Categorias.Add(nuevaCategoria);
            await _context.SaveChangesAsync();

            // Guardamos el mensaje en TempData antes de redirigir
            TempData["Ok"] = "¡La categoría se ha creado con éxito!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);

    }

    // GET: CATEGORIAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        // Mapeo manual de Entidad -> ViewModel (para rellenar el formulario)
        var model = new CategoriaViewModel
        {
            Id = categoria.Id,
            CaNombre = categoria.CaNombre,
            CADescripcion = categoria.CADescripcion,
            Activo = categoria.CaActivo
        };


        return View(model);
    }

    // POST: CATEGORIAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, CategoriaViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            // Verificar que no exista otra categoría con el mismo nombre
            bool nombreDuplicado = await _context.Categorias
                .AnyAsync(c => c.CaNombre == model.CaNombre && c.Id != id);

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.CaNombre),
                    "Ya existe otra categoría con ese nombre."
                );

                return View(model);
            }

            // Actualizar los datos de la categoría
            categoria.CaNombre = model.CaNombre;
            categoria.CADescripcion = model.CADescripcion;
            categoria.CaActivo = model.Activo;
            categoria.updateAt = DateTime.UtcNow;

            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "¡La categoría se ha modificado con éxito!";

            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: CATEGORIAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(m => m.Id == id);
        if (categoria == null)return NotFound();

        // Mapeo manual de Entidad -> ViewModel
        var model = new CategoriaViewModel
        {
            Id = categoria.Id,
            CaNombre = categoria.CaNombre,
            CADescripcion = categoria.CADescripcion,
            Activo = categoria.CaActivo
        };

        return View(model);
    }

    // POST: CATEGORIAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return NotFound();

        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
            return NotFound();

        // Verificar si existen productos asociados
        bool tieneProductos = await _context.Productos
            .AnyAsync(p => p.CaId == id);

        if (tieneProductos)
        {
            TempData["Error"] = $"No se puede eliminar la categoría '{categoria.CaNombre}' porque tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categorias.Remove(categoria);

        await _context.SaveChangesAsync();

        TempData["Ok"] = "Categoría eliminada con éxito.";

        return RedirectToAction(nameof(Index));
    }

    private bool CategoriaExists(int? id)
    {
        return _context.Categorias.Any(e => e.Id == id);
    }
}
