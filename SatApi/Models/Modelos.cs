
using System.Text.Json.Serialization;

namespace SatApi.Models
{
    public class Contribuyente
    {
        public string Nit { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public static class TipoError
    {
        public const string NitEmisorInvalido = "NIT_EMISOR_INVALIDO";
        public const string NitReceptorInvalido = "NIT_RECEPTOR_INVALIDO";
        public const string NitEmisorInexistente = "NIT_EMISOR_INEXISTENTE";
        public const string NitReceptorInexistente = "NIT_RECEPTOR_INEXISTENTE";
        public const string IvaMalCalculado = "IVA_MAL_CALCULADO";
        public const string TotalMalCalculado = "TOTAL_MAL_CALCULADO";
        public const string ReferenciaDuplicada = "REFERENCIA_DUPLICADA";
    }

    public class RegistroFactura
    {
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; } = "";
        public string NitEmisor { get; set; } = "";
        public string NitReceptor { get; set; } = "";
        public decimal Valor { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public long? CodigoAprobacion { get; set; }
        public List<string> Errores { get; set; } = new();

        [JsonIgnore]
        public bool Aprobada => CodigoAprobacion.HasValue;
    }

    public class EstadoSistema
    {
        public Dictionary<string, Contribuyente> Contribuyentes { get; set; } = new();
        public List<RegistroFactura> Facturas { get; set; } = new();
        public Dictionary<string, int> Correlativos { get; set; } = new();
    }

}