
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CapaEntidadBM;
using System.Collections.Generic;
using Utilitarios;
using System.Linq;
using System;
using CapaNegocioBM;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApiProveedorPagos.Controllers
{
    [Authorize]
    [Route("api/Envios")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ApiEnvios")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class EnviosController : Controller
    {

        public static string GetIdentityName(string token)
        {
            
            var parametro = ParametroCN.ObtenerParametroPorNombre("fraseSecreta");
            string secret = parametro.pa_valor_string;
            var key = Encoding.ASCII.GetBytes(secret);
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            return claims.Identity.Name;
        }

        /// <summary>
        /// Consultar valor a pagar de tasa SPE
        /// </summary>
        /// <param name="codigo_envio"> Código de envío a consultar</param>
        /// <returns></returns>
        [HttpGet("ConsultarValorPagar")]
        [ProducesResponseType(201, Type = typeof(RespuestaDTO))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult ConsultarValorPagar(string codigo_envio)
        {
            try
            {
                ResultadoValorCobrarDTO oResultado = LogicaPreAlerta.FnConsultarValorAPagar(codigo_envio);
                return Ok(oResultado);
            }
            catch (Exception ex)
            {
                ResultadoValorCobrarDTO oResultadoError = new ResultadoValorCobrarDTO();
                oResultadoError.Codigo_resultado = "020";
                oResultadoError.Mensaje = ex.Message;
                return StatusCode(500, oResultadoError);

            }

        }

        /// <summary>
        /// Reversar Pagos realizados por Terceros
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Resultado))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("ReversoPagoTerceros")]
        public IActionResult ReversoPagoTerceros([FromBody] RegistrarPagoDTO Pago)
        {
            try
            {
                var authHeader = Request.Headers.Authorization;
                authHeader = authHeader.ToString().Replace("Bearer ", "");
                var strUsuarioToken = GetIdentityName(authHeader);

                RegistrarPagoProveedorDTO PagoDto = new RegistrarPagoProveedorDTO();
                PagoDto.Codigo_envio = Pago.Codigo_envio;
                PagoDto.Fecha_pago = Pago.Fecha_pago;
                PagoDto.Secuencial = Pago.Secuencial;
                PagoDto.Total = Pago.Total;
                PagoDto.Proveedor = strUsuarioToken;

                Registro_pago_tercero_dto oRegistro = LogicaPreAlerta.FnReversoPagoTecero(PagoDto);
                return Ok(oRegistro);
            }
            catch (Exception ex)
            {

                Registro_pago_tercero_dto oResultadoError = new Registro_pago_tercero_dto();
                oResultadoError.CodigoResultado = "020";
                oResultadoError.Mensaje = ex.Message;
                return StatusCode(500, oResultadoError);
            }

        }

        /// <summary>
        /// Consultar Pago realizados por Terceros
        /// </summary>
        /// <param name="fechaPago"> Fecha de Pago</param>
        /// <returns></returns>
        [HttpGet("ConsultarPagosTeceros")]
        public IActionResult ConsultarPagosTeceros(DateTime fechaPago)
        {
            try
            {
                var authHeader = Request.Headers.Authorization;
                authHeader = authHeader.ToString().Replace("Bearer ", "");
                var strUsuarioToken = GetIdentityName(authHeader);

                List<Reporte_pago_dto> oResultado = LogicaPreAlerta.FnConsultarPagosTercero(fechaPago, strUsuarioToken).ToList();
                return Ok(oResultado);
            }
            catch (Exception ex)
            {
                Registro_pago_tercero_dto oResultadoError = new Registro_pago_tercero_dto();
                oResultadoError.CodigoResultado = "020";
                oResultadoError.Mensaje = ex.Message;
                return StatusCode(500, oResultadoError);
            }

        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Resultado))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("RegistroLoteTransaccionesPagosTerceros")]
        public IActionResult RegistroLoteTransaccionesPagosTerceros([FromBody] List<PagoConciliacionDTO> Pagos)
        {
            Registro_pago_tercero_dto Resultado = new Registro_pago_tercero_dto();
            try
            {
                var authHeader = Request.Headers.Authorization;
                authHeader = authHeader.ToString().Replace("Bearer ", "");
                var strUsuarioToken = GetIdentityName(authHeader);

                Resultado = PagoConciliacionCN.RegistroLoteTransaccionesPagosTerceros(Pagos, strUsuarioToken);
                return Ok(Resultado);
            }
            catch (Exception ex)
            {
                Registro_pago_tercero_dto oResultadoError = new Registro_pago_tercero_dto();
                oResultadoError.CodigoResultado = "020";
                oResultadoError.Mensaje = ex.Message;
                return StatusCode(500, oResultadoError);
            }

        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Resultado))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("RegistroPagoTerceros")]
        public IActionResult RegistroPagoTerceros([FromBody] RegistrarPagoDTO Pago)
        {

            try
            {
                var authHeader = Request.Headers.Authorization;
                authHeader = authHeader.ToString().Replace("Bearer ", "");
                var strUsuarioToken = GetIdentityName(authHeader);

                RegistrarPagoProveedorDTO PagoDto = new RegistrarPagoProveedorDTO();
                PagoDto.Codigo_envio = Pago.Codigo_envio;
                PagoDto.Fecha_pago = Pago.Fecha_pago;
                PagoDto.Secuencial = Pago.Secuencial;
                PagoDto.Total = Pago.Total;
                PagoDto.Proveedor = strUsuarioToken;
                PasarelaPagoDTO pasarelaoPago = PasarelaPagoCN.FnGetPasarelaPago(PagoDto.Proveedor, 3).FirstOrDefault();
                if(pasarelaoPago != null)
                {
                    av_log_json_facturacion av_Log_Json = new av_log_json_facturacion();
                    av_Log_Json.fec_creacion = DateTime.Now;
                    av_Log_Json.psp_id = pasarelaoPago.psp_id;
                    av_Log_Json.ljs_json = JsonSerializer.Serialize(PagoDto);
                    LogJSONCN.Registrar(av_Log_Json);
                }
               

                Registro_pago_tercero_dto oRegistro = LogicaPreAlerta.FnRegistrarPago(PagoDto);
                
                return Ok(oRegistro);
            }
            catch (Exception ex)
            {
                Registro_pago_tercero_dto oResultadoError = new Registro_pago_tercero_dto();
                oResultadoError.CodigoResultado = "020";
                oResultadoError.Mensaje = ex.Message;
                return StatusCode(500, oResultadoError);
            }
        }
    }
}