using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SatApi.Services
{
    public static class ParserMensajes
    {
        // Ignora lugar u otro texto: solo interesa dd/mm/yyyy hh24:mi
        private static readonly Regex RxTiempo = new(
            @"(?<d>\d{1,2})/(?<m>\d{1,2})/(?<y>\d{4})\s+(?<h>\d{1,2}):(?<mi>\d{2})", RegexOptions.Compiled);

        public static bool TryFecha(string? texto, out DateTime fecha)
        {
            fecha = default;
            var m = RxTiempo.Match(texto ?? "");
            if (!m.Success) return false;
            try
            {
                fecha = new DateTime(int.Parse(m.Groups["y"].Value), int.Parse(m.Groups["m"].Value),
                    int.Parse(m.Groups["d"].Value), int.Parse(m.Groups["h"].Value), int.Parse(m.Groups["mi"].Value), 0);
                return true;
            }
            catch (ArgumentOutOfRangeException) { return false; }
        }

        public static IEnumerable<JsonElement> Elementos(JsonElement raiz) => raiz.ValueKind switch
        {
            JsonValueKind.Array => raiz.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Object),
            JsonValueKind.Object => new[] { raiz },
            _ => Enumerable.Empty<JsonElement>()
        };

        private static bool TryPropiedad(JsonElement e, string nombre, out JsonElement valor)
        {
            foreach (var p in e.EnumerateObject())
                if (p.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase)) { valor = p.Value; return true; }
            valor = default;
            return false;
        }

        public static string Texto(JsonElement e, string nombre)
        {
            if (!TryPropiedad(e, nombre, out var v)) return "";
            return v.ValueKind switch
            {
                JsonValueKind.String => v.GetString() ?? "",
                JsonValueKind.Number => v.GetRawText(),
                _ => ""
            };
        }

        public static bool TryDecimal(JsonElement e, string nombre, out decimal valor)
        {
            valor = 0;
            if (!TryPropiedad(e, nombre, out var v)) return false;
            if (v.ValueKind == JsonValueKind.Number) return v.TryGetDecimal(out valor);
            return v.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(v.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }
    }
}
