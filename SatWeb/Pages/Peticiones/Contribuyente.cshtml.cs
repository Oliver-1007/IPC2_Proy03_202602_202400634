using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages.Peticiones;

public class ContribuyenteModel : PageModel
{
    private readonly ApiClient _api;
    private readonly PdfService _pdf;
    public ContribuyenteModel(ApiClient api, PdfService pdf) { _api = api; _pdf = pdf; }

    [BindProperty(SupportsGet = true)] public string? Nit { get; set; }
    [BindProperty(SupportsGet = true)] public string Tipo { get; set; } = "emitidas";
    [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }

    public ConsultaContribuyente? Resultado { get; private set; }
    public string? Error { get; private set; }

    private bool Completo => !string.IsNullOrWhiteSpace(Nit) && Desde.HasValue && Hasta.HasValue;

    private async Task CargarAsync()
    {
        try
        {
            Resultado = await _api.ObtenerAsync<ConsultaContribuyente>(
                $"api/consultas/contribuyente?nit={Uri.EscapeDataString(Nit!)}&tipo={Tipo}&desde={ApiClient.F(Desde)}&hasta={ApiClient.F(Hasta)}");
        }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API."; }
    }

    public async Task OnGetAsync() { if (Completo) await CargarAsync(); }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        if (!Completo) return RedirectToPage();
        await CargarAsync();
        if (Resultado == null) return RedirectToPage(new { Nit, Tipo, Desde = ApiClient.F(Desde), Hasta = ApiClient.F(Hasta) });

        var porDia = new PdfService.Seccion
        {
            Titulo = "Facturas autorizadas por fecha", Columnas = new[] { "Fecha", "Cantidad", "Monto total" },
            Filas = Resultado.Dias.Select(d => new[] { d.Fecha, d.Cantidad.ToString(), d.Monto.ToString("N2") }).ToList(),
            Pie = $"Total: {Resultado.TotalFacturas} facturas, Q {Resultado.TotalMonto:N2}"
        };
        var listado = new PdfService.Seccion
        {
            Titulo = "Listado de facturas", Columnas = new[] { "Fecha", "Referencia", "Emisor", "Receptor", "Total", "Autorización" },
            Filas = Resultado.Facturas.Select(f => new[] { f.Fecha, f.Referencia, f.NitEmisor, f.NitReceptor, f.Total.ToString("N2"), f.CodigoAprobacion.ToString() }).ToList()
        };
        var bytes = _pdf.Generar($"Facturas {Resultado.Tipo} · NIT {Resultado.Nit}",
            $"{Resultado.Nombre} · del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}", new[] { porDia, listado });
        return File(bytes, "application/pdf", $"contribuyente_{Resultado.Nit}.pdf");
    }
}
