
using System.Text.Json;
using SatApi.Models;

namespace SatApi.Services
{
    public class ServicioRtu
    {
        private readonly RepositorioDatos _repo;
        public ServicioRtu(RepositorioDatos repo) => _repo = repo;

        public RespuestaRtu Procesar(JsonElement raiz)
        {
            var resp = new RespuestaRtu();
            lock (_repo.Candado)
            {
                foreach (var el in ParserMensajes.Elementos(raiz))
                {
                    string nit = NitValidador.Normalizar(ParserMensajes.Texto(el, "NIT"));
                    string nombre = ParserMensajes.Texto(el, "nombre").Trim();

                    if (!NitValidador.EsValido(nit)) { resp.Invalidos++; continue; }

                    if (_repo.Estado.Contribuyentes.TryGetValue(nit, out var existente))
                    {
                        existente.Nombre = nombre;
                        resp.Actualizados++;
                    }
                    else
                    {
                        _repo.Estado.Contribuyentes[nit] = new Contribuyente { Nit = nit, Nombre = nombre };
                        resp.Nuevos++;
                    }
                }
                _repo.Guardar();
            }
            return resp;
        }

    }
}