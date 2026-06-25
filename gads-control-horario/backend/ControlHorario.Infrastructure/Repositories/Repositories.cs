using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;
using ControlHorario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControlHorario.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _ctx;
    public UsuarioRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<Usuario?> GetByEmailAsync(string email) =>
        _ctx.Usuarios.Include(u => u.Empleado).FirstOrDefaultAsync(u => u.Email == email);

    public Task<Usuario?> GetByIdAsync(int id) =>
        _ctx.Usuarios.FindAsync(id).AsTask();

    public async Task AddAsync(Usuario u) { await _ctx.Usuarios.AddAsync(u); await _ctx.SaveChangesAsync(); }
}

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly AppDbContext _ctx;
    public EmpleadoRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Empleado>> GetAllAsync(bool soloActivos = false) =>
        _ctx.Empleados.Include(e => e.Horario)
            .Where(e => !soloActivos || e.Estado == EstadoEmpleado.Activo)
            .OrderBy(e => e.Apellido).ToListAsync();

    public Task<Empleado?> GetByIdAsync(int id) =>
        _ctx.Empleados.Include(e => e.Horario).FirstOrDefaultAsync(e => e.Id == id);

    public Task<Empleado?> GetByLegajoAsync(string legajo) =>
        _ctx.Empleados.Include(e => e.Horario).FirstOrDefaultAsync(e => e.Legajo == legajo);

    public async Task AddAsync(Empleado e) { await _ctx.Empleados.AddAsync(e); }
    public Task UpdateAsync(Empleado e) { _ctx.Empleados.Update(e); return Task.CompletedTask; }
    public Task DeleteAsync(Empleado e) { _ctx.Empleados.Remove(e); return Task.CompletedTask; }
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}

public class HorarioRepository : IHorarioRepository
{
    private readonly AppDbContext _ctx;
    public HorarioRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Horario>> GetAllAsync() => _ctx.Horarios.OrderBy(h => h.Nombre).ToListAsync();
    public Task<Horario?> GetByIdAsync(int id) => _ctx.Horarios.FindAsync(id).AsTask();
    public async Task AddAsync(Horario h) { await _ctx.Horarios.AddAsync(h); }
    public Task UpdateAsync(Horario h) { _ctx.Horarios.Update(h); return Task.CompletedTask; }
    public Task DeleteAsync(Horario h) { _ctx.Horarios.Remove(h); return Task.CompletedTask; }
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}

public class FichadaRepository : IFichadaRepository
{
    private readonly AppDbContext _ctx;
    public FichadaRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Fichada>> GetByEmpleadoYRangoAsync(int empleadoId, DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;                          // comienzo del día (00:00)
        var fin = hasta.Date.AddDays(1).AddTicks(-1);     // fin del día (23:59:59.999)
        return _ctx.Fichadas.Where(f => f.EmpleadoId == empleadoId
                              && f.Timestamp >= inicio && f.Timestamp <= fin)
            .OrderBy(f => f.Timestamp).ToListAsync();
    }

    public Task<List<Fichada>> GetByRangoAsync(DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;                          // comienzo del día (00:00)
        var fin = hasta.Date.AddDays(1).AddTicks(-1);     // fin del día (23:59:59.999)
        return _ctx.Fichadas.Include(f => f.Empleado)
            .Where(f => f.Timestamp >= inicio && f.Timestamp <= fin)
            .OrderBy(f => f.Timestamp).ToListAsync();
    }

    public async Task AddAsync(Fichada f) { await _ctx.Fichadas.AddAsync(f); }
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}

public class NovedadRepository : INovedadRepository
{
    private readonly AppDbContext _ctx;
    public NovedadRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Novedad>> GetByEmpleadoYRangoAsync(int empleadoId, DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;                          // comienzo del día (00:00)
        var fin = hasta.Date.AddDays(1).AddTicks(-1);     // fin del día (23:59:59.999)
        return _ctx.Novedades.Include(n => n.Empleado)
            .Where(n => n.EmpleadoId == empleadoId
                     && n.FechaDesde >= inicio && n.FechaHasta <= fin)
            .OrderBy(n => n.FechaDesde).ToListAsync();
    }

    public Task<List<Novedad>> GetByRangoAsync(DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;                          // comienzo del día (00:00)
        var fin = hasta.Date.AddDays(1).AddTicks(-1);     // fin del día (23:59:59.999)
        return _ctx.Novedades.Include(n => n.Empleado)
            .Where(n => n.FechaDesde >= inicio && n.FechaHasta <= fin)
            .OrderBy(n => n.FechaDesde).ToListAsync();
    }

    public Task<List<Novedad>> GetPendientesAsync() =>
        _ctx.Novedades.Include(n => n.Empleado)
            .Where(n => n.Estado == EstadoNovedad.Pendiente)
            .OrderByDescending(n => n.FechaCreacion).ToListAsync();

    public Task<Novedad?> GetByIdAsync(long id) =>
        _ctx.Novedades.Include(n => n.Empleado).FirstOrDefaultAsync(n => n.Id == id);

    public async Task AddAsync(Novedad n) { await _ctx.Novedades.AddAsync(n); }
    public async Task AddRangeAsync(IEnumerable<Novedad> ns) { await _ctx.Novedades.AddRangeAsync(ns); }
    public Task UpdateAsync(Novedad n) { _ctx.Novedades.Update(n); return Task.CompletedTask; }
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}

public class CierreRepository : ICierreRepository
{
    private readonly AppDbContext _ctx;
    public CierreRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<CierreMensual>> GetAllAsync() =>
        _ctx.Cierres.Include(c => c.UsuarioCierre)
            .OrderByDescending(c => c.Anio).ThenByDescending(c => c.Mes).ToListAsync();

    public Task<CierreMensual?> GetByIdAsync(int id) =>
        _ctx.Cierres.Include(c => c.UsuarioCierre).FirstOrDefaultAsync(c => c.Id == id);

    public Task<CierreMensual?> GetByPeriodoAsync(int anio, int mes) =>
        _ctx.Cierres.FirstOrDefaultAsync(c => c.Anio == anio && c.Mes == mes);

    public async Task AddAsync(CierreMensual c) { await _ctx.Cierres.AddAsync(c); }
    public Task UpdateAsync(CierreMensual c) { _ctx.Cierres.Update(c); return Task.CompletedTask; }
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}

public class ParametrosRepository : IParametrosRepository
{
    private readonly AppDbContext _ctx;
    public ParametrosRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<ParametrosEmpresa> GetAsync()
    {
        var p = await _ctx.ParametrosEmpresa.FirstOrDefaultAsync();
        if (p == null)
        {
            p = new ParametrosEmpresa { RazonSocial = "Mi Pyme SRL", Cuit = "30-00000000-0" };
            _ctx.ParametrosEmpresa.Add(p);
            await _ctx.SaveChangesAsync();
        }
        return p;
    }

    public async Task UpdateAsync(ParametrosEmpresa p) { _ctx.ParametrosEmpresa.Update(p); await _ctx.SaveChangesAsync(); }
}

public class FeriadoRepository : IFeriadoRepository
{
    private readonly AppDbContext _ctx;
    public FeriadoRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Feriado>> GetAllAsync() => _ctx.Feriados.OrderBy(f => f.Fecha).ToListAsync();
    public Task<List<Feriado>> GetByRangoAsync(DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;                          // comienzo del día (00:00)
        var fin = hasta.Date.AddDays(1).AddTicks(-1);     // fin del día (23:59:59.999)
        return _ctx.Feriados.Where(f => f.Fecha >= inicio && f.Fecha <= fin).ToListAsync();
    }
}
