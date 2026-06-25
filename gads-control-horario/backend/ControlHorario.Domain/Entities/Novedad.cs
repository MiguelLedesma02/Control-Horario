using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

public class Novedad
{
    public long Id { get; set; }
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public TipoNovedad Tipo { get; set; }
    public OrigenNovedad Origen { get; set; }
    public EstadoNovedad Estado { get; set; } = EstadoNovedad.Pendiente;

    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    /// <summary>
    /// Cantidad. Su unidad depende del tipo:
    /// Tardanza/SalidaAnticipada/HoraExtra → minutos
    /// Ausencia/Licencia/Vacaciones → días
    /// </summary>
    public decimal Cantidad { get; set; }

    public string? Observacion { get; set; }

    // Para novedades manuales: usuario que la cargó / aprobó / rechazó
    public int? UsuarioCreadorId { get; set; }
    public int? UsuarioRevisorId { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? MotivoRechazo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Si la novedad fue incluida en un cierre, queda referenciada
    public int? CierreMensualId { get; set; }
    public CierreMensual? CierreMensual { get; set; }
}
