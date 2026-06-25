/**
 * Utilidades de fecha/hora para zona horaria Argentina (ART = UTC-3, sin DST).
 *
 * Estrategia: nunca usamos Date.toISOString() (que siempre emite UTC).
 * En su lugar construimos strings con offset explícito "-03:00" para que
 * el backend reciba la hora local correcta.
 */

const TZ = 'America/Argentina/Buenos_Aires'

/** Fecha/hora local Argentina como "YYYY-MM-DDTHH:mm" (para datetime-local input) */
export function nowLocalInput(): string {
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ,
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit',
  }).format(new Date()).replace(' ', 'T')
}

/** Fecha local Argentina como "YYYY-MM-DD" (para date inputs y filtros) */
export function todayLocal(): string {
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ, year: 'numeric', month: '2-digit', day: '2-digit'
  }).format(new Date())
}

/** Hace N días en fecha local Argentina "YYYY-MM-DD" */
export function daysAgoLocal(n: number): string {
  const d = new Date(Date.now() - n * 86_400_000)
  return new Intl.DateTimeFormat('sv-SE', {
    timeZone: TZ, year: 'numeric', month: '2-digit', day: '2-digit'
  }).format(d)
}

/**
 * Convierte el valor de un <input type="datetime-local"> (ej. "2025-06-20T09:30")
 * en un string ISO con offset ARG explícito: "2025-06-20T09:30:00-03:00".
 * Así el backend recibe la hora que el usuario ingresó, sin conversión UTC.
 */
export function localInputToArgISO(datetimeLocal: string): string {
  return datetimeLocal.length === 16
    ? `${datetimeLocal}:00-03:00`
    : `${datetimeLocal}-03:00`
}
