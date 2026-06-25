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
public class FichadasController : ControllerBase
{
    private readonly IFichadaRepository _repo;
    public FichadasController(IFichadaRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> GetByRango(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var lista = await _repo.GetByRangoAsync(desde, hasta);
        return Ok(lista.Select(Map));
    }

    [HttpGet("empleado/{empleadoId}")]
    public async Task<IActionResult> GetByEmpleado(
        int empleadoId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        // Empleado solo puede ver lo suyo
        if (User.IsInRole("Empleado"))
        {
            var miId = User.FindFirstValue("EmpleadoId");
            if (miId == null || int.Parse(miId) != empleadoId) return Forbid();
        }
        var lista = await _repo.GetByEmpleadoYRangoAsync(empleadoId, desde, hasta);
        return Ok(lista.Select(Map));
    }

    /// <summary>
    /// Crea una fichada. Empleados pueden crear solo su propia fichada (entrada/salida).
    /// Admins pueden cargar fichadas manuales por cualquier empleado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] FichadaCreateDto dto)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub")!);

        if (rol == "Empleado")
        {
            var miEmpId = User.FindFirstValue("EmpleadoId");
            if (miEmpId == null || int.Parse(miEmpId) != dto.EmpleadoId) return Forbid();
        }

        var f = new Fichada
        {
            EmpleadoId = dto.EmpleadoId,
            Timestamp = dto.Timestamp,
            Tipo = dto.Tipo,
            Origen = dto.Origen,
            Observacion = dto.Observacion,
            UsuarioRegistroId = (dto.Origen == OrigenFichada.Manual) ? usuarioId : null
        };
        await _repo.AddAsync(f);
        await _repo.SaveChangesAsync();
        return Ok(new { id = f.Id });
    }

    /// <summary>
    /// Endpoint para que dispositivos externos (relojes biométricos, apps de QR) envíen fichadas.
    /// Solo Administrador (idealmente con API key separada en producción).
    /// </summary>
    [HttpPost("api-externa")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearDesdeApi([FromBody] FichadaCreateDto dto)
    {
        var f = new Fichada
        {
            EmpleadoId = dto.EmpleadoId, Timestamp = dto.Timestamp,
            Tipo = dto.Tipo, Origen = OrigenFichada.ApiExterna,
            Observacion = dto.Observacion
        };
        await _repo.AddAsync(f);
        await _repo.SaveChangesAsync();
        return Ok(new { id = f.Id });
    }

    private static FichadaDto Map(Fichada f) => new(
        f.Id, f.EmpleadoId, f.Empleado?.NombreCompleto ?? "",
        f.Timestamp, f.Tipo, f.Origen, f.Observacion,
        f.FichadaCorregidaId.HasValue);
}
