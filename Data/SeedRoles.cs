using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuevoSistemaNat.Models.Entities;

namespace NuevoSistemaNat.Data
{
    public class SeedRoles
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            // Crear roles si no existen
            string[] roles = { "Administrador", "Encargado", "Operador", "Consulta" };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // Obtener credenciales desde una configuración segura
            string? adminEmail = configuration["AdminUser:Email"];
            string? adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Faltan las credenciales iniciales del administrador en la configuración segura."
                );
            }

            // Crear administrador si no existe
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombre = "Administrador del Sistema",
                    FechaRegistro = DateTime.UtcNow,
                    EmailConfirmed = true,
                    Activo = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
                else
                {
                    throw new Exception(
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }
}