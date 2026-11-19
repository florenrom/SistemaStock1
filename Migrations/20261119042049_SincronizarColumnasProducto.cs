using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuevoSistemaNat.Migrations
{
    //migrationBuilder.RenameColumn(

    /// <inheritdoc />
    public partial class SincronizarColumnasProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrStockMinimo",
                table: "Productos",
                newName: "StockMinimo");

            migrationBuilder.AddColumn<int>(
                name: "CantidadStock",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrDescripcion",
                table: "Productos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Depositos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depositos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockDepositos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idProducto = table.Column<int>(type: "int", nullable: false),
                    idDeposito = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDepositos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDepositos_Depositos_idDeposito",
                        column: x => x.idDeposito,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockDepositos_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    idProducto = table.Column<int>(type: "int", nullable: false),
                    idDepositoOrigen = table.Column<int>(type: "int", nullable: true),
                    idDepositoDestino = table.Column<int>(type: "int", nullable: true),
                    idProveedor = table.Column<int>(type: "int", nullable: true),
                    idCliente = table.Column<int>(type: "int", nullable: true),
                    UsuarioRegistro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimientos_Clientes_idCliente",
                        column: x => x.idCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Movimientos_Depositos_idDepositoDestino",
                        column: x => x.idDepositoDestino,
                        principalTable: "Depositos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Movimientos_Depositos_idDepositoOrigen",
                        column: x => x.idDepositoOrigen,
                        principalTable: "Depositos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Movimientos_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Movimientos_Proveedores_idProveedor",
                        column: x => x.idProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CADescripcion", "CaActivo", "Contacto", "Direccion", "Documento", "Email", "Nombre", "Observaciones", "Telefono", "createAt", "updateAt" },
                values: new object[] { 1, "", true, null, null, null, null, "Cliente A", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "Depositos",
                columns: new[] { "Id", "CADescripcion", "CaActivo", "Nombre", "Ubicacion", "createAt", "updateAt" },
                values: new object[,]
                {
                    { 1, "", true, "Depósito General", "Plantabaja", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "", true, "Depósito Principal", "Planta alta", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "", true, "Depósito Secundario", "Planta baja", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "Id", "CADescripcion", "CaActivo", "Contacto", "Direccion", "Documento", "Email", "Nombre", "Observaciones", "Telefono", "createAt", "updateAt" },
                values: new object[] { 1, "", true, null, null, null, null, "Proveedor A", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_idCliente",
                table: "Movimientos",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_idDepositoDestino",
                table: "Movimientos",
                column: "idDepositoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_idDepositoOrigen",
                table: "Movimientos",
                column: "idDepositoOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_idProducto",
                table: "Movimientos",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_idProveedor",
                table: "Movimientos",
                column: "idProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_StockDepositos_idDeposito",
                table: "StockDepositos",
                column: "idDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_StockDepositos_idProducto",
                table: "StockDepositos",
                column: "idProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropTable(
                name: "StockDepositos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Depositos");

            migrationBuilder.DropColumn(
                name: "CantidadStock",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrDescripcion",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "StockMinimo",
                table: "Productos",
                newName: "PrStockMinimo");
        }
    }
}
