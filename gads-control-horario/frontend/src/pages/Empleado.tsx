import { useEffect, useState } from 'react'
import { Clock, FileWarning } from 'lucide-react'
import { PageHeader, Card, Spinner, Input, Badge, EmptyState } from '../components/UI'
import { fichadaService, novedadService } from '../services/services'
import { useAuth } from '../context/AuthContext'
import { fmtFecha, fmtHora, labelNovedad, colorEstado } from '../utils/format'
import { todayLocal, firstOfMonthLocal } from '../utils/datetime'
import type { Fichada, Novedad } from '../types'

export function MisFichadas() {
  const { user } = useAuth()
  const [desde, setDesde] = useState(firstOfMonthLocal())
  const [hasta, setHasta] = useState(todayLocal())
  const [lista, setLista] = useState<Fichada[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user?.empleadoId) return
    setLoading(true)
    fichadaService.getByEmpleado(user.empleadoId, desde, hasta)
      .then(setLista).finally(() => setLoading(false))
  }, [user, desde, hasta])

  return (
    <>
      <PageHeader title="Mis fichadas" subtitle="Tus eventos de entrada y salida" />
      <Card className="p-4 mb-6 flex gap-3 items-end">
        <Input label="Desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} />
        <Input label="Hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} />
      </Card>
      {loading ? <Spinner /> : lista.length === 0 ? (
        <EmptyState icon={Clock} title="Sin fichadas en el período" />
      ) : (
        <Card className="overflow-hidden">
          <table className="w-full">
            <thead className="bg-ink-800">
              <tr>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300">Fecha</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300">Hora</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300">Tipo</th>
                <th className="text-left px-5 py-3 text-xs uppercase text-ink-300">Origen</th>
              </tr>
            </thead>
            <tbody>
              {lista.map((f, i) => (
                <tr key={f.id} className={`border-t border-ink-700 ${i % 2 ? 'bg-ink-900' : ''}`}>
                  <td className="px-5 py-3 text-sm">{fmtFecha(f.timestamp)}</td>
                  <td className="px-5 py-3 font-mono text-accent">{fmtHora(f.timestamp)}</td>
                  <td className="px-5 py-3">
                    <Badge className={
                      f.tipo === 'Entrada' || f.tipo === 'RegresoDescanso'
                        ? 'bg-good/15 text-good border-good/30'
                        : 'bg-warn/15 text-warn border-warn/30'
                    }>{f.tipo}</Badge>
                  </td>
                  <td className="px-5 py-3 text-xs text-ink-300">{f.origen}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </>
  )
}

export function MisNovedades() {
  const { user } = useAuth()
  const [desde, setDesde] = useState(firstOfMonthLocal())
  const [hasta, setHasta] = useState(todayLocal())
  const [lista, setLista] = useState<Novedad[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user?.empleadoId) return
    setLoading(true)
    novedadService.getByEmpleado(user.empleadoId, desde, hasta)
      .then(setLista).finally(() => setLoading(false))
  }, [user, desde, hasta])

  return (
    <>
      <PageHeader title="Mis novedades" subtitle="Lo que el sistema detectó sobre tu jornada" />
      <Card className="p-4 mb-6 flex gap-3 items-end">
        <Input label="Desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} />
        <Input label="Hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} />
      </Card>
      {loading ? <Spinner /> : lista.length === 0 ? (
        <EmptyState icon={FileWarning} title="Sin novedades en el período" />
      ) : (
        <div className="space-y-2">
          {lista.map(n => (
            <Card key={n.id} className="p-4">
              <div className="flex items-center gap-2 flex-wrap mb-1">
                <span className="font-display font-bold text-white">{labelNovedad[n.tipo]}</span>
                <Badge className={colorEstado[n.estado]}>{n.estado}</Badge>
              </div>
              <div className="text-sm text-ink-300">
                {fmtFecha(n.fechaDesde)}
                {n.fechaDesde !== n.fechaHasta && ` al ${fmtFecha(n.fechaHasta)}`}
                <span className="text-accent font-mono ml-2">· {n.cantidad}</span>
              </div>
              {n.observacion && <div className="text-xs text-ink-300/70 mt-1">{n.observacion}</div>}
            </Card>
          ))}
        </div>
      )}
    </>
  )
}
