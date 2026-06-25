using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHorario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HorariosController : ControllerBase
{
    private readonly IHorarioRepository _repo;
    public HorariosController(IHorarioRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok((await _repo.GetAllAsync()).Select(Map));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var h = await _repo.GetByIdAsync(id);
        return h == null ? NotFound() : Ok(Map(h));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] HorarioDto dto)
    {
        var h = new Horario
        {
            Nombre = dto.Nombre, TipoJornada = dto.TipoJornada,
            DiasLaborables = dto.DiasLaborables,
            HoraEntrada = dto.HoraEntrada, HoraSalida = dto.HoraSalida,
            InicioDescanso = dto.InicioDescanso, FinDescanso = dto.FinDescanso,
            MinutosMinimosDescanso = dto.MinutosMinimosDescanso,
            ToleranciaEntradaMin = dto.ToleranciaEntradaMin,
            ToleranciaSalidaMin = dto.ToleranciaSalidaMin,
            UmbralHorasExtraMin = dto.UmbralHorasExtraMin
        };
        await _repo.AddAsync(h);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = h.Id }, Map(h));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] HorarioDto dto)
    {
        var h = await _repo.GetByIdAsync(id);
        if (h == null) return NotFound();

        h.Nombre = dto.Nombre;
        h.TipoJornada = dto.TipoJornada;
        h.DiasLaborables = dto.DiasLaborables;
        h.HoraEntrada = dto.HoraEntrada;
        h.HoraSalida = dto.HoraSalida;
        h.InicioDescanso = dto.InicioDescanso;
        h.FinDescanso = dto.FinDescanso;
        h.MinutosMinimosDescanso = dto.MinutosMinimosDescanso;
        h.ToleranciaEntradaMin = dto.ToleranciaEntradaMin;
        h.ToleranciaSalidaMin = dto.ToleranciaSalidaMin;
        h.UmbralHorasExtraMin = dto.UmbralHorasExtraMin;

        await _repo.UpdateAsync(h);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var h = await _repo.GetByIdAsync(id);
        if (h == null) return NotFound();
        await _repo.DeleteAsync(h);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private static HorarioDto Map(Horario h) => new(
        h.Id, h.Nombre, h.TipoJornada, h.DiasLaborables,
        h.HoraEntrada, h.HoraSalida, h.InicioDescanso, h.FinDescanso,
        h.MinutosMinimosDescanso, h.ToleranciaEntradaMin,
        h.ToleranciaSalidaMin, h.UmbralHorasExtraMin);
}
