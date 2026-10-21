using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SatApi.Services;

namespace SatApi.ApiControllers
{
    [ApiController]
    [Route("api/rtu")]
    public class RtuController : ControllerBase
    {
        private readonly ServicioRtu _servicio;
        public RtuController(ServicioRtu servicio) => _servicio = servicio;

        [HttpPost]
        public async Task<IActionResult> Solicitar()
        {
            using var lector = new StreamReader(Request.Body);
            if (!ParserMensajes.TryRaiz(await lector.ReadToEndAsync(), out var raiz, out var error))
                return BadRequest(new { error });
            return Ok(_servicio.Procesar(raiz));
        }

        [HttpGet]
        public IActionResult Listar() => Ok(_servicio.Listar());
    }

    [ApiController]
    [Route("api/autorizaciones")]
    public class AutorizacionesController : ControllerBase
    {
        private readonly ServicioAutorizaciones _servicio;
        public AutorizacionesController(ServicioAutorizaciones servicio) => _servicio = servicio;

        [HttpPost]
        public async Task<IActionResult> Solicitar()
        {
            using var lector = new StreamReader(Request.Body);
            if (!ParserMensajes.TryRaiz(await lector.ReadToEndAsync(), out var raiz, out var error))
                return BadRequest(new { error });

            var resp = _servicio.Procesar(raiz);
            var r = resp.Resultado;
            if (r.Aprobadas == 0 && r.Rechazadas == 0 && r.DescartadasPorFormato > 0)
                return BadRequest(new { error = "Ninguna solicitud del mensaje tiene el formato esperado (TIEMPO, REFERENCIA, NIT_EMISOR, NIT_RECEPTOR, VALOR, IVA, TOTAL)." });
            return Ok(resp);
        }
    }

    [ApiController]
    [Route("api/consultas")]
    public class ConsultasController : ControllerBase
    {
        private readonly ServicioConsultas _servicio;
        public ConsultasController(ServicioConsultas servicio) => _servicio = servicio;

        [HttpGet("estadisticas")]
        public IActionResult Estadisticas(DateTime? desde, DateTime? hasta) => Ok(_servicio.Estadisticas(desde, hasta));

        [HttpGet("contribuyente")]
        public IActionResult Contribuyente(string nit, string tipo, DateTime? desde, DateTime? hasta) =>
            Ok(_servicio.ConsultarContribuyente(nit, tipo, desde, hasta));

        [HttpGet("iva")]
        public IActionResult Iva(DateTime fecha) => Ok(_servicio.ObtenerResumenIva(fecha));

        [HttpGet("rango")]
        public IActionResult Rango(DateTime desde, DateTime hasta, string tipo = "total") =>
            Ok(_servicio.ObtenerResumenRango(desde, hasta, tipo.Equals("sinIva", StringComparison.OrdinalIgnoreCase)));
    }

    [ApiController]
    [Route("api/sistema")]
    public class SistemaController : ControllerBase
    {
        private readonly RepositorioDatos _repo;
        public SistemaController(RepositorioDatos repo) => _repo = repo;

        [HttpPost("inicializar")]
        public IActionResult Inicializar()
        {
            _repo.Reiniciar();
            return Ok(new { mensaje = "Sistema inicializado sin datos." });
        }
    }
}