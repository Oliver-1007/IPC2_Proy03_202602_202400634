using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages.Peticiones;

public class EstadisticasModel : PageModel
{
    private readonly ApiClient _api;
    private readonly PdfService _pdf;
    public EstadisticasModel(ApiClient api, PdfService pdf) { _api = api; _pdf = pdf; }

    [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }
    public List<EstadisticaDia> Dias { get; private set; } = new();
    public string? Error { get; private set; }

    private async Task CargarAsync()
    {
        try { Dias = await _api.ObtenerAsync<List<EstadisticaDia>>($"api/consultas/estadisticas?desde={ApiClient.F(Desde)}&hasta={ApiClient.F(Hasta)}") ?? new(); }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API."; }
    }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnGetPdfAsync()
    {
        await CargarAsync();
        var s = new PdfService.Seccion
        {
            Titulo = "Facturas recibidas y errores por fecha",
            Columnas = new[] { "Fecha", "Recibidas", "Emisor inválido", "Receptor inválido", "Emisor inexistente", "Receptor inexistente", "IVA mal", "Total mal", "Ref. duplicada", "Sin errores", "Emisores", "Receptores" },
            Filas = Dias.Select(d => new[] { d.Fecha, d.TotalRecibidas.ToString(), d.NitEmisorInvalido.ToString(), d.NitReceptorInvalido.ToString(),
                d.NitEmisorInexistente.ToString(), d.NitReceptorInexistente.ToString(), d.IvaMalCalculado.ToString(), d.TotalMalCalculado.ToString(),
                d.ReferenciaDuplicada.ToString(), d.SinErrores.ToString(), d.Emisores.ToString(), d.Receptores.ToString() }).ToList()
        };
        string rango = Desde.HasValue && Hasta.HasValue ? $"Del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}" : "Todas las fechas";
        return File(_pdf.Generar("Estadísticas de facturas", rango, new[] { s }, horizontal: true), "application/pdf", "estadisticas.pdf");
    }
}
