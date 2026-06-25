using ControlHorario.Domain.Entities;

namespace ControlHorario.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task AddAsync(Usuario u);
}

public interface IEmpleadoRepository
{
    Task<List<Empleado>> GetAllAsync(bool soloActivos = false);
    Task<Empleado?> GetByIdAsync(int id);
    Task<Empleado?> GetByLegajoAsync(string legajo);
    Task AddAsync(Empleado e);
    Task UpdateAsync(Empleado e);
    Task DeleteAsync(Empleado e);
    Task<int> SaveChangesAsync();
}

public interface IHorarioRepository
{
    Task<List<Horario>> GetAllAsync();
    Task<Horario?> GetByIdAsync(int id);
    Task AddAsync(Horario h);
    Task UpdateAsync(Horario h);
    Task DeleteAsync(Horario h);
    Task<int> SaveChangesAsync();
}

public interface IFichadaRepository
{
    Task<List<Fichada>> GetByEmpleadoYRangoAsync(int empleadoId, DateTime desde, DateTime hasta);
    Task<List<Fichada>> GetByRangoAsync(DateTime desde, DateTime hasta);
    Task AddAsync(Fichada f);
    Task<int> SaveChangesAsync();
}

public interface INovedadRepository
{
    Task<List<Novedad>> GetByEmpleadoYRangoAsync(int empleadoId, DateTime desde, DateTime hasta);
    Task<List<Novedad>> GetByRangoAsync(DateTime desde, DateTime hasta);
    Task<List<Novedad>> GetPendientesAsync();
    Task<Novedad?> GetByIdAsync(long id);
    Task AddRangeAsync(IEnumerable<Novedad> ns);
    Task AddAsync(Novedad n);
    Task UpdateAsync(Novedad n);
    Task<int> SaveChangesAsync();
}

public interface ICierreRepository
{
    Task<List<CierreMensual>> GetAllAsync();
    Task<CierreMensual?> GetByIdAsync(int id);
    Task<CierreMensual?> GetByPeriodoAsync(int anio, int mes);
    Task AddAsync(CierreMensual c);
    Task UpdateAsync(CierreMensual c);
    Task<int> SaveChangesAsync();
}

public interface IParametrosRepository
{
    Task<ParametrosEmpresa> GetAsync();
    Task UpdateAsync(ParametrosEmpresa p);
}

public interface IFeriadoRepository
{
    Task<List<Feriado>> GetAllAsync();
    Task<List<Feriado>> GetByRangoAsync(DateTime desde, DateTime hasta);
}

public interface IExportadorService
{
    byte[] ExportarExcel(int anio, int mes, IEnumerable<DTOs.ResumenEmpleadoDto> resumenes);
    byte[] ExportarCsv(int anio, int mes, IEnumerable<DTOs.ResumenEmpleadoDto> resumenes);
    byte[] ExportarPdf(int anio, int mes, IEnumerable<DTOs.ResumenEmpleadoDto> resumenes);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtService
{
    string GenerarToken(Usuario u);
}
