using System.Text;
using System.Text.Json;

namespace SatWeb.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http) => _http = http;

    public static string F(DateTime? fecha) => fecha?.ToString("yyyy-MM-dd") ?? "";

    public static T? Leer<T>(string json) => JsonSerializer.Deserialize<T>(json, Opciones);

    public async Task<(bool Ok, string Cuerpo)> EnviarAsync(string ruta, string json)
    {
        var resp = await _http.PostAsync(ruta, new StringContent(json, Encoding.UTF8, "application/json"));
        return (resp.IsSuccessStatusCode, await resp.Content.ReadAsStringAsync());
    }

    public Task<T?> ObtenerAsync<T>(string ruta) => _http.GetFromJsonAsync<T>(ruta, Opciones);

    public async Task InicializarAsync() =>
        (await _http.PostAsync("api/sistema/inicializar", null)).EnsureSuccessStatusCode();
}
