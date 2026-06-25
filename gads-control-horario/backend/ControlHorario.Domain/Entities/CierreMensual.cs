using ControlHorario.Domain.Enums;

namespace ControlHorario.Domain.Entities;

public class CierreMensual
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateTime FechaCierre { get; set; }
    public int UsuarioCierreId { get; set; }
    public Usuario UsuarioCierre { get; set; } = null!;

    public EstadoCierre Estado { get; set; } = EstadoCierre.Borrador;

    public string? RutaArchivoExportado { get; set; }
    public string? FormatoExportacion { get; set; }   // XLSX | CSV | PDF

    /// <summary>
    /// Snapshot serializado JSON con el resumen al momento del cierre.
    /// Se usa para garantizar inmutabilidad: aunque cambien las novedades
    /// después, el cierre conserva su versión.
    /// </summary>
    public string? SnapshotJson { get; set; }

    public ICollection<Novedad> Novedades { get; set; } = new List<Novedad>();
}
