namespace ControlHorario.Domain.Enums;

public enum RolUsuario
{
    Administrador = 1,
    Empleado = 2,
    Contador = 3
}

public enum EstadoEmpleado
{
    Activo = 1,
    Inactivo = 2,
    Suspendido = 3
}

public enum TipoJornada
{
    Completa = 1,
    Parcial = 2,
    Flexible = 3,
    Rotativa = 4
}

public enum TipoFichada
{
    Entrada = 1,
    Salida = 2,
    SalidaDescanso = 3,
    RegresoDescanso = 4
}

public enum OrigenFichada
{
    Biometrico = 1,
    QR = 2,
    PIN = 3,
    Manual = 4,
    ApiExterna = 5
}

public enum TipoNovedad
{
    // Automáticas
    Tardanza = 1,
    AusenciaInjustificada = 2,
    HoraExtra50 = 3,
    HoraExtra100 = 4,
    SalidaAnticipada = 5,
    DobleFichada = 6,
    DescansoExcedido = 7,
    DescansoNoTomado = 8,

    // Manuales
    LicenciaEnfermedad = 100,
    LicenciaExamen = 101,
    LicenciaMaternidad = 102,
    VacacionesParciales = 103,
    SuspensionConGoce = 104,
    SuspensionSinGoce = 105,
    PermisoEspecial = 106,
    JustificativoMedico = 107
}

public enum EstadoNovedad
{
    Pendiente = 1,
    Aprobada = 2,
    Rechazada = 3
}

public enum OrigenNovedad
{
    Automatica = 1,
    Manual = 2
}

public enum EstadoCierre
{
    Borrador = 1,
    Cerrado = 2
}
