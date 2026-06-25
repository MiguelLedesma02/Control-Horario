namespace ControlHorario.Domain.Entities;

/// <summary>
/// Parámetros globales por empresa. El motor de reglas los usa
/// para no hardcodear valores. En multi-tenant habría uno por tenant.
/// </summary>
public class ParametrosEmpresa
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? EmailContador { get; set; }

    // Defaults globales (sobrescribibles por horario)
    public int DefaultToleranciaEntradaMin { get; set; } = 5;
    public int DefaultUmbralHorasExtraMin { get; set; } = 15;
    public int VentanaDobleFichadaMin { get; set; } = 3;

    // Multiplicadores de horas extra (por convenio puede variar)
    public decimal MultiplicadorExtraDiaHabil { get; set; } = 1.50m;
    public decimal MultiplicadorExtraFeriado { get; set; } = 2.00m;
}
