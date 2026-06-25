import type { TipoNovedad, EstadoNovedad } from '../types'

export const fmtFecha = (iso: string) => {
  const d = new Date(iso)
  return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

export const fmtHora = (iso: string) => {
  const d = new Date(iso)
  return d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })
}

export const fmtFechaHora = (iso: string) => `${fmtFecha(iso)} ${fmtHora(iso)}`

export const minutosAHoras = (min: number): string => {
  if (!min) return '0h'
  const h = Math.floor(min / 60)
  const m = min % 60
  return h > 0 ? `${h}h ${m}m` : `${m}m`
}

export const labelNovedad: Record<TipoNovedad, string> = {
  Tardanza: 'Tardanza',
  AusenciaInjustificada: 'Ausencia injustificada',
  HoraExtra50: 'Horas extra 50%',
  HoraExtra100: 'Horas extra 100%',
  SalidaAnticipada: 'Salida anticipada',
  DobleFichada: 'Doble fichada',
  DescansoExcedido: 'Descanso excedido',
  DescansoNoTomado: 'Descanso no tomado',
  LicenciaEnfermedad: 'Licencia por enfermedad',
  LicenciaExamen: 'Licencia por examen',
  LicenciaMaternidad: 'Licencia por maternidad',
  VacacionesParciales: 'Vacaciones parciales',
  SuspensionConGoce: 'Suspensión c/ goce',
  SuspensionSinGoce: 'Suspensión s/ goce',
  PermisoEspecial: 'Permiso especial',
  JustificativoMedico: 'Justificativo médico'
}

export const colorEstado: Record<EstadoNovedad, string> = {
  Pendiente: 'bg-warn/15 text-warn border-warn/30',
  Aprobada: 'bg-good/15 text-good border-good/30',
  Rechazada: 'bg-bad/15 text-bad border-bad/30'
}

export const meses = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]
