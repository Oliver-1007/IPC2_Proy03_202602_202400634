using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatWeb.Services;

namespace SatWeb.Pages;

public class InicializarModel : PageModel
{
    private readonly ApiClient _api;
    public InicializarModel(ApiClient api) => _api = api;

    public string? Mensaje { get; private set; }
    public string? Error { get; private set; }

    public async Task OnPostAsync()
    {
        try { await _api.InicializarAsync(); Mensaje = "El sistema quedó sin datos: se borraron el RTU, las facturas y los correlativos."; }
        catch (HttpRequestException) { Error = "No se pudo inicializar el sistema. Verifica que SatApi esté en ejecución."; }
    }
}
