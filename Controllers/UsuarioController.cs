using Microsoft.AspNetCore.Mvc;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.ViewModels.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NuevoSistemaNat.Controllers
{
//    var usuario = await _userManager.FindByIdAsync(model.Id);

//if (usuario == null)
//    return NotFound();

    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuarioController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        // GET: Usuario/Index
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();

            var listaUsuarios = new List<UsuarioViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                listaUsuarios.Add(new UsuarioViewModel
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email ?? string.Empty,
                    Rol = roles.FirstOrDefault() ?? "Sin rol",
                    Activo = usuario.Activo
                });
            }

            return View(listaUsuarios);
        }

        // GET: Usuario/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

            return View();
        }

        // POST: Usuario/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
                return View(model);
            }

            var usuario = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                FechaRegistro = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Activo = true,
                UsuarioRegistro = User.Identity?.Name
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                var resultadoRol = await _userManager.AddToRoleAsync(usuario, model.Rol);

                if (!resultadoRol.Succeeded)
                {
                    await _userManager.DeleteAsync(usuario);

                    foreach (var error in resultadoRol.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

            return View(model);
        }

        // GET: Usuario/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);

            var model = new EditarViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? string.Empty,
                Activo = usuario.Activo
            };

            ViewBag.Roles = new SelectList(
                _roleManager.Roles.ToList(),
                "Name",
                "Name",
                model.Rol
            );

            return View(model);
        }

        // POST: Usuario/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(
                    _roleManager.Roles.ToList(), "Name", "Name", model.Rol);
                return View(model);
            }

            var usuario = await _userManager.FindByIdAsync(model.Id);

            if (usuario == null)
                return NotFound();

            // Evitar que el administrador desactive su propia cuenta
            var usuarioActualId = _userManager.GetUserId(User);

            if (usuario.Id == usuarioActualId && !model.Activo)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No puedes desactivar tu propia cuenta."
                );

                ViewBag.Roles = new SelectList(
                    _roleManager.Roles.ToList(), "Name", "Name", model.Rol);

                return View(model);
            }

            usuario.Nombre = model.Nombre;
            usuario.Email = model.Email;
            usuario.UserName = model.Email;
            usuario.Activo = model.Activo;
            usuario.UpdatedAt = DateTime.UtcNow;

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.Roles = new SelectList(
                    _roleManager.Roles.ToList(), "Name", "Name", model.Rol);

                return View(model);
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            if (rolesActuales.Any())
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

            var resultadoRol = await _userManager.AddToRoleAsync(usuario, model.Rol);

            if (!resultadoRol.Succeeded)
            {
                foreach (var error in resultadoRol.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.Roles = new SelectList(
                    _roleManager.Roles.ToList(), "Name", "Name", model.Rol);

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuario/ResetearPassword/5
        [HttpGet]
        public async Task<IActionResult> ResetearPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
                return NotFound();

            var model = new ResetearPasswordViewModel
            {
                Id = usuario.Id
            };

            return View(model);
        }

        // POST: Usuario/ResetearPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetearPassword(
            ResetearPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.FindByIdAsync(model.Id);

            if (usuario == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var resultado = await _userManager.ResetPasswordAsync(
                usuario,
                token,
                model.PasswordNueva
            );

            if (resultado.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


    }
}
