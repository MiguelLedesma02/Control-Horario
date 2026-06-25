using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControlHorario.Infrastructure.Export;

public class ExportadorService : IExportadorService
{
    static ExportadorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportarExcel(int anio, int mes, IEnumerable<ResumenEmpleadoDto> resumenes)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet($"Preliquidacion {anio}-{mes:D2}");

        // Header
        ws.Cell(1, 1).Value = $"Preliquidación período {mes:D2}/{anio}";
        ws.Range(1, 1, 1, 11).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1f2937"))
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Columnas
        var columnas = new[] {
            "Legajo","Empleado","Días Trabajados","Ausencias Justificadas",
            "Ausencias Injustificadas","Min. Tardanza","Min. HS Extra 50%",
            "Min. HS Extra 100%","Días Licencia","Días Vacaciones","Cant. Novedades"
        };
        for (int i = 0; i < columnas.Length; i++)
        {
            var c = ws.Cell(3, i + 1);
            c.Value = columnas[i];
            c.Style.Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#374151"))
                .Font.SetFontColor(XLColor.White)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        // Filas
        int row = 4;
        foreach (var r in resumenes)
        {
            ws.Cell(row, 1).Value = r.Legajo;
            ws.Cell(row, 2).Value = r.NombreCompleto;
            ws.Cell(row, 3).Value = r.DiasTrabajados;
            ws.Cell(row, 4).Value = r.DiasAusenteJustificado;
            ws.Cell(row, 5).Value = r.DiasAusenteInjustificado;
            ws.Cell(row, 6).Value = r.MinutosTardanza;
            ws.Cell(row, 7).Value = r.MinutosHorasExtra50;
            ws.Cell(row, 8).Value = r.MinutosHorasExtra100;
            ws.Cell(row, 9).Value = r.DiasLicencia;
            ws.Cell(row, 10).Value = r.DiasVacaciones;
            ws.Cell(row, 11).Value = r.Novedades.Count;
            row++;
        }

        ws.Columns().AdjustToContents();

        // Hoja detallada de novedades
        var ws2 = wb.AddWorksheet("Detalle de Novedades");
        var detCols = new[] { "Legajo", "Empleado", "Tipo", "Desde", "Hasta", "Cantidad", "Observación" };
        for (int i = 0; i < detCols.Length; i++)
        {
            ws2.Cell(1, i + 1).Value = detCols[i];
            ws2.Cell(1, i + 1).Style.Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#374151"))
                .Font.SetFontColor(XLColor.White);
        }
        int r2 = 2;
        foreach (var emp in resumenes)
            foreach (var n in emp.Novedades)
            {
                ws2.Cell(r2, 1).Value = emp.Legajo;
                ws2.Cell(r2, 2).Value = emp.NombreCompleto;
                ws2.Cell(r2, 3).Value = n.Tipo.ToString();
                ws2.Cell(r2, 4).Value = n.FechaDesde;
                ws2.Cell(r2, 5).Value = n.FechaHasta;
                ws2.Cell(r2, 6).Value = (double)n.Cantidad;
                ws2.Cell(r2, 7).Value = n.Observacion ?? "";
                r2++;
            }
        ws2.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportarCsv(int anio, int mes, IEnumerable<ResumenEmpleadoDto> resumenes)
    {
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
        using var ms = new MemoryStream();
        using (var sw = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(sw, cfg))
        {
            csv.WriteField("Legajo"); csv.WriteField("Empleado");
            csv.WriteField("DiasTrabajados"); csv.WriteField("AusJustif");
            csv.WriteField("AusInjustif"); csv.WriteField("MinTardanza");
            csv.WriteField("MinHE50"); csv.WriteField("MinHE100");
            csv.WriteField("DiasLicencia"); csv.WriteField("DiasVacaciones");
            csv.NextRecord();

            foreach (var r in resumenes)
            {
                csv.WriteField(r.Legajo); csv.WriteField(r.NombreCompleto);
                csv.WriteField(r.DiasTrabajados); csv.WriteField(r.DiasAusenteJustificado);
                csv.WriteField(r.DiasAusenteInjustificado); csv.WriteField(r.MinutosTardanza);
                csv.WriteField(r.MinutosHorasExtra50); csv.WriteField(r.MinutosHorasExtra100);
                csv.WriteField(r.DiasLicencia); csv.WriteField(r.DiasVacaciones);
                csv.NextRecord();
            }
        }
        return ms.ToArray();
    }

    public byte[] ExportarPdf(int anio, int mes, IEnumerable<ResumenEmpleadoDto> resumenes)
    {
        var lista = resumenes.ToList();

        var doc = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Size(PageSizes.A4.Landscape());
                p.Margin(30);
                p.DefaultTextStyle(t => t.FontSize(9));

                p.Header().Column(col =>
                {
                    col.Item().Text($"Preliquidación período {mes:D2}/{anio}")
                        .FontSize(18).Bold();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                p.Content().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(3);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.2f);
                    });

                    t.Header(h =>
                    {
                        void Hdr(string txt) => h.Cell()
                            .Background(Colors.Grey.Darken3).Padding(4)
                            .Text(txt).FontColor(Colors.White).Bold();
                        Hdr("Legajo"); Hdr("Empleado"); Hdr("Trab.");
                        Hdr("Aus.Just"); Hdr("Aus.Inj"); Hdr("Tard.(min)");
                        Hdr("HE50(min)"); Hdr("HE100(min)"); Hdr("Lic.");
                    });

                    foreach (var r in lista)
                    {
                        t.Cell().Padding(3).Text(r.Legajo);
                        t.Cell().Padding(3).Text(r.NombreCompleto);
                        t.Cell().Padding(3).AlignCenter().Text(r.DiasTrabajados.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.DiasAusenteJustificado.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.DiasAusenteInjustificado.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.MinutosTardanza.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.MinutosHorasExtra50.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.MinutosHorasExtra100.ToString());
                        t.Cell().Padding(3).AlignCenter().Text(r.DiasLicencia.ToString());
                    }
                });

                p.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ").FontSize(8);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" de ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
