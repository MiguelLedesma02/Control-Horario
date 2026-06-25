import { useEffect, useState } from 'react'
import { Plus, Clock, Filter } from 'lucide-react'
import { PageHeader, Card, Button, Spinner, Input, Select, Modal, EmptyState, Badge } from '../components/UI'
import { fichadaService, empleadoService } from '../services/services'
import { fmtFecha, fmtHora } from '../utils/format'
import { nowLocalInput, todayLocal, daysAgoLocal, localInputToNaive } from '../utils/datetime'
import type { Fichada, Empleado } from '../types'

export default function FichadasPage() {
  const [desde, setDesde] = useState(daysAgoLocal(7))
  const [hasta, setHasta] = useState(todayLocal())
  const [empleados, setEmpleados] = useState<Empleado[]>([])
  const [filtroEmp, setFiltroEmp] = useState<number | null>(null)
  const [lista, setLista] = useState<Fichada[]>([])
  const [loading, setLoading] = useState(false)
  const [modal, setModal] = useState(false)
  const [form, setForm] = useState({
    empleadoId: 0, tipo: 'Entrada', timestamp: nowLocalInput(),
    observacion: ''
  })

  useEffect(() => {
    empleadoService.getAll(true).then(e => {
      setEmpleados(e)
      if (e.length) setForm(f => ({ ...f, empleadoId: e[0].id }))
    })
  }, [])

  async function buscar() {
    setLoading(true)
    setLista(await fichadaService.getByRango(desde, hasta))
    setLoading(false)
  }
  useEffect(() => { buscar() }, [desde, hasta])

  const filtrada = filtroEmp ? lista.filter(f => f.empleadoId === filtroEmp) : lista

  async function cargar(ev: React.FormEvent) {
    ev.preventDefault()
    await fichadaService.crear(
      Number(form.empleadoId),
      form.tipo as any,
      'Manual',
      localInputToNaive(form.timestamp),
      form.observacion
    )
    setModal(false)
    buscar()
  }

  function abrirModal() {
    setForm(f => ({ ...f, timestamp: nowLocalInput() }))
    setModal(true)
  }

  return (
    <>
      <PageHeader title="Fichadas" subtitle="Registro de eventos de entrada y salida"
        action={<Button onClick={abrirModal}><Plus className="w-4 h-4" /> Carga manual</Button>} />

      <Card className="p-4 mb-6">
        <div className="flex flex-wrap gap-3 items-end">
          <Input label="Desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} />
          <Input label="Hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} />
          <Select label="Empleado" value={filtroEmp || ''}
            onChange={e => setFiltroEmp(e.target.value ? Number(e.target.value) : null)}>
            <option value="">Todos</option>
            {empleados.map(em => <option key={em.id} value={em.id}>{em.apellido}, {em.nombre}</option>)}
          </Select>
          <Button variant="secondary" onClick={buscar}><Filter className="w-4 h-4" /> Filtrar</Button>
        </div>
      </Card>

      {loading ? <Spinner /> : filtrada.length === 0 ? (
        <EmptyState icon={Clock} title="Sin fichadas en el rango seleccionado" />
      ) : (
        <Card className="overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-ink-800">
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Fecha</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Hora</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Empleado</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Tipo</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Origen</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300 font-semibold tracking-wider">Observación</th>
              </tr>
            </thead>
            <tbody>
              {filtrada.map((f, i) => (
                <tr key={f.id} className={`border-t border-ink-700 ${i % 2 ? 'bg-ink-900' : 'bg-ink-900/50'}`}>
                  <td className="px-5 py-3 text-sm">{fmtFecha(f.timestamp)}</td>
                  <td className="px-5 py-3 font-mono text-accent">{fmtHora(f.timestamp)}</td>
                  <td className="px-5 py-3 text-sm">{f.empleadoNombre}</td>
                  <td className="px-5 py-3">
                    <Badge className={
                      f.tipo === 'Entrada' || f.tipo === 'RegresoDescanso'
                        ? 'bg-good/15 text-good border-good/30'
                        : 'bg-warn/15 text-warn border-warn/30'
                    }>{f.tipo}</Badge>
                  </td>
                  <td className="px-5 py-3 text-xs text-ink-300">{f.origen}</td>
                  <td className="px-5 py-3 text-xs text-ink-300">{f.observacion || '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      <Modal open={modal} onClose={() => setModal(false)} title="Cargar fichada manual">
        <form onSubmit={cargar} className="space-y-3">
          <Select label="Empleado" value={form.empleadoId}
            onChange={e => setForm({ ...form, empleadoId: Number(e.target.value) })}>
            {empleados.map(em => <option key={em.id} value={em.id}>{em.apellido}, {em.nombre} ({em.legajo})</option>)}
          </Select>
          <Select label="Tipo de fichada" value={form.tipo}
            onChange={e => setForm({ ...form, tipo: e.target.value })}>
            <option value="Entrada">Entrada</option>
            <option value="SalidaDescanso">Salida descanso</option>
            <option value="RegresoDescanso">Regreso descanso</option>
            <option value="Salida">Salida</option>
          </Select>
          <Input label="Fecha y hora" type="datetime-local" value={form.timestamp} required
            onChange={e => setForm({ ...form, timestamp: e.target.value })} />
          <Input label="Observación (motivo)" value={form.observacion}
            placeholder="ej. Olvidó fichar a la entrada"
            onChange={e => setForm({ ...form, observacion: e.target.value })} />
          <div className="flex justify-end gap-2 pt-3">
            <Button variant="ghost" onClick={() => setModal(false)}>Cancelar</Button>
            <Button type="submit">Registrar fichada</Button>
          </div>
        </form>
      </Modal>
    </>
  )
}
