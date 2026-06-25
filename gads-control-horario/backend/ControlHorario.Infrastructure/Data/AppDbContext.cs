using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControlHorario.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<Fichada> Fichadas => Set<Fichada>();
    public DbSet<Novedad> Novedades => Set<Novedad>();
    public DbSet<CierreMensual> Cierres => Set<CierreMensual>();
    public DbSet<Feriado> Feriados => Set<Feriado>();
    public DbSet<ParametrosEmpresa> ParametrosEmpresa => Set<ParametrosEmpresa>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Nombre).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.HasOne(u => u.Empleado).WithMany().HasForeignKey(u => u.EmpleadoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Empleado>(e =>
        {
            e.HasIndex(x => x.Legajo).IsUnique();
            e.HasIndex(x => x.Cuil);
            e.Property(x => x.Legajo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellido).HasMaxLength(100).IsRequired();
            e.Property(x => x.Dni).HasMaxLength(15).IsRequired();
            e.Property(x => x.Cuil).HasMaxLength(15).IsRequired();
            e.Property(x => x.CategoriaLaboral).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Horario).WithMany(h => h.Empleados)
                .HasForeignKey(x => x.HorarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Horario>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });

        b.Entity<Fichada>(e =>
        {
            e.HasIndex(x => new { x.EmpleadoId, x.Timestamp });
            e.HasOne(x => x.Empleado).WithMany(x => x.Fichadas)
                .HasForeignKey(x => x.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UsuarioRegistro).WithMany()
                .HasForeignKey(x => x.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.FichadaCorregida).WithMany()
                .HasForeignKey(x => x.FichadaCorregidaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Novedad>(e =>
        {
            e.HasIndex(x => new { x.EmpleadoId, x.FechaDesde });
            e.Property(x => x.Cantidad).HasPrecision(10, 2);
            e.HasOne(x => x.Empleado).WithMany(x => x.Novedades)
                .HasForeignKey(x => x.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CierreMensual).WithMany(c => c.Novedades)
                .HasForeignKey(x => x.CierreMensualId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<CierreMensual>(e =>
        {
            e.HasIndex(x => new { x.Anio, x.Mes }).IsUnique();
            e.HasOne(x => x.UsuarioCierre).WithMany()
                .HasForeignKey(x => x.UsuarioCierreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Feriado>(e =>
        {
            e.HasIndex(x => x.Fecha).IsUnique();
            e.Property(x => x.Descripcion).HasMaxLength(200);
        });

        b.Entity<ParametrosEmpresa>(e =>
        {
            e.Property(x => x.MultiplicadorExtraDiaHabil).HasPrecision(5, 2);
            e.Property(x => x.MultiplicadorExtraFeriado).HasPrecision(5, 2);
        });
    }
}
