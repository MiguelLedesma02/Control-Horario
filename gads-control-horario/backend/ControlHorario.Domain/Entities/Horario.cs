using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

public class Horario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;          // ej. "Comercio Lun-Vie 9 a 18"
    public TipoJornada TipoJornada { get; set; }

    // Días de la semana laborables (bitmask: lunes=1, martes=2, ... domingo=64)
    public int DiasLaborables { get; set; }

    public TimeSpan HoraEntrada { get; set; }
    public TimeSpan HoraSalida { get; set; }

    // Descanso (opcional)
    public TimeSpan? InicioDescanso { get; set; }
    public TimeSpan? FinDescanso { get; set; }
    public int MinutosMinimosDescanso { get; set; }

    // Tolerancias
    public int ToleranciaEntradaMin { get; set; } = 5;
    public int ToleranciaSalidaMin { get; set; } = 5;

    // Umbral a partir del cual se contabilizan horas extra
    public int UmbralHorasExtraMin { get; set; } = 15;

    // Banda horaria flexible (si aplica)
    public TimeSpan? BandaInicio { get; set; }
    public TimeSpan? BandaFin { get; set; }
    public int? HorasMinimasDiarias { get; set; }

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public bool EsDiaLaborable(DayOfWeek dia)
    {
        int bit = dia switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 4,
            DayOfWeek.Thursday => 8,
            DayOfWeek.Friday => 16,
            DayOfWeek.Saturday => 32,
            DayOfWeek.Sunday => 64,
            _ => 0
        };
        return (DiasLaborables & bit) != 0;
    }
}
