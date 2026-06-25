# Casos de Uso Documentados

## Actores

| Actor          | Rol técnico        | Acceso |
|----------------|--------------------|--------|
| Administrador  | `Administrador`    | Total |
| Empleado       | `Empleado`         | Solo sus datos + fichar |
| Contador       | `Contador`         | Solo lectura sobre cierres y resúmenes |

## CU-01 — Cargar empleado

**Actor:** Administrador
**Precondición:** Existe al menos un Horario.

**Flujo principal:**
1. Admin entra a "Empleados" y presiona "Nuevo empleado"
2. Completa: legajo, nombre, apellido, DNI, CUIL, fecha ingreso, categoría, convenio, tipo de jornada, horario asignado, email
3. Sistema valida unicidad de legajo
4. Sistema crea el empleado con estado `Activo`

**Flujos alternativos:**
- 3a. Legajo duplicado → 409 Conflict, error visible al usuario
- Importación masiva por Excel (a implementar)
- Alta vía API REST externa (endpoint disponible)

## CU-02 — Definir un horario

**Actor:** Administrador

1. Admin entra a "Horarios" y presiona "Nuevo horario"
2. Completa: nombre, tipo (Completa/Parcial/Flexible/Rotativa), días laborables (toggles), hora entrada/salida, descanso opcional, tolerancias, umbral de horas extra
3. Sistema crea el horario y queda disponible para asignar a empleados

## CU-03 — Fichar entrada/salida (empleado)

**Actor:** Empleado

1. Empleado se loguea en su panel
2. Sistema muestra reloj en vivo y 4 botones: Entrada, Salida descanso, Regreso descanso, Salida
3. Empleado presiona "Entrada"
4. Sistema registra la fichada con `Origen=PIN` (origen de UI directa) y la asocia a su `EmpleadoId`
5. Sistema actualiza la lista de fichadas del día

**Notas de seguridad:**
- El backend valida que el `EmpleadoId` enviado coincida con el del JWT (no se puede fichar por otro)
- En producción el origen se podría cambiar a `Biometrico` si la PC tiene lector

## CU-04 — Fichada manual por administrador

**Actor:** Administrador
**Caso típico:** un empleado olvidó fichar.

1. Admin entra a "Fichadas" → "Carga manual"
2. Selecciona empleado, tipo de fichada, fecha+hora exacta, observación
3. Sistema crea la fichada con `Origen=Manual` y `UsuarioRegistroId` con el ID del admin
4. Queda registrada como manual con trazabilidad de quién la cargó

## CU-05 — Cargar API externa de reloj biométrico

**Actor:** Sistema de terceros (vía POST autenticado)

1. Reloj biométrico envía `POST /api/fichadas/api-externa` con `EmpleadoId` y timestamp
2. Sistema valida JWT (token de servicio)
3. Sistema crea la fichada con `Origen=ApiExterna`

## CU-06 — Cargar novedad manual (licencia)

**Actor:** Administrador

1. Admin entra a "Novedades" → "Cargar manual"
2. Selecciona empleado, tipo (LicenciaEnfermedad, etc.), rango de fechas, cantidad, observación
3. Sistema crea la novedad con `Estado=Pendiente`, `Origen=Manual`
4. Queda esperando aprobación

## CU-07 — Aprobar/rechazar novedades

**Actor:** Administrador

1. Admin entra a "Novedades" y filtra por estado=Pendiente
2. Para cada novedad ve el detalle y puede:
   - Click ✓ → aprueba (queda `Estado=Aprobada` y se incluirá en el cierre)
   - Click ✗ → ingresa motivo y rechaza (queda `Estado=Rechazada`, no se incluye)
3. El sistema registra `UsuarioRevisorId`, `FechaRevision` y eventualmente `MotivoRechazo`

## CU-08 — Recalcular novedades automáticas

**Actor:** Administrador

1. Admin va a "Cierre Mensual" y selecciona año/mes
2. Click en "Recalcular novedades"
3. Sistema corre `MotorReglasService.EvaluarPeriodo` para cada empleado
4. Sistema compara con novedades existentes y agrega solo las nuevas
5. Sistema muestra cantidad de novedades generadas

## CU-09 — Cerrar el período mensual

**Actor:** Administrador

1. Admin selecciona año/mes y verifica que no haya novedades pendientes
2. Click en "Cerrar período"
3. Sistema corre recálculo final (idempotente)
4. Sistema verifica que NO haya novedades en estado `Pendiente`
   - Si las hay → 400 Bad Request "Hay X novedades pendientes"
5. Sistema genera el snapshot JSON con resumen consolidado
6. Sistema crea (o actualiza) el `CierreMensual` con `Estado=Cerrado`
7. Sistema asocia todas las novedades aprobadas al cierre

**Postcondición:** El cierre queda inmutable. El snapshot JSON garantiza que aunque después se modifiquen novedades, lo reportado al contador queda registrado.

## CU-10 — Exportar al contador

**Actor:** Administrador o Contador

1. Usuario selecciona año/mes
2. Click en uno de los tres botones: Excel, CSV o PDF
3. Sistema genera el archivo en memoria con `ExportadorService`:
   - **Excel** (ClosedXML): hoja resumen + hoja detalle de novedades, formato profesional
   - **CSV** (CsvHelper): formato simple con `;` como separador, listo para importar a sistemas de liquidación
   - **PDF** (QuestPDF): tabla apaisada con cabecera y paginación
4. Browser descarga el archivo

## CU-11 — Vista del contador (solo lectura)

**Actor:** Contador

1. Contador se loguea con su rol
2. Solo ve dos opciones de menú: Dashboard y Preliquidaciones
3. Puede consultar resumen de cualquier período y exportar
4. NO puede crear/modificar empleados, fichadas ni novedades
