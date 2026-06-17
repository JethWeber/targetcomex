using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Target.Api.Data;
using Target.Api.Models;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var reservas = _context.Reservas.OrderByDescending(r => r.DataReserva).ToList();
        return Ok(reservas);
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public IActionResult GetPorUsuario(int usuarioId)
    {
        var reservas = _context.Reservas
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.DataReserva)
            .ToList();

        return Ok(reservas);
    }

    [HttpPost]
    public IActionResult CriarReserva([FromBody] ReservaRequest request)
    {
        if (request == null || request.UsuarioId <= 0 || request.VeiculoId <= 0)
            return BadRequest("Dados de reserva inválidos.");

        var reserva = new Reserva
        {
            UsuarioId = request.UsuarioId,
            VeiculoId = request.VeiculoId,
            TipoPedido = request.TipoPedido,
            Showroom = request.Showroom,
            Estado = "Pendente",
            DataReserva = DateTime.UtcNow
        };

        _context.Reservas.Add(reserva);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetPorUsuario), new { usuarioId = reserva.UsuarioId }, reserva);
    }

    [HttpPut("{id:int}/estado")]
    public IActionResult AtualizarEstado(int id, [FromBody] string novoEstado)
    {
        var reserva = _context.Reservas.Find(id);
        if (reserva == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(novoEstado))
            return BadRequest("Estado inválido.");

        reserva.Estado = novoEstado.Trim();
        _context.SaveChanges();

        return Ok(reserva);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var reserva = _context.Reservas.Find(id);
        if (reserva == null)
            return NotFound();

        _context.Reservas.Remove(reserva);
        _context.SaveChanges();
        return Ok("Reserva removida com sucesso");
    }
}
