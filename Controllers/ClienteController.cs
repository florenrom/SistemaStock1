using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels;


namespace NuevoSistemaNat.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cliente
        public async Task<IActionResult> Index(string buscar)
        {
            var clientes = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string termino = buscar.Trim();

                clientes = clientes.Where(c =>
                    c.Nombre.Contains(termino) ||
                    (c.Documento != null && c.Documento.Contains(termino)) ||
                    (c.Telefono != null && c.Telefono.Contains(termino)) ||
                    (c.Email != null && c.Email.Contains(termino)));
            }

            var lista = await clientes
                .OrderBy(c => c.Nombre)
                .Select(c => new ClienteViewModel
                {
                    id = c.Id,
                    Nombre = c.Nombre,
                    Documento = c.Documento,
                    Direccion = c.Direccion,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = c.CaActivo
                })
                .ToListAsync();

            return View(lista);
        }

        // GET: Cliente/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.CaActivo
            };

            return View(model);
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            return View(new ClienteViewModel());
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Obtener el DNI sin espacios al principio o al final
            string? documento = model.Documento?.Trim();

            // Comprobar si el DNI ya está registrado
            bool documentoDuplicado = await _context.Clientes
                .AnyAsync(c => c.Documento == documento);

            if (!string.IsNullOrEmpty(documento) && documentoDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Documento),
                    "Ya existe un cliente registrado con ese DNI."
                );

                return View(model);
            }

            var cliente = new Cliente
            {
                Nombre = model.Nombre.Trim(),
                Documento = documento,
                Direccion = model.Direccion?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Email = model.Email?.Trim(),
                CaActivo = model.Activo
            };

            _context.Clientes.Add(cliente);

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Cliente/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.CaActivo
            };

            return View(model);
        }

        // POST: Cliente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteViewModel model)
        {
            if (id != model.id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            // Verificar que el DNI no pertenezca a otro cliente
            string? documento = model.Documento?.Trim();

            bool documentoDuplicado = await _context.Clientes
                .AnyAsync(c => c.Documento == documento && c.Id != id);

            if (!string.IsNullOrEmpty(documento) && documentoDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Documento),
                    "Ya existe otro cliente registrado con ese DNI."
                );

                return View(model);
            }

            cliente.Nombre = model.Nombre.Trim();
            cliente.Documento = documento;
            cliente.Direccion = model.Direccion?.Trim();
            cliente.Telefono = model.Telefono?.Trim();
            cliente.Email = model.Email?.Trim();
            cliente.CaActivo = model.Activo;
            cliente.updateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Cliente/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.CaActivo
            };

            return View(model);
        }

        // POST: Cliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            if (!cliente.CaActivo)
            {
                TempData["Error"] = "El cliente ya se encuentra desactivado.";
                return RedirectToAction(nameof(Index));
            }

            cliente.CaActivo = false;
            cliente.updateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
