using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuevoSistemaNat.Models.Entities;
using NuevoSistemaNat.Models.Enum;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NuevoSistemaNat.Controllers
{
    public class ReporteController : Controller
    {
        private readonly AppDbContext _context;
        public ReporteController(AppDbContext context)
        {
            _context = context;
        }

   
    public IActionResult Index()
    {
         return View();
    }

     // REPORTE EXCEL
    public async Task<IActionResult> DescargarExcel(DateTime? desde,DateTime? hasta,string? tipo)
    {
        var movimientos = await FiltrarMovimientosAsync(desde, hasta, tipo);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Movimientos");

        worksheet.Cell(1, 1).Value = "Reporte de Movimientos de Stock";

        string[] encabezados =
        {
            "ID",
            "Fecha",
            "Tipo",
            "Producto",
            "Depósito Origen",
            "Depósito Destino",
            "Cantidad",
            "Usuario"
        };

        for (int i = 0; i < encabezados.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = encabezados[i];
        }

        int fila = 4;

        foreach (var m in movimientos)
        {
            worksheet.Cell(fila, 1).Value = m.Id;
            worksheet.Cell(fila, 2).Value = m.Fecha;
            worksheet.Cell(fila, 3).Value = m.Tipo.ToString();
            worksheet.Cell(fila, 4).Value = m.Producto?.PrNombre ?? "Sin producto";
            worksheet.Cell(fila, 5).Value = m.DepositoOrigen?.Nombre ?? "Sin origen";
            worksheet.Cell(fila, 6).Value = m.DepositoDestino?.Nombre ?? "Sin destino";
            worksheet.Cell(fila, 7).Value = m.Cantidad;
            worksheet.Cell(fila, 8).Value = m.UsuarioRegistro;

            fila++;
        }

        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        var rangoEncabezados = worksheet.Range(3, 1, 3, 8);
        rangoEncabezados.Style.Font.Bold = true;
        rangoEncabezados.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Column(2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ReporteMovimientos.xlsx"
            );
        }

        // REPORTE PDF
        public async Task<IActionResult> DescargarPdf(DateTime? desde,DateTime? hasta,string? tipo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var movimientos = await FiltrarMovimientosAsync(desde, hasta, tipo);

            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);

                    page.Header()
                        .Text("Reporte de Movimientos de Stock")
                        .FontSize(18)
                        .Bold();

                    page.Content().PaddingVertical(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.6f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("ID");
                            header.Cell().Element(HeaderStyle).Text("Fecha");
                            header.Cell().Element(HeaderStyle).Text("Tipo");
                            header.Cell().Element(HeaderStyle).Text("Producto");
                            header.Cell().Element(HeaderStyle).Text("Origen");
                            header.Cell().Element(HeaderStyle).Text("Destino");
                            header.Cell().Element(HeaderStyle).Text("Cantidad");
                            header.Cell().Element(HeaderStyle).Text("Usuario");
                        });

                        foreach (var m in movimientos)
                        {
                            table.Cell().Element(CellStyle).Text(m.Id.ToString());
                            table.Cell().Element(CellStyle).Text(m.Fecha.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Element(CellStyle).Text(m.Tipo.ToString());
                            table.Cell().Element(CellStyle).Text(m.Producto?.PrNombre ?? "Sin producto");
                            table.Cell().Element(CellStyle).Text(m.DepositoOrigen?.Nombre ?? "Sin origen");
                            table.Cell().Element(CellStyle).Text(m.DepositoDestino?.Nombre ?? "Sin destino");
                            table.Cell().Element(CellStyle).Text(m.Cantidad.ToString());
                            table.Cell().Element(CellStyle).Text(m.UsuarioRegistro);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });

            var pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                "ReporteMovimientos.pdf"
            );
        }

        // FILTRAR MOVIMIENTOS
        private async Task<List<Movimiento>> FiltrarMovimientosAsync(DateTime? desde, DateTime? hasta,string? tipo)
        {
            var query = _context.Movimientos
                .AsNoTracking()
                .Include(m => m.Producto)
                .Include(m => m.DepositoOrigen)
                .Include(m => m.DepositoDestino)
                .Include(m => m.Proveedor)
                .Include(m => m.Cliente)
                .AsQueryable();

            if (desde.HasValue)
            {
                query = query.Where(m => m.Fecha >= desde.Value.Date);
            }

            if (hasta.HasValue)
            {
                query = query.Where(m => m.Fecha < hasta.Value.Date.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                if (tipo == "Entrada")
                {
                    query = query.Where(m => m.Tipo == TipoMovimiento.Entrada);
                }
                else if (tipo == "Salida")
                {
                    query = query.Where(m => m.Tipo == TipoMovimiento.Salida);
                }
                else if (tipo == "Transferencia")
                {
                    query = query.Where(m =>
                        m.Tipo == TipoMovimiento.TransferenciaEntrada ||
                        m.Tipo == TipoMovimiento.TransferenciaSalida);
                }
            }

            return await query
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        // ESTILOS DE TABLA PDF
        private static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Darken2)
                .Padding(5)
                .DefaultTextStyle(x => x
                    .FontColor(Colors.White)
                    .SemiBold()
                    .FontSize(8));
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .DefaultTextStyle(x => x.FontSize(8));
        }

    }
}
