using System.Security.Claims;
using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using ControlHorario.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHorario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CierreController : ControllerBase
{
    private readonly CierreService _cierreSvc;
    private readonly ICierreRepository _cierreRepo;
    private readonly IExportadorService _exportador;

    public CierreController(CierreService svc, ICierreRepository repo, IExportadorService exp)
    {
        _cierreSvc = svc; _cierreRepo = repo; _exportador = exp;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _cierreRepo.GetAllAsync();
        return Ok(lista.Select(c => new CierreMensualDto(
            c.Id, c.Anio, c.Mes, c.FechaCierre, c.Estado,
            c.UsuarioCierre?.Nombre ?? "—",
            0, c.Novedades?.Count ?? 0)));
    }

    [HttpGet("{anio}/{mes}/resumen")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> GetResumen(int anio, int mes)
    {
        var resumenes = await _cierreSvc.GenerarResumenesAsync(anio, mes);
        return Ok(resumenes);
    }

    [HttpPost("{anio}/{mes}/recalcular")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Recalcular(int anio, int mes)
    {
        var n = await _cierreSvc.RecalcularPeriodoAsync(anio, mes);
        return Ok(new { novedadesGeneradas = n });
    }

    [HttpPost("{anio}/{mes}/cerrar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Cerrar(int anio, int mes)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var c = await _cierreSvc.EjecutarCierreAsync(anio, mes, usuarioId);
            return Ok(new { id = c.Id, fechaCierre = c.FechaCierre });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{anio}/{mes}/exportar/{formato}")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> Exportar(int anio, int mes, string formato)
    {
        var resumenes = await _cierreSvc.GenerarResumenesAsync(anio, mes);
        var nombreBase = $"preliquidacion_{anio}_{mes:D2}";
        return formato.ToLower() switch
        {
            "xlsx" => File(_exportador.ExportarExcel(anio, mes, resumenes),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{nombreBase}.xlsx"),
            "csv" => File(_exportador.ExportarCsv(anio, mes, resumenes),
                "text/csv", $"{nombreBase}.csv"),
            "pdf" => File(_exportador.ExportarPdf(anio, mes, resumenes),
                "application/pdf", $"{nombreBase}.pdf"),
            _ => BadRequest(new { message = "Formato no soportado: usar xlsx, csv o pdf" })
        };
    }
}
