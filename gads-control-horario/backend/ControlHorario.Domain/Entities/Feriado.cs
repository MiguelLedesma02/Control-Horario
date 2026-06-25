namespace ControlHorario.Domain.Entities;

public class Feriado
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool EsNacional { get; set; } = true;
}
