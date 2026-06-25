using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

/// <summary>
/// Dato crudo. Una vez insertada NO se modifica. Las correcciones se hacen
/// con una nueva Fichada que apunta a la original mediante FichadaCorregidaId.
///
/// Timestamp usa DateTimeOffset para preservar la zona horaria Argentina (-03:00)
/// tal como la envía el frontend, evitando conversiones involuntarias a UTC.
/// </summary>
public class Fichada
{
    public long Id { get; set; }
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    /// <summary>
    /// Hora local Argentina con offset explícito (-03:00).
    /// Se almacena como texto en SQLite para no perder el offset.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    public TipoFichada Tipo { get; set; }
    public OrigenFichada Origen { get; set; }

    public int? UsuarioRegistroId { get; set; }
    public Usuario? UsuarioRegistro { get; set; }

    public long? FichadaCorregidaId { get; set; }
    public Fichada? FichadaCorregida { get; set; }

    public string? Observacion { get; set; }

    /// <summary>Momento de inserción en hora local Argentina.</summary>
    public DateTimeOffset FechaInsercion { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Hora local como DateTime (sin offset) para cálculos del motor de reglas.
    /// Equivale a la hora que vio el empleado en su reloj.
    /// No se persiste en la DB.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTime TimestampLocal => Timestamp.DateTime;
}
