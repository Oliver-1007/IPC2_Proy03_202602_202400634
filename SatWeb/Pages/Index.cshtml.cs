using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<EstadisticaDia>? Dias { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Dias = await _api.ObtenerAsync<List<EstadisticaDia>>("api/consultas/estadisticas"); }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API. Inicia el proyecto SatApi (http://localhost:5100)."; }
    }
}
