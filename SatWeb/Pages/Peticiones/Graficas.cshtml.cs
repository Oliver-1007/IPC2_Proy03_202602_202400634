using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages.Peticiones;

public class GraficasModel : PageModel
{
    private readonly ApiClient _api;
    private readonly PdfService _pdf;
    public GraficasModel(ApiClient api, PdfService pdf) { _api = api; _pdf = pdf; }

    [BindProperty(SupportsGet = true)] public DateTime? Fecha { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }
    [BindProperty(SupportsGet = true)] public string Modo { get; set; } = "total";

    public List<ResumenIvaNit> Iva { get; private set; } = new();
    public ResumenRango? Rango { get; private set; }
    public string? Error { get; private set; }
    public string IvaJson => JsonSerializer.Serialize(Iva);
    public string RangoJson => JsonSerializer.Serialize(Rango?.Dias ?? new List<DiaValor>());

    private async Task CargarAsync()
    {
        try
        {
            if (Fecha.HasValue) Iva = await _api.ObtenerAsync<List<ResumenIvaNit>>($"api/consultas/iva?fecha={ApiClient.F(Fecha)}") ?? new();
            if (Desde.HasValue && Hasta.HasValue)
                Rango = await _api.ObtenerAsync<ResumenRango>($"api/consultas/rango?desde={ApiClient.F(Desde)}&hasta={ApiClient.F(Hasta)}&tipo={Modo}");
        }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API."; }
    }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnGetPdfAsync()
    {
        await CargarAsync();
        var secciones = new List<PdfService.Seccion>();
        if (Fecha.HasValue)
            secciones.Add(new PdfService.Seccion
            {
                Titulo = $"IVA por NIT · {Fecha:dd/MM/yyyy}", Columnas = new[] { "NIT", "IVA emitido", "IVA recibido" },
                Filas = Iva.Select(x => new[] { x.Nit, x.IvaEmitido.ToString("N2"), x.IvaRecibido.ToString("N2") }).ToList()
            });
        if (Rango != null)
            secciones.Add(new PdfService.Seccion
            {
                Titulo = $"Valor {(Modo == "sinIva" ? "sin IVA" : "total")} por fecha", Columnas = new[] { "Fecha", "Valor" },
                Filas = Rango.Dias.Select(d => new[] { d.Fecha, d.Valor.ToString("N2") }).ToList(),
                Pie = $"Total del rango: Q {Rango.Total:N2}"
            });
        if (secciones.Count == 0) return RedirectToPage();
        return File(_pdf.Generar("Datos de las gráficas", "Resumen de IVA y resumen por rango de fechas", secciones), "application/pdf", "graficas.pdf");
    }
}
