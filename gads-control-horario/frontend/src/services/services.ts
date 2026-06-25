import api from './api'
import { nowArgNaive } from '../utils/datetime'
import type {
  LoginResponse, Empleado, Horario, Fichada, Novedad,
  ResumenEmpleado, CierreMensual, TipoFichada, OrigenFichada,
  TipoNovedad, EstadoNovedad
} from '../types'

export const authService = {
  login: (email: string, password: string) =>
    api.post<LoginResponse>('/auth/login', { email, password }).then(r => r.data)
}

export const empleadoService = {
  getAll: (soloActivos = false) =>
    api.get<Empleado[]>('/empleados', { params: { soloActivos } }).then(r => r.data),
  getById: (id: number) =>
    api.get<Empleado>(`/empleados/${id}`).then(r => r.data),
  crear: (data: Partial<Empleado>) =>
    api.post<Empleado>('/empleados', data).then(r => r.data),
  actualizar: (id: number, data: Partial<Empleado>) =>
    api.put(`/empleados/${id}`, data),
  desactivar: (id: number) =>
    api.post(`/empleados/${id}/desactivar`),
  eliminar: (id: number) =>
    api.delete(`/empleados/${id}`),
  importar: (lista: Partial<Empleado>[]) =>
    api.post<{ importados: number; omitidos: string[] }>('/empleados/importar', lista).then(r => r.data),
}

export const horarioService = {
  getAll: () => api.get<Horario[]>('/horarios').then(r => r.data),
  crear: (data: Partial<Horario>) => api.post<Horario>('/horarios', data).then(r => r.data),
  actualizar: (id: number, data: Partial<Horario>) => api.put(`/horarios/${id}`, data),
  eliminar: (id: number) => api.delete(`/horarios/${id}`),
}

export const fichadaService = {
  getByRango: (desde: string, hasta: string) =>
    api.get<Fichada[]>('/fichadas', { params: { desde, hasta } }).then(r => r.data),
  getByEmpleado: (empleadoId: number, desde: string, hasta: string) =>
    api.get<Fichada[]>(`/fichadas/empleado/${empleadoId}`,
      { params: { desde, hasta } }).then(r => r.data),
  crear: (empleadoId: number, tipo: TipoFichada, origen: OrigenFichada,
          timestamp = nowArgNaive(), observacion?: string) =>
    api.post('/fichadas', { empleadoId, tipo, origen, timestamp, observacion }).then(r => r.data)
}

export const novedadService = {
  getByRango: (desde: string, hasta: string, estado?: EstadoNovedad) =>
    api.get<Novedad[]>('/novedades',
      { params: { desde, hasta, estado } }).then(r => r.data),
  getPendientes: () =>
    api.get<Novedad[]>('/novedades/pendientes').then(r => r.data),
  getByEmpleado: (empleadoId: number, desde: string, hasta: string) =>
    api.get<Novedad[]>(`/novedades/empleado/${empleadoId}`,
      { params: { desde, hasta } }).then(r => r.data),
  crear: (empleadoId: number, tipo: TipoNovedad, fechaDesde: string,
          fechaHasta: string, cantidad: number, observacion?: string) =>
    api.post('/novedades', { empleadoId, tipo, fechaDesde, fechaHasta, cantidad, observacion }),
  aprobar: (id: number) => api.post(`/novedades/${id}/aprobar`),
  rechazar: (id: number, motivo: string) =>
    api.post(`/novedades/${id}/rechazar`, { motivo })
}

export const cierreService = {
  getAll: () => api.get<CierreMensual[]>('/cierre').then(r => r.data),
  getResumen: (anio: number, mes: number) =>
    api.get<ResumenEmpleado[]>(`/cierre/${anio}/${mes}/resumen`).then(r => r.data),
  recalcular: (anio: number, mes: number) =>
    api.post(`/cierre/${anio}/${mes}/recalcular`),
  cerrar: (anio: number, mes: number) =>
    api.post(`/cierre/${anio}/${mes}/cerrar`),
  exportar: (anio: number, mes: number, formato: 'xlsx' | 'csv' | 'pdf') =>
    api.get(`/cierre/${anio}/${mes}/exportar/${formato}`, { responseType: 'blob' })
}
