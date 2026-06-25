using ControlHorario.Domain.Enums;

namespace ControlHorario.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Nombre, string Rol, int? EmpleadoId);

public record EmpleadoDto(
    int Id, string Legajo, string Nombre, string Apellido, string Dni, string Cuil,
    DateTime FechaIngreso, string CategoriaLaboral, string? ConvenioColectivo,
    TipoJornada TipoJornada, EstadoEmpleado Estado, int HorarioId, string HorarioNombre,
    string? Email, string? Telefono);

public record EmpleadoCreateDto(
    string Legajo, string Nombre, string Apellido, string Dni, string Cuil,
    DateTime FechaIngreso, string CategoriaLaboral, string? ConvenioColectivo,
    TipoJornada TipoJornada, int HorarioId, string? Email, string? Telefono);

public record HorarioDto(
    int Id, string Nombre, TipoJornada TipoJornada, int DiasLaborables,
    TimeSpan HoraEntrada, TimeSpan HoraSalida,
    TimeSpan? InicioDescanso, TimeSpan? FinDescanso, int MinutosMinimosDescanso,
    int ToleranciaEntradaMin, int ToleranciaSalidaMin, int UmbralHorasExtraMin);

public record FichadaDto(
    long Id, int EmpleadoId, string EmpleadoNombre,
    // Devolvemos el timestamp con offset para que el frontend muestre la hora correcta
    DateTimeOffset Timestamp,
    TipoFichada Tipo, OrigenFichada Origen, string? Observacion, bool EsCorreccion);

public record FichadaCreateDto(
    int EmpleadoId,
    // DateTimeOffset preserva el "-03:00" que envía el frontend
    DateTimeOffset Timestamp,
    TipoFichada Tipo, OrigenFichada Origen,
    string? Observacion);

public record NovedadDto(
    long Id, int EmpleadoId, string EmpleadoNombre, TipoNovedad Tipo,
    OrigenNovedad Origen, EstadoNovedad Estado, DateTime FechaDesde, DateTime FechaHasta,
    decimal Cantidad, string? Observacion, DateTime FechaCreacion);

public record NovedadCreateDto(
    int EmpleadoId, TipoNovedad Tipo, DateTime FechaDesde, DateTime FechaHasta,
    decimal Cantidad, string? Observacion);

public record ResumenEmpleadoDto(
    int EmpleadoId, string Legajo, string NombreCompleto,
    int DiasTrabajados, int DiasAusenteJustificado, int DiasAusenteInjustificado,
    int MinutosTardanza, int MinutosHorasExtra50, int MinutosHorasExtra100,
    int DiasLicencia, int DiasVacaciones,
    List<NovedadDto> Novedades);

public record CierreMensualDto(
    int Id, int Anio, int Mes, DateTime FechaCierre, EstadoCierre Estado,
    string Cerrador, int CantidadEmpleados, int CantidadNovedades);
