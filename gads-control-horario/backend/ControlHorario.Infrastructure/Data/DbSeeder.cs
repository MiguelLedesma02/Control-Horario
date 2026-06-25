using ControlHorario.Application.Interfaces;
using ControlHorario.Application.Services;
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
            Horario = horarioFijo,
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
        var admin = new Usuario
        {
            Email = "admin@pyme.com", Nombre = "Administrador",
            Rol = RolUsuario.Administrador,
            PasswordHash = hasher.Hash("Admin123!")
        };
        var userJuan = new Usuario
        {
            Email = "jperez@pyme.com", Nombre = "Juan Pérez",
            Rol = RolUsuario.Empleado, EmpleadoId = emp1.Id,
            PasswordHash = hasher.Hash("Empleado1!")
        };
        var contador = new Usuario
        {
            Email = "contador@estudio.com", Nombre = "Contador Externo",
            Rol = RolUsuario.Contador,
            PasswordHash = hasher.Hash("Contador1!")
        };
        ctx.Usuarios.AddRange(admin, userJuan, contador);

        // Feriados 2026 (algunos)
        var feriados = new List<Feriado>
        {
            new() { Fecha = new DateTime(2026, 1, 1),  Descripcion = "Año Nuevo" },
            new() { Fecha = new DateTime(2026, 5, 1),  Descripcion = "Día del Trabajador" },
            new() { Fecha = new DateTime(2026, 5, 25), Descripcion = "Revolución de Mayo" },
            new() { Fecha = new DateTime(2026, 7, 9),  Descripcion = "Día de la Independencia" },
            new() { Fecha = new DateTime(2026, 12, 25), Descripcion = "Navidad" }
        };
        ctx.Feriados.AddRange(feriados);
        await ctx.SaveChangesAsync();

        // ─────────────────────────────────────────────────────────────
        // Data de demo para el lado EMPLEADO (Juan Pérez): fichadas del
        // mes con patrones variados que disparan cada regla del motor.
        // ─────────────────────────────────────────────────────────────
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        // Días hábiles (L-V) del mes corriente hasta hoy
        var habiles = new List<DateTime>();
        for (var d = inicioMes; d <= hoy; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                habiles.Add(d);

        var fichadas = new List<Fichada>();

        void Marca(DateTime dia, int h, int m, TipoFichada tipo, OrigenFichada origen = OrigenFichada.Biometrico)
            => fichadas.Add(new Fichada
            {
                EmpleadoId = emp1.Id,
                Timestamp = dia.AddHours(h).AddMinutes(m),
                Tipo = tipo,
                Origen = origen
            });

        // Jornada completa estándar: entrada / descanso 13-14 / salida 18
        void Jornada(DateTime dia, int entH, int entM, int salH, int salM, int regM = 0)
        {
            Marca(dia, entH, entM, TipoFichada.Entrada);
            Marca(dia, 13, 0, TipoFichada.SalidaDescanso);
            Marca(dia, 14, regM, TipoFichada.RegresoDescanso);
            Marca(dia, salH, salM, TipoFichada.Salida);
        }

        // Días sin fichadas: 7 = ausencia injustificada, 13/14 = licencia (justificada)
        var sinFichadas = new HashSet<int> { 7, 13, 14 };

        for (int i = 0; i < habiles.Count; i++)
        {
            var dia = habiles[i];

            // Hoy: jornada en curso, para que el panel muestre "fichadas de hoy"
            if (dia == hoy)
            {
                Marca(dia, 9, 3, TipoFichada.Entrada, OrigenFichada.PIN);
                if (DateTime.Now.Hour >= 13) Marca(dia, 13, 2, TipoFichada.SalidaDescanso, OrigenFichada.PIN);
                if (DateTime.Now.Hour >= 14) Marca(dia, 14, 0, TipoFichada.RegresoDescanso, OrigenFichada.PIN);
                continue;
            }

            if (sinFichadas.Contains(i)) continue; // ausencia / licencia: sin fichadas

            switch (i)
            {
                case 1: // Tardanza: llega 09:25 (+20 sobre tolerancia)
                    Jornada(dia, 9, 25, 18, 0);
                    break;
                case 3: // Descanso excedido: regreso 14:40 (+40)
                    Jornada(dia, 9, 0, 18, 0, regM: 40);
                    break;
                case 5: // Salida anticipada: 17:25 (-30)
                    Jornada(dia, 9, 0, 17, 25);
                    break;
                case 9: // Horas extra 50%: salida 19:50 (+110)
                    Jornada(dia, 9, 0, 19, 50);
                    break;
                case 11: // Doble fichada: dos Entradas en < 3 min
                    Marca(dia, 9, 0, TipoFichada.Entrada);
                    Marca(dia, 9, 2, TipoFichada.Entrada, OrigenFichada.PIN);
                    Marca(dia, 13, 0, TipoFichada.SalidaDescanso);
                    Marca(dia, 14, 0, TipoFichada.RegresoDescanso);
                    Marca(dia, 18, 0, TipoFichada.Salida);
                    break;
                default: // jornada normal sin novedades
                    Jornada(dia, 9, 0, 18, 0);
                    break;
            }
        }

        // Un sábado trabajado → Hora extra 100% (día de descanso)
        var sabado = hoy.AddDays(-1);
        while (sabado >= inicioMes && sabado.DayOfWeek != DayOfWeek.Saturday) sabado = sabado.AddDays(-1);
        if (sabado >= inicioMes && sabado.DayOfWeek == DayOfWeek.Saturday)
        {
            Marca(sabado, 10, 0, TipoFichada.Entrada);
            Marca(sabado, 14, 0, TipoFichada.Salida);
        }

        ctx.Fichadas.AddRange(fichadas);
        await ctx.SaveChangesAsync();

        // ─── Novedades manuales (cargadas/aprobadas por RRHH) ───
        var manuales = new List<Novedad>();

        // Licencia por enfermedad APROBADA (días 13-14, justifica la ausencia → sin "ausencia injustificada")
        if (habiles.Count > 14)
        {
            manuales.Add(new Novedad
            {
                EmpleadoId = emp1.Id, Tipo = TipoNovedad.LicenciaEnfermedad,
                Origen = OrigenNovedad.Manual, Estado = EstadoNovedad.Aprobada,
                FechaDesde = habiles[13].Date, FechaHasta = habiles[14].Date,
                Cantidad = 2, Observacion = "Reposo por gripe — certificado médico adjunto",
                UsuarioCreadorId = userJuan.Id, UsuarioRevisorId = admin.Id,
                FechaRevision = DateTime.UtcNow
            });
        }

        // Justificativo médico PENDIENTE (sobre un día trabajado normal)
        if (habiles.Count > 2)
        {
            manuales.Add(new Novedad
            {
                EmpleadoId = emp1.Id, Tipo = TipoNovedad.JustificativoMedico,
                Origen = OrigenNovedad.Manual, Estado = EstadoNovedad.Pendiente,
                FechaDesde = habiles[2].Date, FechaHasta = habiles[2].Date,
                Cantidad = 1, Observacion = "Turno médico por la mañana — adjunto constancia",
                UsuarioCreadorId = userJuan.Id
            });
        }

        // Permiso especial RECHAZADO
        if (habiles.Count > 4)
        {
            manuales.Add(new Novedad
            {
                EmpleadoId = emp1.Id, Tipo = TipoNovedad.PermisoEspecial,
                Origen = OrigenNovedad.Manual, Estado = EstadoNovedad.Rechazada,
                FechaDesde = habiles[4].Date, FechaHasta = habiles[4].Date,
                Cantidad = 1, Observacion = "Solicitud de día por trámite personal",
                MotivoRechazo = "No corresponde: el trámite puede realizarse fuera del horario laboral",
                UsuarioCreadorId = userJuan.Id, UsuarioRevisorId = admin.Id,
                FechaRevision = DateTime.UtcNow
            });
        }

        // ─── Novedades automáticas (las genera el MOTOR DE REGLAS real) ───
        var motor = new MotorReglasService(parametros, feriados);
        var automaticas = motor.EvaluarPeriodo(emp1, fichadas, inicioMes, hoy,
            manuales.Where(n => n.Estado == EstadoNovedad.Aprobada));

        // Estados variados para la demo (aprobar/rechazar algunas)
        foreach (var n in automaticas)
        {
            switch (n.Tipo)
            {
                case TipoNovedad.HoraExtra50:
                case TipoNovedad.HoraExtra100:
                case TipoNovedad.Tardanza:
                    n.Estado = EstadoNovedad.Aprobada;
                    n.UsuarioRevisorId = admin.Id;
                    n.FechaRevision = DateTime.UtcNow;
                    break;
                case TipoNovedad.SalidaAnticipada:
                    n.Estado = EstadoNovedad.Rechazada;
                    n.UsuarioRevisorId = admin.Id;
                    n.FechaRevision = DateTime.UtcNow;
                    n.MotivoRechazo = "Salida autorizada por el encargado";
                    break;
                // Ausencia, DescansoExcedido, DobleFichada → quedan Pendientes
            }
        }

        ctx.Novedades.AddRange(automaticas);
        ctx.Novedades.AddRange(manuales);
        await ctx.SaveChangesAsync();
    }
}
