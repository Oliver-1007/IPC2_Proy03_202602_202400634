
using System.Text.Json;
using SatApi.Models;

namespace SatApi.Services
{
    public class RepositorioDatos
    {
        private readonly string _ruta;
        public readonly object Candado = new();
        public EstadoSistema Estado { get; private set; } = new();
        public HashSet<string> ReferenciasAprobadas { get; } = new();

        public RepositorioDatos(IWebHostEnvironment env)
        {
            string dir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dir);
            _ruta = Path.Combine(dir, "estado.json");
            Cargar();
        }

        private void Cargar()
        {
            if (File.Exists(_ruta))
            {
                try { Estado = JsonSerializer.Deserialize<EstadoSistema>(File.ReadAllText(_ruta)) ?? new(); }
                catch (JsonException) { Estado = new(); }
            }
            ReferenciasAprobadas.Clear();
            foreach (var f in Estado.Facturas.Where(f => f.Aprobada)) ReferenciasAprobadas.Add(f.Referencia);
        }

        public void Guardar() => File.WriteAllText(_ruta, JsonSerializer.Serialize(Estado));

        public void Reiniciar()
        {
            lock (Candado)
            {
                Estado = new();
                ReferenciasAprobadas.Clear();
                Guardar();
            }
        }
    }
}