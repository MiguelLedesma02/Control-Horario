using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHorario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoRepository _repo;
    private readonly IHorarioRepository _horarios;

    public EmpleadosController(IEmpleadoRepository repo, IHorarioRepository horarios)
    {
        _repo = repo; _horarios = horarios;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivos = false)
    {
        var lista = await _repo.GetAllAsync(soloActivos);
        return Ok(lista.Select(Map));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? NotFound() : Ok(Map(e));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] EmpleadoCreateDto dto)
    {
        var horario = await _horarios.GetByIdAsync(dto.HorarioId);
        if (horario == null) return BadRequest(new { message = "Horario inexistente" });

        var existente = await _repo.GetByLegajoAsync(dto.Legajo);
        if (existente != null) return Conflict(new { message = "Legajo duplicado" });

        var e = new Empleado
        {
            Legajo = dto.Legajo, Nombre = dto.Nombre, Apellido = dto.Apellido,
            Dni = dto.Dni, Cuil = dto.Cuil, FechaIngreso = dto.FechaIngreso,
            CategoriaLaboral = dto.CategoriaLaboral, ConvenioColectivo = dto.ConvenioColectivo,
            TipoJornada = dto.TipoJornada, HorarioId = dto.HorarioId,
            Email = dto.Email, Telefono = dto.Telefono
        };
        await _repo.AddAsync(e);
        await _repo.SaveChangesAsync();

        var creado = await _repo.GetByIdAsync(e.Id);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, Map(creado!));
    }

    [HttpPost("importar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Importar([FromBody] List<EmpleadoCreateDto> lista)
    {
        if (lista == null || lista.Count == 0)
            return BadRequest(new { message = "Lista vacía" });

        var importados = 0;
        var omitidos = new List<string>();

        foreach (var dto in lista)
        {
            var horario = await _horarios.GetByIdAsync(dto.HorarioId);
            if (horario == null) { omitidos.Add($"{dto.Legajo} (horario inválido)"); continue; }

            var existente = await _repo.GetByLegajoAsync(dto.Legajo);
            if (existente != null) { omitidos.Add($"{dto.Legajo} (legajo duplicado)"); continue; }

            var e = new Empleado
            {
                Legajo = dto.Legajo, Nombre = dto.Nombre, Apellido = dto.Apellido,
                Dni = dto.Dni, Cuil = dto.Cuil, FechaIngreso = dto.FechaIngreso,
                CategoriaLaboral = dto.CategoriaLaboral, ConvenioColectivo = dto.ConvenioColectivo,
                TipoJornada = dto.TipoJornada, HorarioId = dto.HorarioId,
                Email = dto.Email, Telefono = dto.Telefono
            };
            await _repo.AddAsync(e);
            importados++;
        }

        await _repo.SaveChangesAsync();
        return Ok(new { importados, omitidos });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] EmpleadoCreateDto dto)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e == null) return NotFound();

        e.Nombre = dto.Nombre; e.Apellido = dto.Apellido;
        e.Dni = dto.Dni; e.Cuil = dto.Cuil;
        e.CategoriaLaboral = dto.CategoriaLaboral;
        e.ConvenioColectivo = dto.ConvenioColectivo;
        e.TipoJornada = dto.TipoJornada;
        e.HorarioId = dto.HorarioId;
        e.Email = dto.Email; e.Telefono = dto.Telefono;

        await _repo.UpdateAsync(e);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/desactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e == null) return NotFound();
        e.Estado = Domain.Enums.EstadoEmpleado.Inactivo;
        await _repo.UpdateAsync(e);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e == null) return NotFound();
        await _repo.DeleteAsync(e);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private static EmpleadoDto Map(Empleado e) => new(
        e.Id, e.Legajo, e.Nombre, e.Apellido, e.Dni, e.Cuil, e.FechaIngreso,
        e.CategoriaLaboral, e.ConvenioColectivo, e.TipoJornada, e.Estado,
        e.HorarioId, e.Horario?.Nombre ?? "", e.Email, e.Telefono);
}
