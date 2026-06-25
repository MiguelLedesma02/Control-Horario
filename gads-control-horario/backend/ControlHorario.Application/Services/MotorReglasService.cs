using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;

namespace ControlHorario.Application.Services;

/// <summary>
/// Motor de Reglas: dada una colección de fichadas crudas + horario asignado +
/// parámetros de la empresa, produce una lista de Novedades automáticas
/// para un empleado y un rango de fechas.
///
/// Es PURO (no toca DB). Se ejecuta en memoria. Se puede recalcular cuantas
/// veces sea necesario sin perder el dato crudo.
/// </summary>
public class MotorReglasService
{
    private readonly ParametrosEmpresa _parametros;
    private readonly HashSet<DateTime> _feriados;

    public MotorReglasService(ParametrosEmpresa parametros, IEnumerable<Feriado> feriados)
    {
        _parametros = parametros;
        _feriados = feriados.Select(f => f.Fecha.Date).ToHashSet();
    }

    public List<Novedad> EvaluarPeriodo(
        Empleado empleado,
        IEnumerable<Fichada> fichadasDelPeriodo,
        DateTime fechaDesde,
        DateTime fechaHasta,
        IEnumerable<Novedad> novedadesManualesAprobadas)
    {
        var resultado = new List<Novedad>();
        var horario = empleado.Horario;

        // Solo considerar fichadas NO corregidas (la corrección reemplaza a la original)
        var fichadasIdsCorregidas = fichadasDelPeriodo
            .Where(f => f.FichadaCorregidaId.HasValue)
            .Select(f => f.FichadaCorregidaId!.Value)
            .ToHashSet();

        var fichadasVigentes = fichadasDelPeriodo
            .Where(f => !fichadasIdsCorregidas.Contains(f.Id))
            .OrderBy(f => f.Timestamp)
            .ToList();

        // Detectar dobles fichadas (mismo tipo en ventana corta)
        DetectarDobleFichada(empleado, fichadasVigentes, resultado);

        // Mapa: fechas con licencia/justificativo aprobado
        var diasJustificados = ObtenerDiasJustificados(novedadesManualesAprobadas);

        // Recorrer día por día
        for (var dia = fechaDesde.Date; dia <= fechaHasta.Date; dia = dia.AddDays(1))
        {
            EvaluarDia(empleado, horario, dia, fichasDelDia(fichadasVigentes, dia),
                       diasJustificados, resultado);
        }

        return resultado;
    }

    private static IEnumerable<Fichada> fichasDelDia(IEnumerable<Fichada> fichadas, DateTime dia)
        => fichadas.Where(f => f.TimestampLocal.Date == dia.Date);

    private static HashSet<DateTime> ObtenerDiasJustificados(IEnumerable<Novedad> novedadesAprobadas)
    {
        var dias = new HashSet<DateTime>();
        foreach (var nov in novedadesAprobadas.Where(n => n.Estado == EstadoNovedad.Aprobada))
        {
            for (var d = nov.FechaDesde.Date; d <= nov.FechaHasta.Date; d = d.AddDays(1))
                dias.Add(d);
        }
        return dias;
    }

    private void EvaluarDia(
        Empleado empleado, Horario horario, DateTime dia,
        IEnumerable<Fichada> fichadasDelDia,
        HashSet<DateTime> diasJustificados,
        List<Novedad> resultado)
    {
        var lista = fichadasDelDia.OrderBy(f => f.Timestamp).ToList();
        bool esLaborable = horario.EsDiaLaborable(dia.DayOfWeek);
        bool esFeriado = _feriados.Contains(dia.Date);

        var entrada = lista.FirstOrDefault(f => f.Tipo == TipoFichada.Entrada);
        var salida = lista.LastOrDefault(f => f.Tipo == TipoFichada.Salida);
        var salidaDesc = lista.FirstOrDefault(f => f.Tipo == TipoFichada.SalidaDescanso);
        var regresoDesc = lista.FirstOrDefault(f => f.Tipo == TipoFichada.RegresoDescanso);

        // ─── Caso 1: día laborable sin fichadas ───
        if (esLaborable && !lista.Any() && !esFeriado)
        {
            if (!diasJustificados.Contains(dia.Date))
            {
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.AusenciaInjustificada,
                    dia, dia, 1, OrigenNovedad.Automatica,
                    "Día laborable sin fichadas y sin justificativo"));
            }
            return;
        }

        if (!lista.Any()) return; // día no laborable sin fichadas → ignorar

        // ─── Caso 2: hay fichadas en día NO laborable o feriado → potencial 100% ───
        if ((!esLaborable || esFeriado) && entrada != null && salida != null)
        {
            int minutos = (int)(salida.TimestampLocal - entrada.TimestampLocal).TotalMinutes;
            // Descontar descanso si lo tomó
            if (salidaDesc != null && regresoDesc != null)
                minutos -= (int)(regresoDesc.TimestampLocal - salidaDesc.TimestampLocal).TotalMinutes;

            if (minutos > 0)
            {
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.HoraExtra100,
                    dia, dia, minutos, OrigenNovedad.Automatica,
                    esFeriado ? $"Trabajo en feriado ({minutos} min)"
                              : $"Trabajo en día de descanso ({minutos} min)"));
            }
            return;
        }

        // ─── Caso 3: día laborable con fichadas ───

        // 3a. Tardanza
        if (entrada != null)
        {
            var entradaEsperada = dia.Date.Add(horario.HoraEntrada);
            var tolerancia = TimeSpan.FromMinutes(horario.ToleranciaEntradaMin);
            if (entrada.TimestampLocal > entradaEsperada.Add(tolerancia))
            {
                int delta = (int)(entrada.TimestampLocal - entradaEsperada).TotalMinutes;
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.Tardanza,
                    dia, dia, delta, OrigenNovedad.Automatica,
                    $"Llegó a las {entrada.TimestampLocal:HH:mm}, esperado {horario.HoraEntrada:hh\\:mm} (+{delta} min)"));
            }
        }
        else if (esLaborable && !diasJustificados.Contains(dia.Date))
        {
            // Hay alguna fichada pero NO de entrada → posible olvido
            resultado.Add(NuevaNovedad(empleado, TipoNovedad.AusenciaInjustificada,
                dia, dia, 1, OrigenNovedad.Automatica,
                "Faltan fichadas de entrada"));
        }

        // 3b. Salida anticipada
        if (salida != null && entrada != null)
        {
            var salidaEsperada = dia.Date.Add(horario.HoraSalida);
            var tolerancia = TimeSpan.FromMinutes(horario.ToleranciaSalidaMin);
            if (salida.TimestampLocal < salidaEsperada.Subtract(tolerancia))
            {
                int delta = (int)(salidaEsperada - salida.TimestampLocal).TotalMinutes;
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.SalidaAnticipada,
                    dia, dia, delta, OrigenNovedad.Automatica,
                    $"Salió a las {salida.TimestampLocal:HH:mm}, esperado {horario.HoraSalida:hh\\:mm} (-{delta} min)"));
            }
        }

        // 3c. Horas extra al 50% (día hábil)
        if (salida != null && entrada != null && esLaborable && !esFeriado)
        {
            var salidaEsperada = dia.Date.Add(horario.HoraSalida);
            var umbral = TimeSpan.FromMinutes(horario.UmbralHorasExtraMin);
            if (salida.TimestampLocal > salidaEsperada.Add(umbral))
            {
                int delta = (int)(salida.TimestampLocal - salidaEsperada).TotalMinutes;
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.HoraExtra50,
                    dia, dia, delta, OrigenNovedad.Automatica,
                    $"Horas extra día hábil ({delta} min)"));
            }
        }

        // 3d. Descanso
        if (horario.InicioDescanso.HasValue && horario.FinDescanso.HasValue
            && salidaDesc != null && regresoDesc != null)
        {
            int minutosTomados = (int)(regresoDesc.TimestampLocal - salidaDesc.TimestampLocal).TotalMinutes;
            int minutosAsignados = (int)(horario.FinDescanso.Value - horario.InicioDescanso.Value).TotalMinutes;

            if (minutosTomados > minutosAsignados)
            {
                int exceso = minutosTomados - minutosAsignados;
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.DescansoExcedido,
                    dia, dia, exceso, OrigenNovedad.Automatica,
                    $"Descanso excedido en {exceso} min"));
            }
            else if (minutosTomados < horario.MinutosMinimosDescanso && horario.MinutosMinimosDescanso > 0)
            {
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.DescansoNoTomado,
                    dia, dia, horario.MinutosMinimosDescanso - minutosTomados, OrigenNovedad.Automatica,
                    $"Descanso por debajo del mínimo ({minutosTomados} min)"));
            }
        }
    }

    private void DetectarDobleFichada(Empleado empleado, List<Fichada> fichadas, List<Novedad> resultado)
    {
        var ventana = TimeSpan.FromMinutes(_parametros.VentanaDobleFichadaMin);
        for (int i = 1; i < fichadas.Count; i++)
        {
            var prev = fichadas[i - 1];
            var curr = fichadas[i];
            if (prev.Tipo == curr.Tipo &&
                (curr.TimestampLocal - prev.TimestampLocal) <= ventana)
            {
                resultado.Add(NuevaNovedad(empleado, TipoNovedad.DobleFichada,
                    curr.TimestampLocal, curr.TimestampLocal, 1, OrigenNovedad.Automatica,
                    $"Doble {curr.Tipo} en menos de {ventana.TotalMinutes} min"));
            }
        }
    }

    private static Novedad NuevaNovedad(Empleado emp, TipoNovedad tipo,
        DateTime desde, DateTime hasta, decimal cantidad, OrigenNovedad origen, string obs)
        => new()
        {
            EmpleadoId = emp.Id,
            Tipo = tipo,
            FechaDesde = desde.Date,
            FechaHasta = hasta.Date,
            Cantidad = cantidad,
            Origen = origen,
            Estado = EstadoNovedad.Pendiente,
            Observacion = obs,
            FechaCreacion = DateTime.UtcNow
        };
}
