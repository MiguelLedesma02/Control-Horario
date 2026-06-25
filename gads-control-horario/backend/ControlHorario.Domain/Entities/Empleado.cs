using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

public class Empleado
{
    public int Id { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Cuil { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string CategoriaLaboral { get; set; } = string.Empty;
    public string? ConvenioColectivo { get; set; }
    public TipoJornada TipoJornada { get; set; }
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    public int HorarioId { get; set; }
    public Horario Horario { get; set; } = null!;

    public ICollection<Fichada> Fichadas { get; set; } = new List<Fichada>();
    public ICollection<Novedad> Novedades { get; set; } = new List<Novedad>();

    public string NombreCompleto => $"{Apellido}, {Nombre}";
}
