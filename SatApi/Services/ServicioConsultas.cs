using SatApi.Models;

namespace SatApi.Services;

public class ServicioConsultas
{
    private readonly RepositorioDatos _repo;
    public ServicioConsultas(RepositorioDatos repo) => _repo = repo;

    private static string D(DateTime f) => f.ToString("dd/MM/yyyy");

    private List<RegistroFactura> Filtrar(DateTime? desde, DateTime? hasta)
    {
        lock (_repo.Candado)
            return _repo.Estado.Facturas
                .Where(f => (!desde.HasValue || f.Fecha.Date >= desde.Value.Date) &&
                            (!hasta.HasValue || f.Fecha.Date <= hasta.Value.Date))
                .ToList();
    }

    public List<EstadisticaDia> Estadisticas(DateTime? desde, DateTime? hasta) =>
        Filtrar(desde, hasta).GroupBy(f => f.Fecha.Date).OrderBy(g => g.Key).Select(g => new EstadisticaDia
        {
            Fecha = D(g.Key),
            TotalRecibidas = g.Count(),
            NitEmisorInvalido = g.Count(f => f.Errores.Contains(TipoError.NitEmisorInvalido)),
            NitReceptorInvalido = g.Count(f => f.Errores.Contains(TipoError.NitReceptorInvalido)),
            NitEmisorInexistente = g.Count(f => f.Errores.Contains(TipoError.NitEmisorInexistente)),
            NitReceptorInexistente = g.Count(f => f.Errores.Contains(TipoError.NitReceptorInexistente)),
            IvaMalCalculado = g.Count(f => f.Errores.Contains(TipoError.IvaMalCalculado)),
            TotalMalCalculado = g.Count(f => f.Errores.Contains(TipoError.TotalMalCalculado)),
            ReferenciaDuplicada = g.Count(f => f.Errores.Contains(TipoError.ReferenciaDuplicada)),
            SinErrores = g.Count(f => f.Errores.Count == 0),
            Emisores = g.Select(f => f.NitEmisor).Distinct().Count(),
            Receptores = g.Select(f => f.NitReceptor).Distinct().Count()
        }).ToList();

    public ConsultaContribuyente ConsultarContribuyente(string nit, string tipo, DateTime? desde, DateTime? hasta)
    {
        nit = NitValidador.Normalizar(nit);
        bool emitidas = tipo.Equals("emitidas", StringComparison.OrdinalIgnoreCase);

        var lista = Filtrar(desde, hasta)
            .Where(f => f.Aprobada && (emitidas ? f.NitEmisor == nit : f.NitReceptor == nit))
            .OrderBy(f => f.Fecha).ToList();

        string nombre;
        lock (_repo.Candado)
            nombre = _repo.Estado.Contribuyentes.TryGetValue(nit, out var c) ? c.Nombre : "";

        return new ConsultaContribuyente
        {
            Nit = nit, Nombre = nombre, Tipo = emitidas ? "emitidas" : "recibidas",
            Dias = lista.GroupBy(f => f.Fecha.Date)
                        .Select(g => new DiaMonto { Fecha = D(g.Key), Cantidad = g.Count(), Monto = g.Sum(f => f.Total) }).ToList(),
            TotalFacturas = lista.Count,
            TotalMonto = lista.Sum(f => f.Total),
            Facturas = lista.Select(f => new FacturaDto
            {
                Fecha = f.Fecha.ToString("dd/MM/yyyy HH:mm"), Referencia = f.Referencia,
                NitEmisor = f.NitEmisor, NitReceptor = f.NitReceptor,
                Valor = f.Valor, Iva = f.Iva, Total = f.Total, CodigoAprobacion = f.CodigoAprobacion!.Value
            }).ToList()
        };
    }

    public List<ResumenIvaNit> ObtenerResumenIva(DateTime fecha)
    {
        var l = Filtrar(fecha, fecha).Where(f => f.Aprobada).ToList();
        return l.Select(f => f.NitEmisor).Concat(l.Select(f => f.NitReceptor)).Distinct()
                .OrderBy(n => n, StringComparer.Ordinal)
                .Select(n => new ResumenIvaNit
                {
                    Nit = n,
                    IvaEmitido = l.Where(f => f.NitEmisor == n).Sum(f => f.Iva),
                    IvaRecibido = l.Where(f => f.NitReceptor == n).Sum(f => f.Iva)
                }).ToList();
    }

    public ResumenRango ObtenerResumenRango(DateTime desde, DateTime hasta, bool sinIva)
    {
        var dias = Filtrar(desde, hasta).Where(f => f.Aprobada)
            .GroupBy(f => f.Fecha.Date).OrderBy(g => g.Key)
            .Select(g => new DiaValor { Fecha = D(g.Key), Valor = g.Sum(f => sinIva ? f.Valor : f.Total) }).ToList();
        return new ResumenRango { Tipo = sinIva ? "sinIva" : "total", Dias = dias, Total = dias.Sum(x => x.Valor) };
    }
}
