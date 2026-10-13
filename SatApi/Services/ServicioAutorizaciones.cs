using System.Text.Json;
using SatApi.Models;

namespace SatApi.Services;

public class ServicioAutorizaciones
{
    private readonly RepositorioDatos _repo;
    public ServicioAutorizaciones(RepositorioDatos repo) => _repo = repo;

    public RespuestaAutorizaciones Procesar(JsonElement raiz)
    {
        var resp = new RespuestaAutorizaciones();
        var r = resp.Resultado;

        lock (_repo.Candado)
        {
            DateTime ahora = DateTime.Now;
            string clave = ahora.ToString("yyyyMMdd");

            foreach (var el in ParserMensajes.Elementos(raiz))
            {
                if (!ParserMensajes.TryFecha(ParserMensajes.Texto(el, "TIEMPO"), out var fecha)) continue;
                if (!ParserMensajes.TryDecimal(el, "VALOR", out var valor) ||
                    !ParserMensajes.TryDecimal(el, "IVA", out var iva) ||
                    !ParserMensajes.TryDecimal(el, "TOTAL", out var total)) continue;

                string referencia = ParserMensajes.Texto(el, "REFERENCIA").Trim();
                if (referencia.Length == 0) continue;
                if (referencia.Length > 40) referencia = referencia[..40];

                var reg = new RegistroFactura
                {
                    Fecha = fecha, Referencia = referencia,
                    NitEmisor = NitValidador.Normalizar(ParserMensajes.Texto(el, "NIT_EMISOR")),
                    NitReceptor = NitValidador.Normalizar(ParserMensajes.Texto(el, "NIT_RECEPTOR")),
                    Valor = valor, Iva = iva, Total = total
                };

                Validar(reg);

                if (reg.Errores.Contains(TipoError.NitEmisorInexistente)) r.RechazadasEmisor++;
                if (reg.Errores.Contains(TipoError.NitReceptorInexistente)) r.RechazadasReceptor++;

                if (reg.Errores.Count == 0)
                {
                    int n = _repo.Estado.Correlativos.GetValueOrDefault(clave) + 1;
                    _repo.Estado.Correlativos[clave] = n;
                    reg.CodigoAprobacion = long.Parse($"{clave}{n:D8}");
                    _repo.ReferenciasAprobadas.Add(reg.Referencia);
                    r.Aprobadas++;
                    r.Detalle.Add(new DetalleAprobacion { Referencia = reg.Referencia, Codigo = reg.CodigoAprobacion.Value });
                }
                _repo.Estado.Facturas.Add(reg);
            }
            _repo.Guardar();
        }
        return resp;
    }

    private void Validar(RegistroFactura f)
    {
        var contribuyentes = _repo.Estado.Contribuyentes;

        if (!NitValidador.EsValido(f.NitEmisor)) f.Errores.Add(TipoError.NitEmisorInvalido);
        else if (!contribuyentes.ContainsKey(f.NitEmisor)) f.Errores.Add(TipoError.NitEmisorInexistente);

        if (!NitValidador.EsValido(f.NitReceptor)) f.Errores.Add(TipoError.NitReceptorInvalido);
        else if (!contribuyentes.ContainsKey(f.NitReceptor)) f.Errores.Add(TipoError.NitReceptorInexistente);

        if (Math.Round(f.Valor * 0.12m, 2, MidpointRounding.AwayFromZero) != f.Iva) f.Errores.Add(TipoError.IvaMalCalculado);
        if (f.Valor + f.Iva != f.Total) f.Errores.Add(TipoError.TotalMalCalculado);
        if (_repo.ReferenciasAprobadas.Contains(f.Referencia)) f.Errores.Add(TipoError.ReferenciaDuplicada);
    }
}
