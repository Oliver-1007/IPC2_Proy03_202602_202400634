using System.Text.RegularExpressions;

namespace SatApi.Services
{
    public static class NitValidador
    {
        private static readonly Regex Formato = new(@"^\d{1,20}[0-9K]$", RegexOptions.Compiled);

        public static string Normalizar(string? nit) => (nit ?? "").Trim().ToUpperInvariant();

        public static bool EsValido(string? nit)
        {
            nit = Normalizar(nit);
            if (!Formato.IsMatch(nit)) return false;

            string cuerpo = nit[..^1];
            char verificador = nit[^1];

            int suma = 0, peso = cuerpo.Length + 1;
            foreach (char c in cuerpo) { suma += (c - '0') * peso; peso--; }

            int resultado = (11 - suma % 11) % 11;
            char esperado = resultado == 10 ? 'K' : (char)('0' + resultado);
            return verificador == esperado;
        }
    }
   
}