
using System.Text.Json.Serialization;

namespace SatApi.Models
{
    public class RespuestaRtu
    {
        [JsonPropertyName("CONTRIBUYENTES_NUEVOS")] public int Nuevos { get; set; }
        [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")] public int Actualizados { get; set; }
        [JsonPropertyName("NITS_INVALIDOS")] public int Invalidos { get; set; }
    }

    public class RespuestaAutorizaciones
    {
        [JsonPropertyName("resultadoAutorizaciones")]
        public ResultadoAutorizaciones Resultado { get; set; } = new();
    }

    public class ResultadoAutorizaciones
    {
        [JsonPropertyName("cantidadAutorizacionesAprobadas")] public int Aprobadas { get; set; }
        [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")] public int RechazadasEmisor { get; set; }
        [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")] public int RechazadasReceptor { get; set; }
        [JsonPropertyName("detalleAprobaciones")] public List<DetalleAprobacion> Detalle { get; set; } = new();
    }

    public class DetalleAprobacion
    {
        [JsonPropertyName("referencia")] public string Referencia { get; set; } = "";
        [JsonPropertyName("codigoAprobacion")] public long Codigo { get; set; }
    }

    // ---- DTOs de consultas ----
    public class EstadisticaDia
    {
        public string Fecha { get; set; } = "";
        public int TotalRecibidas { get; set; }
        public int NitEmisorInvalido { get; set; }
        public int NitReceptorInvalido { get; set; }
        public int NitEmisorInexistente { get; set; }
        public int NitReceptorInexistente { get; set; }
        public int IvaMalCalculado { get; set; }
        public int TotalMalCalculado { get; set; }
        public int ReferenciaDuplicada { get; set; }
        public int SinErrores { get; set; }
        public int Emisores { get; set; }
        public int Receptores { get; set; }
    }

    public class DiaMonto { public string Fecha { get; set; } = ""; public int Cantidad { get; set; } public decimal Monto { get; set; } }

    public class FacturaDto
    {
        public string Fecha { get; set; } = "";
        public string Referencia { get; set; } = "";
        public string NitEmisor { get; set; } = "";
        public string NitReceptor { get; set; } = "";
        public decimal Valor { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public long CodigoAprobacion { get; set; }
    }

    public class ConsultaContribuyente
    {
        public string Nit { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public List<DiaMonto> Dias { get; set; } = new();
        public int TotalFacturas { get; set; }
        public decimal TotalMonto { get; set; }
        public List<FacturaDto> Facturas { get; set; } = new();
    }

    public class ResumenIvaNit { public string Nit { get; set; } = ""; public decimal IvaEmitido { get; set; } public decimal IvaRecibido { get; set; } }
    public class DiaValor { public string Fecha { get; set; } = ""; public decimal Valor { get; set; } }
    public class ResumenRango { public string Tipo { get; set; } = ""; public List<DiaValor> Dias { get; set; } = new(); public decimal Total { get; set; } }

}