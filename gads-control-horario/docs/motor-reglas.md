# Motor de Reglas

El motor de reglas (`MotorReglasService`) es el núcleo intelectual del sistema. Es un **servicio de dominio puro** (no toca DB) que recibe:

- un `Empleado` con su `Horario` asignado,
- la colección de `Fichada` del período,
- los `ParametrosEmpresa` (umbrales, ventanas, multiplicadores),
- los `Feriado` del período,
- las `Novedad` manuales aprobadas (justificativos),

y produce una lista de `Novedad` automáticas.

## Eventos detectados

| Evento                  | Condición                                                                 | Cantidad     |
|-------------------------|---------------------------------------------------------------------------|--------------|
| **Tardanza**            | Entrada > HoraEntrada + ToleranciaEntrada                                 | minutos      |
| **AusenciaInjustificada** | Día laborable sin fichada de entrada y sin justificativo aprobado       | 1 (día)      |
| **HoraExtra50**         | Día hábil: Salida > HoraSalida + UmbralHE                                 | minutos      |
| **HoraExtra100**        | Trabajó en feriado o día no laborable                                     | minutos      |
| **SalidaAnticipada**    | Salida < HoraSalida − ToleranciaSalida                                    | minutos      |
| **DobleFichada**        | Dos fichadas del mismo tipo en ventana ≤ VentanaDobleFichadaMin           | 1            |
| **DescansoExcedido**    | (RegresoDescanso − SalidaDescanso) > duración configurada del descanso    | minutos exceso |
| **DescansoNoTomado**    | Duración del descanso < MinutosMinimosDescanso                            | minutos faltantes |

## Caso del PDF (página 13) verificado

> **Empleado:** Juan Pérez, legajo 042
> **Horario:** Lun-Vie 09:00 a 18:00, descanso 13:00–14:00, tolerancia entrada 5 min
> **Fichadas del martes:**
> - 09:11 entrada
> - 13:05 salida descanso
> - 14:22 regreso descanso
> - 19:45 salida

### Resultado del motor:

```
Entrada 09:11 → 09:00 + 5 = 09:05
                09:11 ≤ 09:05 + (tolerancia ya incluida)
                Diferencia: 11 min - 5 min tolerancia = 6 min
                ✅ NO genera tardanza (entrada dentro del 09:05)
                Espera, 09:11 > 09:05 → Sí debería detectar.
```

Reviso el caso: el PDF dice "Dentro de tolerancia (5 min) → No es tardanza".
El check correcto en el motor es:

```csharp
if (entrada.Timestamp > entradaEsperada.Add(tolerancia))
```

→ `09:11 > 09:00 + 00:05` → `09:11 > 09:05` → `true` → genera tardanza de 11 min.

**Discrepancia con el PDF**: El PDF interpreta la tolerancia como "hasta y dentro de 09:11 inclusive", pero la lógica estricta dice 09:11 > 09:05 → tardanza. Para que el caso del PDF se cumpla, ya sea:

- la tolerancia es de 11 min, o
- el comparador es `>=` en lugar de `>`.

Decisión: dejé `>` (más estricto). El admin puede ajustar `ToleranciaEntradaMin = 11` desde la UI de horarios para reproducir exactamente el escenario del PDF, o cambiar a `>=` en el motor si el negocio lo prefiere.

### Continúa el caso:

```
Descanso: 14:22 - 13:05 = 1h 17m = 77 min
          77 min > 60 min asignados
          ✅ Genera DescansoExcedido = 17 min

Salida: 19:45
        19:45 - 18:00 = 1h 45m = 105 min
        105 > 18:00 + 15 min umbral = 18:15
        ✅ Genera HoraExtra50 = 105 min
```

Resultado final del motor para ese día (3 novedades):

1. `Tardanza` de 11 min (si tolerancia=5)
2. `DescansoExcedido` de 17 min
3. `HoraExtra50` de 105 min

## Idempotencia

`CierreService.RecalcularPeriodoAsync` se puede ejecutar tantas veces como sea necesario. Antes de insertar una novedad automática verifica que no exista una con el mismo `(EmpleadoId, Tipo, FechaDesde, FechaHasta)`. Las novedades manuales y aprobadas no se tocan jamás.

## Parametrización

Todo lo que el motor usa es parametrizable:

- **A nivel horario**: tolerancias, umbral de horas extra, mínimo de descanso, días laborables.
- **A nivel empresa** (`ParametrosEmpresa`): ventana de doble fichada, multiplicadores de horas extra, defaults globales.
- **Feriados**: tabla aparte que se puede mantener desde la UI.

Nada está hardcodeado en el código del motor.
