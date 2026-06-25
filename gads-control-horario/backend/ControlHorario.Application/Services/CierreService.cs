using System.Text.Json;
using ControlHorario.Application.DTOs;
using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;

namespace ControlHorario.Application.Services;

public class CierreService
{
    private readonly IEmpleadoRepository _empleados;
    private readonly IFichadaRepository _fichadas;
    private readonly INovedadRepository _novedades;
    private readonly ICierreRepository _cierres;
    private readonly IParametrosRepository _parametros;
    private readonly IFeriadoRepository _feriados;
    private readonly ResumenService _resumenSvc;

    public CierreService(
        IEmpleadoRepository empleados, IFichadaRepository fichadas,
        INovedadRepository novedades, ICierreRepository cierres,
        IParametrosRepository parametros, IFeriadoRepository feriados,
        ResumenService resumenSvc)
    {
        _empleados = empleados; _fichadas = fichadas;
        _novedades = novedades; _cierres = cierres;
        _parametros = parametros; _feriados = feriados;
        _resumenSvc = resumenSvc;
    }

    /// <summary>
    /// Recalcula novedades automáticas para todos los empleados activos en el período.
    /// Solo agrega novedades NUEVAS (idempotente: no duplica si ya existían).
    /// </summary>
    public async Task<int> RecalcularPeriodoAsync(int anio, int mes)
    {
        var (desde, hasta) = RangoMes(anio, mes);
        var parametros = await _parametros.GetAsync();
        var feriados = await _feriados.GetByRangoAsync(desde, hasta);
        var motor = new MotorReglasService(parametros, feriados);

        var empleados = await _empleados.GetAllAsync(soloActivos: true);
        int totalNuevas = 0;

        foreach (var emp in empleados)
        {
            var fichadas = await _fichadas.GetByEmpleadoYRangoAsync(emp.Id, desde, hasta);
            var novsExist = await _novedades.GetByEmpleadoYRangoAsync(emp.Id, desde, hasta);
            var manualesAprob = novsExist.Where(n => n.Origen == OrigenNovedad.Manual
                                                 && n.Estado == EstadoNovedad.Aprobada);

            var nuevas = motor.EvaluarPeriodo(emp, fichadas, desde, hasta, manualesAprob);

            // Filtrar las que ya existen (mismo tipo + mismas fechas)
            var keysExistentes = novsExist
                .Where(n => n.Origen == OrigenNovedad.Automatica)
                .Select(n => $"{n.Tipo}|{n.FechaDesde:yyyyMMdd}|{n.FechaHasta:yyyyMMdd}")
                .ToHashSet();

            var aInsertar = nuevas
                .Where(n => !keysExistentes.Contains($"{n.Tipo}|{n.FechaDesde:yyyyMMdd}|{n.FechaHasta:yyyyMMdd}"))
                .ToList();

            if (aInsertar.Any())
            {
                await _novedades.AddRangeAsync(aInsertar);
                totalNuevas += aInsertar.Count;
            }
        }

        await _novedades.SaveChangesAsync();
        return totalNuevas;
    }

    public async Task<List<ResumenEmpleadoDto>> GenerarResumenesAsync(int anio, int mes)
    {
        var (desde, hasta) = RangoMes(anio, mes);
        var empleados = await _empleados.GetAllAsync(soloActivos: true);
        var resumenes = new List<ResumenEmpleadoDto>();

        foreach (var emp in empleados)
        {
            var novs = await _novedades.GetByEmpleadoYRangoAsync(emp.Id, desde, hasta);
            resumenes.Add(_resumenSvc.Consolidar(emp, novs, desde, hasta));
        }
        return resumenes;
    }

    public async Task<CierreMensual> EjecutarCierreAsync(int anio, int mes, int usuarioId)
    {
        var existente = await _cierres.GetByPeriodoAsync(anio, mes);
        if (existente != null && existente.Estado == EstadoCierre.Cerrado)
            throw new InvalidOperationException("El período ya está cerrado.");

        // 1. Recalcular para incluir todo lo último
        await RecalcularPeriodoAsync(anio, mes);

        // 2. Verificar que no queden novedades pendientes
        var (desde, hasta) = RangoMes(anio, mes);
        var pendientes = (await _novedades.GetByRangoAsync(desde, hasta))
            .Where(n => n.Estado == EstadoNovedad.Pendiente).ToList();
        if (pendientes.Any())
            throw new InvalidOperationException(
                $"Hay {pendientes.Count} novedades pendientes de revisión. " +
                "Aprobar o rechazar antes de cerrar.");

        // 3. Generar snapshot
        var resumenes = await GenerarResumenesAsync(anio, mes);
        var snapshot = JsonSerializer.Serialize(resumenes,
            new JsonSerializerOptions { WriteIndented = false });

        var cierre = existente ?? new CierreMensual
        {
            Anio = anio, Mes = mes,
        };

        cierre.FechaCierre = DateTime.UtcNow;
        cierre.UsuarioCierreId = usuarioId;
        cierre.Estado = EstadoCierre.Cerrado;
        cierre.SnapshotJson = snapshot;

        if (existente == null)
            await _cierres.AddAsync(cierre);
        else
            await _cierres.UpdateAsync(cierre);

        // 4. Asociar novedades aprobadas al cierre
        var novsAprobadas = (await _novedades.GetByRangoAsync(desde, hasta))
            .Where(n => n.Estado == EstadoNovedad.Aprobada).ToList();
        foreach (var n in novsAprobadas)
        {
            n.CierreMensualId = cierre.Id;
            await _novedades.UpdateAsync(n);
        }

        await _cierres.SaveChangesAsync();
        await _novedades.SaveChangesAsync();
        return cierre;
    }

    public static (DateTime, DateTime) RangoMes(int anio, int mes)
    {
        var desde = new DateTime(anio, mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);
        return (desde, hasta);
    }
}
