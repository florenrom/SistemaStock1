using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuevoSistemaNat.Models.Entities;
using System;
using System.Reflection.Emit;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; } = default!;
    public DbSet<Categoria> Categorias { get; set; } = default!;
    public DbSet<Movimiento> Movimientos { get; set; } = default!;
    public DbSet<Deposito> Depositos { get; set; } = default!;
    public DbSet<Proveedor> Proveedores { get; set; } = default!;
    public DbSet<Cliente> Clientes { get; set; } = default!;
    public DbSet<StockDeposito> StockDepositos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Evitar delete cascade
        modelBuilder.Entity<Producto>()
        .HasOne(m => m.Categoria) // Un Producto tiene una Categoria
        .WithMany(m => m.Productos) // Una Categoria tiene muchos Productos
        .HasForeignKey(m => m.CaId) // La clave foránea en Producto
        .OnDelete(DeleteBehavior.Restrict); // Evita que operaciones en cascada afecten a la categoría

        //Datos semilla de pueba para las tablas
         modelBuilder.Entity<Categoria>().HasData(
            new Categoria{ Id = 1, CaNombre = "Alimentos",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), updateAt = null},
            new Categoria { Id = 2, CaNombre = "Limpieza",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), updateAt = null},
            new Categoria{ Id = 3, CaNombre = "Lacteos",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), updateAt = null}
         );

        modelBuilder.Entity<Deposito>().HasData(
        new Deposito{Id = 1,Nombre = "Depósito General",Ubicacion = "Plantabaja", CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0,DateTimeKind.Utc), updateAt = null },

        new Deposito { Id = 2, Nombre = "Depósito Principal", Ubicacion ="Planta alta", CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0,DateTimeKind.Utc), updateAt = null },

        new Deposito { Id = 3,Nombre = "Depósito Secundario",Ubicacion ="Planta baja",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0,DateTimeKind.Utc),updateAt = null});

        modelBuilder.Entity<Proveedor>().HasData(
        new Proveedor{Id = 1,Nombre = "Proveedor A",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),updateAt =null});

        modelBuilder.Entity<Cliente>().HasData
        (new Cliente{Id = 1,Nombre = "Cliente A",CaActivo = true, createAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),updateAt = null});

    }
}
