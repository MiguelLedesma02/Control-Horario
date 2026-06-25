import { useEffect, useState } from 'react'
import { Plus, Calendar, Clock, Pencil, Trash2 } from 'lucide-react'
import { PageHeader, Card, Button, Spinner, Input, Select, Modal, EmptyState, Badge } from '../components/UI'
import { horarioService } from '../services/services'
import type { Horario } from '../types'

const dias = [
  { bit: 1, nombre: 'L' }, { bit: 2, nombre: 'M' }, { bit: 4, nombre: 'X' },
  { bit: 8, nombre: 'J' }, { bit: 16, nombre: 'V' }, { bit: 32, nombre: 'S' }, { bit: 64, nombre: 'D' }
]

const formVacio = {
  nombre: '', tipoJornada: 'Completa',
  horaEntrada: '09:00', horaSalida: '18:00',
  inicioDescanso: '13:00', finDescanso: '14:00',
  minutosMinimosDescanso: 30,
  diasLaborables: 1 + 2 + 4 + 8 + 16,
  toleranciaEntradaMin: 5, toleranciaSalidaMin: 5, umbralHorasExtraMin: 15
}

function toTime(ts: string) { return ts?.substring(0, 5) ?? '' }
function fromForm(form: typeof formVacio) {
  return {
    ...form,
    horaEntrada: form.horaEntrada + ':00',
    horaSalida: form.horaSalida + ':00',
    inicioDescanso: form.inicioDescanso ? form.inicioDescanso + ':00' : null,
    finDescanso: form.finDescanso ? form.finDescanso + ':00' : null,
    minutosMinimosDescanso: Number(form.minutosMinimosDescanso),
    toleranciaEntradaMin: Number(form.toleranciaEntradaMin),
    toleranciaSalidaMin: Number(form.toleranciaSalidaMin),
    umbralHorasExtraMin: Number(form.umbralHorasExtraMin)
  }
}

export default function HorariosPage() {
  const [lista, setLista] = useState<Horario[]>([])
  const [loading, setLoading] = useState(true)
  const [modal, setModal] = useState(false)
  const [editando, setEditando] = useState<Horario | null>(null)
  const [form, setForm] = useState(formVacio)

  async function cargar() {
    setLoading(true)
    setLista(await horarioService.getAll())
    setLoading(false)
  }
  useEffect(() => { cargar() }, [])

  function abrirNuevo() {
    setEditando(null)
    setForm(formVacio)
    setModal(true)
  }

  function abrirEditar(h: Horario) {
    setEditando(h)
    setForm({
      nombre: h.nombre,
      tipoJornada: h.tipoJornada,
      horaEntrada: toTime(h.horaEntrada as unknown as string),
      horaSalida: toTime(h.horaSalida as unknown as string),
      inicioDescanso: h.inicioDescanso ? toTime(h.inicioDescanso as unknown as string) : '',
      finDescanso: h.finDescanso ? toTime(h.finDescanso as unknown as string) : '',
      minutosMinimosDescanso: h.minutosMinimosDescanso,
      diasLaborables: h.diasLaborables,
      toleranciaEntradaMin: h.toleranciaEntradaMin,
      toleranciaSalidaMin: h.toleranciaSalidaMin,
      umbralHorasExtraMin: h.umbralHorasExtraMin,
    })
    setModal(true)
  }

  function toggleDia(bit: number) {
    setForm(f => ({ ...f, diasLaborables: f.diasLaborables ^ bit }))
  }

  async function guardar(ev: React.FormEvent) {
    ev.preventDefault()
    const data = fromForm(form)
    try {
      if (editando) await horarioService.actualizar(editando.id, data)
      else await horarioService.crear(data)
      setModal(false)
      cargar()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Error al guardar el horario')
    }
  }

  async function eliminar(h: Horario) {
    if (!confirm(`¿Eliminar el horario "${h.nombre}"?\n\nSólo es posible si no tiene empleados asignados.`)) return
    try {
      await horarioService.eliminar(h.id)
      cargar()
    } catch (err: any) {
      alert(err.response?.data?.message || 'No se puede eliminar: el horario tiene empleados asignados.')
    }
  }

  if (loading) return <Spinner />

  return (
    <>
      <PageHeader title="Horarios y turnos" subtitle="Plantillas que se asignan a cada empleado"
        action={<Button onClick={abrirNuevo}><Plus className="w-4 h-4" /> Nuevo horario</Button>} />

      {lista.length === 0 ? (
        <EmptyState icon={Calendar} title="Sin horarios definidos" />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {lista.map(h => (
            <Card key={h.id} className="p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="font-display font-bold text-lg text-white">{h.nombre}</h3>
                  <Badge className="mt-1 bg-accent/15 text-accent border-accent/30">{h.tipoJornada}</Badge>
                </div>
                <div className="flex items-center gap-1">
                  <button onClick={() => abrirEditar(h)} title="Editar turno"
                    className="p-1.5 hover:bg-ink-700 rounded text-ink-300 hover:text-white transition-colors">
                    <Pencil className="w-4 h-4" />
                  </button>
                  <button onClick={() => eliminar(h)} title="Eliminar turno"
                    className="p-1.5 hover:bg-ink-700 rounded text-ink-300 hover:text-bad transition-colors">
                    <Trash2 className="w-4 h-4" />
                  </button>
                  <Clock className="w-5 h-5 text-ink-600 ml-1" />
                </div>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-ink-300">Entrada</span>
                  <span className="font-mono text-white">{toTime(h.horaEntrada as unknown as string)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-ink-300">Salida</span>
                  <span className="font-mono text-white">{toTime(h.horaSalida as unknown as string)}</span>
                </div>
                {h.inicioDescanso && (
                  <div className="flex justify-between">
                    <span className="text-ink-300">Descanso</span>
                    <span className="font-mono text-white">
                      {toTime(h.inicioDescanso as unknown as string)} – {toTime(h.finDescanso as unknown as string)}
                    </span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span className="text-ink-300">Tolerancia</span>
                  <span className="font-mono text-white">±{h.toleranciaEntradaMin}m</span>
                </div>
                <div className="flex gap-1 pt-2">
                  {dias.map(d => (
                    <span key={d.bit} className={`w-7 h-7 rounded text-xs font-bold flex items-center justify-center ${
                      (h.diasLaborables & d.bit) ? 'bg-accent text-ink-950' : 'bg-ink-800 text-ink-600'
                    }`}>{d.nombre}</span>
                  ))}
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal open={modal} onClose={() => setModal(false)}
        title={editando ? `Editar horario — ${editando.nombre}` : 'Nuevo horario'}>
        <form onSubmit={guardar} className="space-y-3">
          <Input label="Nombre" value={form.nombre} required
            onChange={e => setForm({ ...form, nombre: e.target.value })} />
          <Select label="Tipo de jornada" value={form.tipoJornada}
            onChange={e => setForm({ ...form, tipoJornada: e.target.value })}>
            <option>Completa</option><option>Parcial</option>
            <option>Flexible</option><option>Rotativa</option>
          </Select>

          <div>
            <span className="block text-xs text-ink-300 uppercase tracking-wider mb-1.5">Días laborables</span>
            <div className="flex gap-1.5">
              {dias.map(d => (
                <button key={d.bit} type="button" onClick={() => toggleDia(d.bit)}
                  className={`w-10 h-10 rounded font-bold transition-colors ${
                    (form.diasLaborables & d.bit) ? 'bg-accent text-ink-950' : 'bg-ink-800 text-ink-300'
                  }`}>{d.nombre}</button>
              ))}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="Hora entrada" type="time" value={form.horaEntrada} required
              onChange={e => setForm({ ...form, horaEntrada: e.target.value })} />
            <Input label="Hora salida" type="time" value={form.horaSalida} required
              onChange={e => setForm({ ...form, horaSalida: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Inicio descanso" type="time" value={form.inicioDescanso}
              onChange={e => setForm({ ...form, inicioDescanso: e.target.value })} />
            <Input label="Fin descanso" type="time" value={form.finDescanso}
              onChange={e => setForm({ ...form, finDescanso: e.target.value })} />
          </div>
          <div className="grid grid-cols-3 gap-3">
            <Input label="Tol. entrada (min)" type="number" value={form.toleranciaEntradaMin}
              onChange={e => setForm({ ...form, toleranciaEntradaMin: Number(e.target.value) })} />
            <Input label="Tol. salida (min)" type="number" value={form.toleranciaSalidaMin}
              onChange={e => setForm({ ...form, toleranciaSalidaMin: Number(e.target.value) })} />
            <Input label="Umbral HS extra" type="number" value={form.umbralHorasExtraMin}
              onChange={e => setForm({ ...form, umbralHorasExtraMin: Number(e.target.value) })} />
          </div>

          <div className="flex justify-end gap-2 pt-3">
            <Button variant="ghost" onClick={() => setModal(false)}>Cancelar</Button>
            <Button type="submit">{editando ? 'Guardar cambios' : 'Crear horario'}</Button>
          </div>
        </form>
      </Modal>
    </>
  )
}
