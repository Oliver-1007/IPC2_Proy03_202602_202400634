using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SatApi.Models;
using SatWeb.Services;

namespace SatWeb.Pages;

public class EnviarModel : PageModel
{
    private readonly ApiClient _api;
    public EnviarModel(ApiClient api) => _api = api;

    [BindProperty] public IFormFile? Archivo { get; set; }
    [BindProperty] public string Tipo { get; set; } = "autorizaciones";

    public string? Contenido { get; private set; }
    public int Solicitudes { get; private set; }
    public string? Respuesta { get; private set; }
    public string? Error { get; private set; }
    public RespuestaRtu? Rtu { get; private set; }
    public RespuestaAutorizaciones? Autorizaciones { get; private set; }

    public async Task OnPostAsync()
    {
        if (Archivo == null || Archivo.Length == 0) { Error = "Selecciona un archivo .json antes de enviar."; return; }

        using var lector = new StreamReader(Archivo.OpenReadStream(), Encoding.UTF8);
        Contenido = await lector.ReadToEndAsync();
        try
        {
            using var doc = JsonDocument.Parse(Contenido);
            Solicitudes = doc.RootElement.ValueKind == JsonValueKind.Array ? doc.RootElement.GetArrayLength() : 1;
        }
        catch (JsonException) { Error = "El archivo no contiene un JSON válido."; return; }

        try
        {
            var (ok, cuerpo) = await _api.EnviarAsync(Tipo == "rtu" ? "api/rtu" : "api/autorizaciones", Contenido);
            if (!ok) { Error = $"La API rechazó el mensaje: {cuerpo}"; return; }

            Respuesta = JsonSerializer.Serialize(JsonDocument.Parse(cuerpo).RootElement,
                new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            if (Tipo == "rtu") Rtu = ApiClient.Leer<RespuestaRtu>(cuerpo);
            else Autorizaciones = ApiClient.Leer<RespuestaAutorizaciones>(cuerpo);
        }
        catch (HttpRequestException) { Error = "No se pudo conectar con la API. Inicia el proyecto SatApi (http://localhost:5100)."; }
    }
}
