import { useEffect, useState } from 'react'
import { Plus, FileWarning, Check, X, Filter } from 'lucide-react'
import { PageHeader, Card, Button, Spinner, Input, Select, Modal, EmptyState, Badge } from '../components/UI'
import { novedadService, empleadoService } from '../services/services'
import { fmtFecha, labelNovedad, colorEstado } from '../utils/format'
import { todayLocal, firstOfMonthLocal } from '../utils/datetime'
import type { Novedad, Empleado, EstadoNovedad } from '../types'

const tiposManuales = [
  'LicenciaEnfermedad', 'LicenciaExamen', 'LicenciaMaternidad',
  'VacacionesParciales', 'SuspensionConGoce', 'SuspensionSinGoce',
  'PermisoEspecial', 'JustificativoMedico'
]

export default function NovedadesPage() {
  const hoy = todayLocal()

  const [desde, setDesde] = useState(firstOfMonthLocal())
  const [hasta, setHasta] = useState(hoy)
  const [estado, setEstado] = useState<EstadoNovedad | ''>('')
  const [empleados, setEmpleados] = useState<Empleado[]>([])
  const [lista, setLista] = useState<Novedad[]>([])
  const [loading, setLoading] = useState(false)
  const [modal, setModal] = useState(false)
  const [modalRechazo, setModalRechazo] = useState<Novedad | null>(null)
  const [motivo, setMotivo] = useState('')
  const [form, setForm] = useState({
    empleadoId: 0, tipo: 'LicenciaEnfermedad',
    fechaDesde: hoy, fechaHasta: hoy, cantidad: 1, observacion: ''
  })

  useEffect(() => {
    empleadoService.getAll(true).then(e => {
      setEmpleados(e); if (e.length) setForm(f => ({ ...f, empleadoId: e[0].id }))
    })
  }, [])

  async function buscar() {
    setLoading(true)
    setLista(await novedadService.getByRango(desde, hasta, estado || undefined))
    setLoading(false)
  }
  useEffect(() => { buscar() }, [desde, hasta, estado])

  async function crear(ev: React.FormEvent) {
    ev.preventDefault()
    await novedadService.crear(
      Number(form.empleadoId), form.tipo as any,
      form.fechaDesde, form.fechaHasta, Number(form.cantidad), form.observacion
    )
    setModal(false); buscar()
  }

  async function aprobar(n: Novedad) {
    await novedadService.aprobar(n.id); buscar()
  }
  async function confirmarRechazo() {
    if (!modalRechazo) return
    await novedadService.rechazar(modalRechazo.id, motivo)
    setModalRechazo(null); setMotivo(''); buscar()
  }

  return (
    <>
      <PageHeader title="Novedades" subtitle="Eventos automáticos del motor + cargas manuales"
        action={<Button onClick={() => setModal(true)}><Plus className="w-4 h-4" /> Cargar manual</Button>} />

      <Card className="p-4 mb-6">
        <div className="flex flex-wrap gap-3 items-end">
          <Input label="Desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} />
          <Input label="Hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} />
          <Select label="Estado" value={estado} onChange={e => setEstado(e.target.value as any)}>
            <option value="">Todos</option>
            <option value="Pendiente">Pendientes</option>
            <option value="Aprobada">Aprobadas</option>
            <option value="Rechazada">Rechazadas</option>
          </Select>
          <Button variant="secondary" onClick={buscar}><Filter className="w-4 h-4" /> Filtrar</Button>
        </div>
      </Card>

      {loading ? <Spinner /> : lista.length === 0 ? (
        <EmptyState icon={FileWarning} title="Sin novedades en el rango"
          hint="Generá fichadas o ejecutá el recálculo desde Cierre Mensual" />
      ) : (
        <div className="space-y-2">
          {lista.map(n => (
            <Card key={n.id} className="p-4 flex items-start gap-4">
              <div className={`w-1 self-stretch rounded-full ${
                n.estado === 'Pendiente' ? 'bg-warn' :
                n.estado === 'Aprobada' ? 'bg-good' : 'bg-bad'
              }`} />
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap mb-1">
                  <span className="font-display font-bold text-white">{labelNovedad[n.tipo]}</span>
                  <Badge className={colorEstado[n.estado]}>{n.estado}</Badge>
                  <Badge className="bg-ink-700 text-ink-300 border-ink-600">{n.origen}</Badge>
                </div>
                <div className="text-sm text-ink-300 mb-1">
                  <span className="text-white">{n.empleadoNombre}</span>
                  <span> · {fmtFecha(n.fechaDesde)}</span>
                  {n.fechaDesde !== n.fechaHasta && <span> al {fmtFecha(n.fechaHasta)}</span>}
                  <span className="text-accent ml-2 font-mono">· {n.cantidad}</span>
                </div>
                {n.observacion && <div className="text-xs text-ink-300/70">{n.observacion}</div>}
              </div>
              {n.estado === 'Pendiente' && (
                <div className="flex gap-2">
                  <button onClick={() => aprobar(n)} className="p-2 bg-good/15 hover:bg-good/25 text-good rounded">
                    <Check className="w-4 h-4" />
                  </button>
                  <button onClick={() => setModalRechazo(n)} className="p-2 bg-bad/15 hover:bg-bad/25 text-bad rounded">
                    <X className="w-4 h-4" />
                  </button>
                </div>
              )}
            </Card>
          ))}
        </div>
      )}

      <Modal open={modal} onClose={() => setModal(false)} title="Cargar novedad manual">
        <form onSubmit={crear} className="space-y-3">
          <Select label="Empleado" value={form.empleadoId}
            onChange={e => setForm({ ...form, empleadoId: Number(e.target.value) })}>
            {empleados.map(em => <option key={em.id} value={em.id}>{em.apellido}, {em.nombre}</option>)}
          </Select>
          <Select label="Tipo de novedad" value={form.tipo}
            onChange={e => setForm({ ...form, tipo: e.target.value })}>
            {tiposManuales.map(t => <option key={t} value={t}>{labelNovedad[t as keyof typeof labelNovedad]}</option>)}
          </Select>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Desde" type="date" value={form.fechaDesde} required
              onChange={e => setForm({ ...form, fechaDesde: e.target.value })} />
            <Input label="Hasta" type="date" value={form.fechaHasta} required
              onChange={e => setForm({ ...form, fechaHasta: e.target.value })} />
          </div>
          <Input label="Cantidad (días o minutos)" type="number" value={form.cantidad} required
            onChange={e => setForm({ ...form, cantidad: Number(e.target.value) })} />
          <Input label="Observación" value={form.observacion}
            placeholder="ej. Certificado médico Dr. Pérez"
            onChange={e => setForm({ ...form, observacion: e.target.value })} />
          <div className="flex justify-end gap-2 pt-3">
            <Button variant="ghost" onClick={() => setModal(false)}>Cancelar</Button>
            <Button type="submit">Crear novedad</Button>
          </div>
        </form>
      </Modal>

      <Modal open={!!modalRechazo} onClose={() => setModalRechazo(null)} title="Rechazar novedad">
        <p className="text-sm text-ink-300 mb-3">
          Indicá el motivo de rechazo para {modalRechazo?.empleadoNombre}:
        </p>
        <Input label="Motivo" value={motivo} onChange={e => setMotivo(e.target.value)} />
        <div className="flex justify-end gap-2 mt-4">
          <Button variant="ghost" onClick={() => setModalRechazo(null)}>Cancelar</Button>
          <Button variant="danger" onClick={confirmarRechazo}>Rechazar</Button>
        </div>
      </Modal>
    </>
  )
}
