# Modelo de Datos

## Diagrama Entidad-Relación (lógico)

```
┌─────────────────┐         ┌──────────────────┐
│    Usuario      │   *──1  │     Empleado     │ 1──1  ┌─────────┐
│─────────────────│◄────────│──────────────────│──────►│ Horario │
│ Id              │         │ Id               │       └─────────┘
│ Email (UQ)      │         │ Legajo (UQ)      │            ▲
│ PasswordHash    │         │ Nombre, Apellido │            │ N
│ Rol             │         │ DNI, CUIL        │            │
│ EmpleadoId? ────┼─────────│ FechaIngreso     │      ┌─────┴────┐
└─────────────────┘         │ Categoría        │      │ Empleados│
                            │ Estado           │      └──────────┘
                            │ HorarioId        │
                            └──────────────────┘
                                    │1
                                    │
                             ┌──────┴───────┐
                             │              │
                            *▼             *▼
                     ┌──────────────┐ ┌────────────────┐
                     │   Fichada    │ │    Novedad     │
                     │──────────────│ │────────────────│
                     │ Id           │ │ Id             │
                     │ EmpleadoId   │ │ EmpleadoId     │
                     │ Timestamp    │ │ Tipo           │
                     │ Tipo         │ │ Origen         │
                     │ Origen       │ │ Estado         │
                     │ FichadaCorr? │ │ FechaDesde     │
                     └──────────────┘ │ FechaHasta     │
                                      │ Cantidad       │
                                      │ CierreId? ─────┼──┐
                                      └────────────────┘  │
                                                          │
                                                  ┌───────▼─────────┐
                                                  │ CierreMensual   │
                                                  │─────────────────│
                                                  │ Id              │
                                                  │ Anio, Mes (UQ)  │
                                                  │ FechaCierre     │
                                                  │ Estado          │
                                                  │ SnapshotJson    │
                                                  └─────────────────┘
```

## Decisiones de diseño clave

### 1. Fichada como dato crudo inmutable

Una `Fichada` representa un evento físico ocurrido en el mundo real. Una vez insertada **nunca se modifica**. Si hay un error, se inserta una nueva fichada con `FichadaCorregidaId` apuntando a la original. De esta forma:

- Hay trazabilidad completa de qué se fichó originalmente.
- Las correcciones se ven en auditoría (quién, cuándo, por qué).
- El motor de reglas puede recalcular en cualquier momento sin perder historia.

### 2. Separación entre Fichada (dato) y Novedad (interpretación)

Las `Novedad` son **derivadas** de las `Fichada` mediante el motor de reglas. Esto permite:

- Cambiar las reglas (umbrales, tolerancias) y recalcular sin perder los eventos originales.
- Que un usuario admin agregue novedades manuales (licencias, justificativos) que coexisten con las automáticas.
- Workflow de aprobación independiente del registro físico.

### 3. Cierre mensual con snapshot inmutable

El campo `SnapshotJson` en `CierreMensual` guarda el resumen consolidado **al momento exacto del cierre**. Aunque después se modifiquen novedades, el snapshot sigue reflejando lo que se reportó al contador. Garantiza la **inmutabilidad regulatoria** del cierre.

### 4. `DiasLaborables` como bitmask

```
Lunes     = 1
Martes    = 2
Miércoles = 4
Jueves    = 8
Viernes   = 16
Sábado    = 32
Domingo   = 64
```

Lun a Vie = `1+2+4+8+16 = 31`. Permite consultar con un AND bit a bit y configurar cualquier combinación de días en un solo entero.

### 5. Roles y autorización

Los roles `Administrador`, `Empleado`, `Contador` se almacenan como enum en `Usuario.Rol` y se proyectan al claim JWT. Cada controller usa `[Authorize(Roles = "...")]` para restringir endpoints. El empleado solo puede acceder a **sus propias** fichadas y novedades (chequeo adicional comparando `EmpleadoId` del claim).

### 6. Multi-empresa

La versión actual es **single-tenant** (una sola empresa por instalación), como pide el alcance del TP. Para multi-tenant se agregaría `EmpresaId` como FK en todas las tablas y se filtraría por claim. La tabla `ParametrosEmpresa` ya está modelada para sostener esa configuración global.
