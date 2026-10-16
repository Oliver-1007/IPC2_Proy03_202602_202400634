using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages.Peticiones;

public class ResumenRangoModel : PageModel
{
    private readonly ApiClient _api;
    private readonly PdfService _pdf;
    public ResumenRangoModel(ApiClient api, PdfService pdf) { _api = api; _pdf = pdf; }

    [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }
    [BindProperty(SupportsGet = true)] public string Modo { get; set; } = "total";
    public ResumenRango? Resultado { get; private set; }
    public string? Error { get; private set; }

    private async Task CargarAsync()
    {
        try { Resultado = await _api.ObtenerAsync<ResumenRango>($"api/consultas/rango?desde={ApiClient.F(Desde)}&hasta={ApiClient.F(Hasta)}&tipo={Modo}"); }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API."; }
    }

    public async Task OnGetAsync() { if (Desde.HasValue && Hasta.HasValue) await CargarAsync(); }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        if (!Desde.HasValue || !Hasta.HasValue) return RedirectToPage();
        await CargarAsync();
        if (Resultado == null) return RedirectToPage(new { Desde = ApiClient.F(Desde), Hasta = ApiClient.F(Hasta), Modo });
        var etiqueta = Modo == "sinIva" ? "Valor sin IVA" : "Valor total";
        var s = new PdfService.Seccion
        {
            Titulo = etiqueta + " autorizado por fecha", Columnas = new[] { "Fecha", etiqueta },
            Filas = Resultado.Dias.Select(d => new[] { d.Fecha, d.Valor.ToString("N2") }).ToList(),
            Pie = $"Total del rango: Q {Resultado.Total:N2}"
        };
        return File(_pdf.Generar("Resumen por rango de fechas", $"Del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}", new[] { s }), "application/pdf", "resumen_rango.pdf");
    }
}
