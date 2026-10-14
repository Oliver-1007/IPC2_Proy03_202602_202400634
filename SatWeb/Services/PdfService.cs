using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SatWeb.Services;

public class PdfService
{
    public class Seccion
    {
        public string Titulo { get; set; } = "";
        public string[] Columnas { get; set; } = Array.Empty<string>();
        public List<string[]> Filas { get; set; } = new();
        public string? Pie { get; set; }
    }

    public PdfService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generar(string titulo, string subtitulo, IEnumerable<Seccion> secciones, bool horizontal = false) =>
        Document.Create(doc => doc.Page(p =>
        {
            p.Size(horizontal ? PageSizes.A4.Landscape() : PageSizes.A4);
            p.Margin(36);
            p.DefaultTextStyle(t => t.FontSize(9));
            p.Header().Column(c =>
            {
                c.Item().Text(titulo).FontSize(16).Bold();
                c.Item().Text(subtitulo).FontColor(Colors.Grey.Darken2);
            });
            p.Content().PaddingVertical(12).Column(col =>
            {
                foreach (var s in secciones)
                {
                    col.Item().PaddingTop(10).Text(s.Titulo).FontSize(12).Bold();
                    if (s.Filas.Count == 0) { col.Item().Text("Sin datos para los filtros indicados."); continue; }
                    col.Item().PaddingTop(4).Table(t =>
                    {
                        t.ColumnsDefinition(cd => { foreach (var _ in s.Columnas) cd.RelativeColumn(); });
                        t.Header(h => { foreach (var c in s.Columnas) h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(c).Bold(); });
                        foreach (var fila in s.Filas)
                            foreach (var v in fila)
                                t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(v);
                    });
                    if (s.Pie != null) col.Item().PaddingTop(4).Text(s.Pie).Bold();
                }
            });
            p.Footer().AlignCenter().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
        })).GeneratePdf();
}
