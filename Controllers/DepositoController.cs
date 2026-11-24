using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;

namespace NuevoSistemaNat.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class DepositoController : Controller
    {
        private readonly AppDbContext _context;

        public DepositoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deposito/Index
        public async Task<IActionResult> Index(string buscar)
        {
            var depositos = _context.Depositos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                depositos = depositos.Where(d =>
                    d.Nombre.Contains(buscar) ||
                    (d.Ubicacion != null && d.Ubicacion.Contains(buscar)));
            }

            var lista = await depositos
                .OrderBy(d => d.Nombre)
                .Select(d => new DepositoViewModel
                {
                    id = d.Id,
                    Nombre = d.Nombre,
                    Ubicacion = d.Ubicacion,
                    Activo = d.CaActivo
                })
                .ToListAsync();

            return View(lista);
        }

        // GET: Deposito/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var deposito = await _context.Depositos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deposito == null)
                return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.Id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.CaActivo
            };

            return View(model);
        }

        // GET: Deposito/Create
        public IActionResult Create()
        {
            return View(new DepositoViewModel());
        }

        // POST: Deposito/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepositoViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.Depositos
                    .AnyAsync(d => d.Nombre.ToLower() == model.Nombre.Trim().ToLower());

                if (existe)
                {
                    ModelState.AddModelError(
                        "Nombre",
                        "Ya existe un depósito con ese nombre.");
                    return View(model);
                }

                var deposito = new Deposito
                {
                    Nombre = model.Nombre.Trim(),
                    Ubicacion = model.Ubicacion?.Trim(),
                    CaActivo = true,
                    createAt = DateTime.UtcNow
                };

                _context.Depositos.Add(deposito);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Depósito creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Deposito/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var deposito = await _context.Depositos.FindAsync(id);

            if (deposito == null)
                return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.Id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.CaActivo
            };

            return View(model);
        }

        // POST: Deposito/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepositoViewModel model)
        {
            if (id != model.id)
                return NotFound();

            if (ModelState.IsValid)
            {
                bool existe = await _context.Depositos
                    .AnyAsync(d =>
                        d.Nombre.ToLower() == model.Nombre.Trim().ToLower() &&
                        d.Id != id);

                if (existe)
                {
                    ModelState.AddModelError(
                        "Nombre",
                        "Ya existe otro depósito con ese nombre.");
                    return View(model);
                }

                var deposito = await _context.Depositos.FindAsync(id);

                if (deposito == null)
                    return NotFound();

                if (!model.Activo)
                {
                    bool tieneStock = await _context.StockDepositos
                        .AnyAsync(s =>
                            s.idDeposito == id &&
                            s.Cantidad > 0);

                    if (tieneStock)
                    {
                        ModelState.AddModelError(
                            "Activo",
                            "No se puede desactivar el depósito porque tiene stock disponible.");
                        return View(model);
                    }
                }

                deposito.Nombre = model.Nombre.Trim();
                deposito.Ubicacion = model.Ubicacion?.Trim();
                deposito.CaActivo = model.Activo;
                deposito.updateAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Ok"] = "Depósito actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Deposito/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var deposito = await _context.Depositos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deposito == null)
                return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.Id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.CaActivo
            };

            return View(model);
        }

        // POST: Deposito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deposito = await _context.Depositos.FindAsync(id);

            if (deposito == null)
                return NotFound();

            bool tieneStock = await _context.StockDepositos
                .AnyAsync(s =>
                    s.idDeposito == id &&
                    s.Cantidad > 0);

            if (tieneStock)
            {
                TempData["Error"] =
                    "No se puede desactivar el depósito porque tiene stock disponible.";

                return RedirectToAction(nameof(Index));
            }

            deposito.CaActivo = false;
            deposito.updateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Depósito desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
