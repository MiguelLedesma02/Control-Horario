export type Rol = 'Administrador' | 'Empleado' | 'Contador'

export interface LoginResponse {
  token: string
  nombre: string
  rol: Rol
  empleadoId: number | null
}

export type EstadoEmpleado = 'Activo' | 'Inactivo' | 'Suspendido'
export type TipoJornada = 'Completa' | 'Parcial' | 'Flexible' | 'Rotativa'

export interface Empleado {
  id: number
  legajo: string
  nombre: string
  apellido: string
  dni: string
  cuil: string
  fechaIngreso: string
  categoriaLaboral: string
  convenioColectivo: string | null
  tipoJornada: TipoJornada
  estado: EstadoEmpleado
  horarioId: number
  horarioNombre: string
  email: string | null
  telefono: string | null
}

export interface Horario {
  id: number
  nombre: string
  tipoJornada: TipoJornada
  diasLaborables: number
  horaEntrada: string
  horaSalida: string
  inicioDescanso: string | null
  finDescanso: string | null
  minutosMinimosDescanso: number
  toleranciaEntradaMin: number
  toleranciaSalidaMin: number
  umbralHorasExtraMin: number
}

export type TipoFichada = 'Entrada' | 'Salida' | 'SalidaDescanso' | 'RegresoDescanso'
export type OrigenFichada = 'Biometrico' | 'QR' | 'PIN' | 'Manual' | 'ApiExterna'

export interface Fichada {
  id: number
  empleadoId: number
  empleadoNombre: string
  timestamp: string
  tipo: TipoFichada
  origen: OrigenFichada
  observacion: string | null
  esCorreccion: boolean
}

export type TipoNovedad =
  | 'Tardanza' | 'AusenciaInjustificada' | 'HoraExtra50' | 'HoraExtra100'
  | 'SalidaAnticipada' | 'DobleFichada' | 'DescansoExcedido' | 'DescansoNoTomado'
  | 'LicenciaEnfermedad' | 'LicenciaExamen' | 'LicenciaMaternidad'
  | 'VacacionesParciales' | 'SuspensionConGoce' | 'SuspensionSinGoce'
  | 'PermisoEspecial' | 'JustificativoMedico'

export type EstadoNovedad = 'Pendiente' | 'Aprobada' | 'Rechazada'
export type OrigenNovedad = 'Automatica' | 'Manual'

export interface Novedad {
  id: number
  empleadoId: number
  empleadoNombre: string
  tipo: TipoNovedad
  origen: OrigenNovedad
  estado: EstadoNovedad
  fechaDesde: string
  fechaHasta: string
  cantidad: number
  observacion: string | null
  fechaCreacion: string
}

export interface ResumenEmpleado {
  empleadoId: number
  legajo: string
  nombreCompleto: string
  diasTrabajados: number
  diasAusenteJustificado: number
  diasAusenteInjustificado: number
  minutosTardanza: number
  minutosHorasExtra50: number
  minutosHorasExtra100: number
  diasLicencia: number
  diasVacaciones: number
  novedades: Novedad[]
}

export interface CierreMensual {
  id: number
  anio: number
  mes: number
  fechaCierre: string
  estado: 'Borrador' | 'Cerrado'
  cerrador: string
  cantidadEmpleados: number
  cantidadNovedades: number
}
