/**
 * Utilidades de fecha/hora para zona horaria Argentina (ART = UTC-3, sin DST).
 *
 * El backend guarda la hora como DateTime "naive" (sin zona). Por eso NUNCA
 * usamos Date.toISOString() (que convierte a UTC y corre la hora 3 horas).
 * En su lugar construimos siempre el "reloj de pared" en hora Argentina.
 */

export const TZ_ARG = 'America/Argentina/Buenos_Aires'
const TZ = TZ_ARG

/** Fecha/hora actual de Argentina como "YYYY-MM-DDTHH:mm" (para <input datetime-local>). */
export function nowLocalInput(): string {
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ,
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date()).replace(' ', 'T')
}

/**
 * Fecha/hora actual de Argentina como "YYYY-MM-DDTHH:mm:ss" naive (sin Z ni offset).
 * Es lo que se manda al backend para registrar una fichada en tiempo real.
 */
export function nowArgNaive(): string {
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ,
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  }).format(new Date()).replace(' ', 'T')
}

/** Fecha de hoy en Argentina como "YYYY-MM-DD" (para inputs date y filtros). */
export function todayLocal(): string {
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ, year: 'numeric', month: '2-digit', day: '2-digit',
  }).format(new Date())
}

/** Hace N días en fecha de Argentina como "YYYY-MM-DD". */
export function daysAgoLocal(n: number): string {
  const d = new Date(Date.now() - n * 86_400_000)
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ, year: 'numeric', month: '2-digit', day: '2-digit',
  }).format(d)
}

/** Primer día del mes actual (hora Argentina) como "YYYY-MM-DD". */
export function firstOfMonthLocal(): string {
  return `${todayLocal().substring(0, 7)}-01`
}

/** Hora actual de Argentina como "HH:mm" (para relojes en vivo). */
export function horaActualArg(): string {
  return new Intl.DateTimeFormat('es-AR', {
    timeZone: TZ, hour: '2-digit', minute: '2-digit',
  }).format(new Date())
}

/**
 * Convierte el valor de un <input type="datetime-local"> (ej. "2025-06-20T09:30")
 * en un string naive con segundos: "2025-06-20T09:30:00".
 * Se manda así, sin Z ni offset, para que el backend lo guarde tal cual (hora ARG).
 */
export function localInputToNaive(datetimeLocal: string): string {
  return datetimeLocal.length === 16 ? `${datetimeLocal}:00` : datetimeLocal
}
