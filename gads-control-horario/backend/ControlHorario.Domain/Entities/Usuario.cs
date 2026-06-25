using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Si el usuario es Empleado, está vinculado a un Empleado
    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }
}
