using ControlHorario.Application.Interfaces;
using ControlHorario.Domain.Entities;
using ControlHorario.Domain.Enums;
using ControlHorario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControlHorario.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext ctx, IPasswordHasher hasher)
    {
        if (await ctx.Usuarios.AnyAsync()) return;

        // Parámetros de empresa
        var parametros = new ParametrosEmpresa
        {
            RazonSocial = "Pyme La Matanza SRL",
            Cuit = "30-12345678-9",
            EmailContador = "contador@estudio.com"
        };
        ctx.ParametrosEmpresa.Add(parametros);

        // Horarios
        var horarioFijo = new Horario
        {
            Nombre = "Comercio Lun-Vie 09 a 18",
            TipoJornada = TipoJornada.Completa,
            DiasLaborables = 1 + 2 + 4 + 8 + 16, // L M X J V
            HoraEntrada = new TimeSpan(9, 0, 0),
            HoraSalida = new TimeSpan(18, 0, 0),
            InicioDescanso = new TimeSpan(13, 0, 0),
            FinDescanso = new TimeSpan(14, 0, 0),
            MinutosMinimosDescanso = 30,
            ToleranciaEntradaMin = 5,
            ToleranciaSalidaMin = 5,
            UmbralHorasExtraMin = 15
        };
        var horarioParcial = new Horario
        {
            Nombre = "Media jornada Lun-Vie 14 a 18",
            TipoJornada = TipoJornada.Parcial,
            DiasLaborables = 1 + 2 + 4 + 8 + 16,
            HoraEntrada = new TimeSpan(14, 0, 0),
            HoraSalida = new TimeSpan(18, 0, 0),
            ToleranciaEntradaMin = 5,
            ToleranciaSalidaMin = 5,
            UmbralHorasExtraMin = 15
        };
        ctx.Horarios.AddRange(horarioFijo, horarioParcial);
        await ctx.SaveChangesAsync();

        // Empleados
        var emp1 = new Empleado
        {
            Legajo = "042", Nombre = "Juan", Apellido = "Pérez",
            Dni = "30123456", Cuil = "20-30123456-7",
            FechaIngreso = new DateTime(2022, 5, 1),
            CategoriaLaboral = "Vendedor", ConvenioColectivo = "CCT 130/75",
            TipoJornada = TipoJornada.Completa,
            Estado = EstadoEmpleado.Activo,
            HorarioId = horarioFijo.Id,
            Email = "jperez@pyme.com"
        };
        var emp2 = new Empleado
        {
            Legajo = "043", Nombre = "María", Apellido = "González",
            Dni = "32456789", Cuil = "27-32456789-4",
            FechaIngreso = new DateTime(2023, 2, 10),
            CategoriaLaboral = "Cajera", ConvenioColectivo = "CCT 130/75",
            TipoJornada = TipoJornada.Completa,
            Estado = EstadoEmpleado.Activo,
            HorarioId = horarioFijo.Id,
            Email = "mgonzalez@pyme.com"
        };
        var emp3 = new Empleado
        {
            Legajo = "044", Nombre = "Carlos", Apellido = "Rodríguez",
            Dni = "28987654", Cuil = "20-28987654-3",
            FechaIngreso = new DateTime(2021, 8, 15),
            CategoriaLaboral = "Encargado",
            TipoJornada = TipoJornada.Parcial,
            Estado = EstadoEmpleado.Activo,
            HorarioId = horarioParcial.Id,
            Email = "crodriguez@pyme.com"
        };
        ctx.Empleados.AddRange(emp1, emp2, emp3);
        await ctx.SaveChangesAsync();

        // Usuarios
        ctx.Usuarios.AddRange(
            new Usuario
            {
                Email = "admin@pyme.com", Nombre = "Administrador",
                Rol = RolUsuario.Administrador,
                PasswordHash = hasher.Hash("Admin123!")
            },
            new Usuario
            {
                Email = "jperez@pyme.com", Nombre = "Juan Pérez",
                Rol = RolUsuario.Empleado, EmpleadoId = emp1.Id,
                PasswordHash = hasher.Hash("Empleado1!")
            },
            new Usuario
            {
                Email = "contador@estudio.com", Nombre = "Contador Externo",
                Rol = RolUsuario.Contador,
                PasswordHash = hasher.Hash("Contador1!")
            });

        // Feriados 2026 (algunos)
        ctx.Feriados.AddRange(
            new Feriado { Fecha = new DateTime(2026, 1, 1), Descripcion = "Año Nuevo" },
            new Feriado { Fecha = new DateTime(2026, 5, 1), Descripcion = "Día del Trabajador" },
            new Feriado { Fecha = new DateTime(2026, 5, 25), Descripcion = "Revolución de Mayo" },
            new Feriado { Fecha = new DateTime(2026, 7, 9), Descripcion = "Día de la Independencia" },
            new Feriado { Fecha = new DateTime(2026, 12, 25), Descripcion = "Navidad" }
        );

        // Fichadas de muestra: el caso "Juan Pérez martes" del PDF
        var martes = ProximoMartes(DateTime.Today);
        ctx.Fichadas.AddRange(
            new Fichada { EmpleadoId = emp1.Id, Timestamp = martes.AddHours(9).AddMinutes(11), Tipo = TipoFichada.Entrada, Origen = OrigenFichada.Biometrico },
            new Fichada { EmpleadoId = emp1.Id, Timestamp = martes.AddHours(13).AddMinutes(5),  Tipo = TipoFichada.SalidaDescanso, Origen = OrigenFichada.Biometrico },
            new Fichada { EmpleadoId = emp1.Id, Timestamp = martes.AddHours(14).AddMinutes(22), Tipo = TipoFichada.RegresoDescanso, Origen = OrigenFichada.Biometrico },
            new Fichada { EmpleadoId = emp1.Id, Timestamp = martes.AddHours(19).AddMinutes(45), Tipo = TipoFichada.Salida, Origen = OrigenFichada.Biometrico }
        );

        await ctx.SaveChangesAsync();
    }

    private static DateTime ProximoMartes(DateTime desde)
    {
        // último martes pasado (para que esté en el mes actual)
        var d = desde.Date;
        while (d.DayOfWeek != DayOfWeek.Tuesday) d = d.AddDays(-1);
        return d;
    }
}
