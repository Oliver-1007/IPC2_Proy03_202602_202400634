using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages.Peticiones;

public class ResumenIvaModel : PageModel
{
    private readonly ApiClient _api;
    private readonly PdfService _pdf;
    public ResumenIvaModel(ApiClient api, PdfService pdf) { _api = api; _pdf = pdf; }

    [BindProperty(SupportsGet = true)] public DateTime? Fecha { get; set; }
    public List<ResumenIvaNit>? Resultado { get; private set; }
    public string? Error { get; private set; }

    private async Task CargarAsync()
    {
        try { Resultado = await _api.ObtenerAsync<List<ResumenIvaNit>>($"api/consultas/iva?fecha={ApiClient.F(Fecha)}"); }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API."; }
    }

    public async Task OnGetAsync() { if (Fecha.HasValue) await CargarAsync(); }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        if (!Fecha.HasValue) return RedirectToPage();
        await CargarAsync();
        if (Resultado == null) return RedirectToPage(new { Fecha = ApiClient.F(Fecha) });
        var s = new PdfService.Seccion
        {
            Titulo = "IVA por NIT", Columnas = new[] { "NIT", "IVA emitido", "IVA recibido" },
            Filas = Resultado.Select(x => new[] { x.Nit, x.IvaEmitido.ToString("N2"), x.IvaRecibido.ToString("N2") }).ToList(),
            Pie = $"Totales: emitido Q {Resultado.Sum(x => x.IvaEmitido):N2} · recibido Q {Resultado.Sum(x => x.IvaRecibido):N2}"
        };
        return File(_pdf.Generar("Resumen de IVA por fecha y NIT", $"Fecha: {Fecha:dd/MM/yyyy}", new[] { s }), "application/pdf", "resumen_iva.pdf");
    }
}
