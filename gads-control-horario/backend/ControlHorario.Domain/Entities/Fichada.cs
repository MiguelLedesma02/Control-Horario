using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

/// <summary>
/// Dato crudo. Una vez insertada NO se modifica. Las correcciones se hacen
/// con una nueva Fichada que apunta a la original mediante FichadaCorregidaId.
/// </summary>
public class Fichada
{
    public long Id { get; set; }
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateTime Timestamp { get; set; }     // fecha + hora del evento
    public TipoFichada Tipo { get; set; }
    public OrigenFichada Origen { get; set; }

    // Si la fichada fue manual o por API, quién la cargó
    public int? UsuarioRegistroId { get; set; }
    public Usuario? UsuarioRegistro { get; set; }

    // Si esta fichada corrige a otra previa
    public long? FichadaCorregidaId { get; set; }
    public Fichada? FichadaCorregida { get; set; }

    public string? Observacion { get; set; }
    public DateTime FechaInsercion { get; set; } = DateTime.UtcNow;
}
