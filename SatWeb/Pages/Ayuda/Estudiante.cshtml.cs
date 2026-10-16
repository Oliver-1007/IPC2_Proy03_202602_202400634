using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SatWeb.Pages.Ayuda;

public class EstudianteModel : PageModel
{
    private readonly IConfiguration _config;
    public EstudianteModel(IConfiguration config) => _config = config;
    public string this[string clave] => _config[$"Estudiante:{clave}"] ?? "";
}
