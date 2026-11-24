using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;


namespace NuevoSistemaNat.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index(string buscar)
        {
            var proveedores = _context.Proveedores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string termino = buscar.Trim();

                proveedores = proveedores.Where(p =>
                    p.Nombre.Contains(termino) ||
                    (p.Documento != null && p.Documento.Contains(termino)) ||
                    (p.Telefono != null && p.Telefono.Contains(termino)) ||
                    (p.Email != null && p.Email.Contains(termino)) ||
                    (p.Direccion != null && p.Direccion.Contains(termino)));
            }

            var lista = await proveedores
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorViewModel
                {
                    id = p.Id,
                    Nombre = p.Nombre,
                    Documento = p.Documento,
                    Direccion = p.Direccion,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Activo = p.CaActivo
                })
                .ToListAsync();

            return View(lista);
        }

        // GET: Proveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Activo = proveedor.CaActivo
            };

            return View(model);
        }

        // GET: Proveedor/Create
        public IActionResult Create()
        {
            return View(new ProveedorViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? documento = model.Documento?.Trim();

            // Comprobar si el DNI ya está registrado
            bool documentoDuplicado = await _context.Proveedores
                .AnyAsync(p => p.Documento == documento);

            if (!string.IsNullOrEmpty(documento) && documentoDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Documento),
                    "Ya existe un proveedor registrado con ese DNI."
                );

                return View(model);
            }

            var proveedor = new Proveedor
            {
                Nombre = model.Nombre.Trim(),
                Documento = documento,
                Direccion = model.Direccion?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Email = model.Email?.Trim(),
                CaActivo = model.Activo
            };

            _context.Proveedores.Add(proveedor);

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Proveedor registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Proveedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Activo = proveedor.CaActivo
            };

            return View(model);
        }

        // POST: Proveedor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProveedorViewModel model)
        {
            if (id != model.id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound();

            string? documento = model.Documento?.Trim();

            // Comprobar si el DNI pertenece a otro proveedor
            bool documentoDuplicado = await _context.Proveedores
                .AnyAsync(p => p.Documento == documento && p.Id != id);

            if (!string.IsNullOrEmpty(documento) && documentoDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Documento),
                    "Ya existe otro proveedor registrado con ese DNI."
                );

                return View(model);
            }

            proveedor.Nombre = model.Nombre.Trim();
            proveedor.Documento = documento;
            proveedor.Direccion = model.Direccion?.Trim();
            proveedor.Telefono = model.Telefono?.Trim();
            proveedor.Email = model.Email?.Trim();
            proveedor.CaActivo = model.Activo;
            proveedor.updateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Proveedor actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Proveedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Activo = proveedor.CaActivo
            };

            return View(model);
        }

        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound();

            if (!proveedor.CaActivo)
            {
                TempData["Error"] = "El proveedor ya se encuentra desactivado.";
                return RedirectToAction(nameof(Index));
            }

            proveedor.CaActivo = false;
            proveedor.updateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Proveedor desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
