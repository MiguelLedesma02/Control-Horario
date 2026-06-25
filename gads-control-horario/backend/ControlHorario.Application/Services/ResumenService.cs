using ControlHorario.Application.DTOs;
using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;

namespace ControlHorario.Application.Services;

public class ResumenService
{
    /// <summary>
    /// Consolida las novedades aprobadas + datos del empleado en un resumen
    /// listo para ser exportado al contador.
    /// </summary>
    public ResumenEmpleadoDto Consolidar(
        Empleado empleado,
        IEnumerable<Novedad> novedadesPeriodo,
        DateTime desde, DateTime hasta)
    {
        var aprobadas = novedadesPeriodo.Where(n => n.Estado == EstadoNovedad.Aprobada).ToList();

        int diasAusJust = (int)aprobadas
            .Where(n => EsLicenciaJustificada(n.Tipo))
            .Sum(n => n.Cantidad);

        int diasAusInjust = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.AusenciaInjustificada)
            .Sum(n => n.Cantidad);

        int minTardanza = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.Tardanza)
            .Sum(n => n.Cantidad);

        int minHE50 = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.HoraExtra50)
            .Sum(n => n.Cantidad);

        int minHE100 = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.HoraExtra100)
            .Sum(n => n.Cantidad);

        int diasLicencia = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.LicenciaEnfermedad
                     || n.Tipo == TipoNovedad.LicenciaMaternidad
                     || n.Tipo == TipoNovedad.LicenciaExamen)
            .Sum(n => n.Cantidad);

        int diasVacaciones = (int)aprobadas
            .Where(n => n.Tipo == TipoNovedad.VacacionesParciales)
            .Sum(n => n.Cantidad);

        int diasLaborables = ContarDiasLaborables(empleado.Horario, desde, hasta);
        int diasTrabajados = diasLaborables - diasAusInjust - diasAusJust - diasLicencia - diasVacaciones;
        if (diasTrabajados < 0) diasTrabajados = 0;

        return new ResumenEmpleadoDto(
            empleado.Id, empleado.Legajo, empleado.NombreCompleto,
            diasTrabajados, diasAusJust, diasAusInjust,
            minTardanza, minHE50, minHE100,
            diasLicencia, diasVacaciones,
            aprobadas.Select(MapNovedad).ToList()
        );
    }

    private static bool EsLicenciaJustificada(TipoNovedad t) => t is
        TipoNovedad.LicenciaEnfermedad or TipoNovedad.LicenciaMaternidad
        or TipoNovedad.LicenciaExamen or TipoNovedad.PermisoEspecial
        or TipoNovedad.JustificativoMedico or TipoNovedad.SuspensionConGoce;

    private static int ContarDiasLaborables(Horario h, DateTime desde, DateTime hasta)
    {
        int n = 0;
        for (var d = desde.Date; d <= hasta.Date; d = d.AddDays(1))
            if (h.EsDiaLaborable(d.DayOfWeek)) n++;
        return n;
    }

    private static NovedadDto MapNovedad(Novedad n) => new(
        n.Id, n.EmpleadoId, n.Empleado?.NombreCompleto ?? "",
        n.Tipo, n.Origen, n.Estado, n.FechaDesde, n.FechaHasta,
        n.Cantidad, n.Observacion, n.FechaCreacion);
}
