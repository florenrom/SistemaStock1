using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuevoSistemaNat.Migrations
{
    /// <inheritdoc />
    public partial class inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CADescripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrCodigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrPrecioCosto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrPrecioVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrStockMinimo = table.Column<int>(type: "int", nullable: false),
                    PrActivo = table.Column<bool>(type: "bit", nullable: false),
                    UnidadDeMedida = table.Column<int>(type: "int", nullable: false),
                    CaId = table.Column<int>(type: "int", nullable: false),
                    CADescripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaActivo = table.Column<bool>(type: "bit", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CaId",
                        column: x => x.CaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "CADescripcion", "CaActivo", "CaNombre", "createAt", "updateAt" },
                values: new object[,]
                {
                    { 1, "", true, "Alimentos", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "", true, "Limpieza", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "", true, "Lacteos", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CaId",
                table: "Productos",
                column: "CaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
