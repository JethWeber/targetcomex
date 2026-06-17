using Microsoft.AspNetCore.Mvc;
using Target.Api.Models;

namespace Target.Api.Controllers.Sms
{
    [ApiController]
    [Route("api/sms")]
    public class SmsController : ControllerBase
    {
        [HttpPost("enviar")]
        public IActionResult Enviar([FromBody] SmsRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Payload inválido para envio de SMS.");

            // Aqui pode ser integrado com um gateway SMS real.
            // Por enquanto, apenas devolvemos o payload recebido para testes.
            return Ok(new { status = "sucesso", to = request.To, message = request.Message });
        }
    }
}
