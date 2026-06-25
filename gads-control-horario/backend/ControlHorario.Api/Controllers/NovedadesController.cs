using System.Security.Claims;
using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHorario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NovedadesController : ControllerBase
{
    private readonly INovedadRepository _repo;
    public NovedadesController(INovedadRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetByRango(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta,
        [FromQuery] EstadoNovedad? estado = null)
    {
        var lista = await _repo.GetByRangoAsync(desde, hasta);
        if (estado.HasValue) lista = lista.Where(n => n.Estado == estado.Value).ToList();
        return Ok(lista.Select(Map));
    }

    [HttpGet("pendientes")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetPendientes()
    {
        var lista = await _repo.GetPendientesAsync();
        return Ok(lista.Select(Map));
    }

    [HttpGet("empleado/{empleadoId}")]
    public async Task<IActionResult> GetByEmpleado(
        int empleadoId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        if (User.IsInRole("Empleado"))
        {
            var miId = User.FindFirstValue("EmpleadoId");
            if (miId == null || int.Parse(miId) != empleadoId) return Forbid();
        }
        var lista = await _repo.GetByEmpleadoYRangoAsync(empleadoId, desde, hasta);
        return Ok(lista.Select(Map));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] NovedadCreateDto dto)
    {
        var usuarioId = int.Parse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var n = new Novedad
        {
            EmpleadoId = dto.EmpleadoId, Tipo = dto.Tipo,
            FechaDesde = dto.FechaDesde, FechaHasta = dto.FechaHasta,
            Cantidad = dto.Cantidad, Observacion = dto.Observacion,
            Origen = OrigenNovedad.Manual, Estado = EstadoNovedad.Pendiente,
            UsuarioCreadorId = usuarioId
        };
        await _repo.AddAsync(n);
        await _repo.SaveChangesAsync();
        return Ok(new { id = n.Id });
    }

    [HttpPost("{id}/aprobar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Aprobar(long id)
    {
        var n = await _repo.GetByIdAsync(id);
        if (n == null) return NotFound();
        n.Estado = EstadoNovedad.Aprobada;
        n.UsuarioRevisorId = int.Parse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        n.FechaRevision = DateTime.UtcNow;
        await _repo.UpdateAsync(n);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/rechazar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Rechazar(long id, [FromBody] RechazoDto dto)
    {
        var n = await _repo.GetByIdAsync(id);
        if (n == null) return NotFound();
        n.Estado = EstadoNovedad.Rechazada;
        n.MotivoRechazo = dto.Motivo;
        n.UsuarioRevisorId = int.Parse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        n.FechaRevision = DateTime.UtcNow;
        await _repo.UpdateAsync(n);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private static NovedadDto Map(Novedad n) => new(
        n.Id, n.EmpleadoId, n.Empleado?.NombreCompleto ?? "",
        n.Tipo, n.Origen, n.Estado, n.FechaDesde, n.FechaHasta,
        n.Cantidad, n.Observacion, n.FechaCreacion);

    public record RechazoDto(string Motivo);
}
