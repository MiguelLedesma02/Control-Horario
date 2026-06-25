import { useEffect, useState } from 'react'
import { CalendarCheck, RefreshCw, Lock, FileSpreadsheet, FileText, FileType } from 'lucide-react'
import { PageHeader, Card, Button, Spinner, Select, EmptyState } from '../components/UI'
import { cierreService } from '../services/services'
import { meses, minutosAHoras } from '../utils/format'
import type { ResumenEmpleado } from '../types'
import { useAuth } from '../context/AuthContext'

export default function CierrePage() {
  const { user } = useAuth()
  const ahora = new Date()
  const [anio, setAnio] = useState(ahora.getFullYear())
  const [mes, setMes] = useState(ahora.getMonth() + 1)
  const [resumen, setResumen] = useState<ResumenEmpleado[]>([])
  const [loading, setLoading] = useState(false)
  const [trabajando, setTrabajando] = useState('')

  async function cargar() {
    setLoading(true)
    setResumen(await cierreService.getResumen(anio, mes))
    setLoading(false)
  }
  useEffect(() => { cargar() }, [anio, mes])

  async function recalcular() {
    setTrabajando('recalc')
    try {
      const { data } = await cierreService.recalcular(anio, mes) as any
      alert(`Se generaron ${data.novedadesGeneradas} novedades nuevas`)
      cargar()
    } finally { setTrabajando('') }
  }

  async function cerrar() {
    if (!confirm('¿Cerrar el período? Una vez cerrado no se puede modificar.')) return
    setTrabajando('cerrar')
    try {
      await cierreService.cerrar(anio, mes)
      alert('Período cerrado correctamente')
      cargar()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Error al cerrar')
    } finally { setTrabajando('') }
  }

  async function exportar(formato: 'xlsx' | 'csv' | 'pdf') {
    setTrabajando(formato)
    try {
      const resp = await cierreService.exportar(anio, mes, formato)
      const url = URL.createObjectURL(new Blob([resp.data]))
      const a = document.createElement('a')
      a.href = url
      a.download = `preliquidacion_${anio}_${String(mes).padStart(2, '0')}.${formato}`
      a.click()
      URL.revokeObjectURL(url)
    } finally { setTrabajando('') }
  }

  const totales = resumen.reduce((acc, r) => ({
    trabajados: acc.trabajados + r.diasTrabajados,
    ausInj: acc.ausInj + r.diasAusenteInjustificado,
    he50: acc.he50 + r.minutosHorasExtra50,
    he100: acc.he100 + r.minutosHorasExtra100,
    tardanzas: acc.tardanzas + r.minutosTardanza
  }), { trabajados: 0, ausInj: 0, he50: 0, he100: 0, tardanzas: 0 })

  const esAdmin = user?.rol === 'Administrador'

  return (
    <>
      <PageHeader title="Cierre mensual" subtitle="Consolidación y exportación al contador" />

      <Card className="p-5 mb-6">
        <div className="flex flex-wrap items-end gap-3">
          <Select label="Mes" value={mes} onChange={e => setMes(Number(e.target.value))}>
            {meses.map((m, i) => <option key={i} value={i + 1}>{m}</option>)}
          </Select>
          <Select label="Año" value={anio} onChange={e => setAnio(Number(e.target.value))}>
            {[ahora.getFullYear(), ahora.getFullYear() - 1, ahora.getFullYear() - 2].map(y =>
              <option key={y} value={y}>{y}</option>
            )}
          </Select>
          <div className="flex-1" />
          {esAdmin && (
            <>
              <Button variant="secondary" onClick={recalcular} disabled={!!trabajando}>
                <RefreshCw className={`w-4 h-4 ${trabajando === 'recalc' ? 'animate-spin' : ''}`} />
                Recalcular novedades
              </Button>
              <Button onClick={cerrar} disabled={!!trabajando}>
                <Lock className="w-4 h-4" /> Cerrar período
              </Button>
            </>
          )}
        </div>
      </Card>

      {/* Totales */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mb-6">
        <TotalCard label="Días trabajados" value={totales.trabajados} accent />
        <TotalCard label="Aus. injustif." value={totales.ausInj} />
        <TotalCard label="Tardanzas" value={minutosAHoras(totales.tardanzas)} />
        <TotalCard label="HE 50%" value={minutosAHoras(totales.he50)} />
        <TotalCard label="HE 100%" value={minutosAHoras(totales.he100)} />
      </div>

      {/* Exportar */}
      <Card className="p-5 mb-6">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <div className="font-mono text-xs text-accent uppercase tracking-widest">→ Para el contador</div>
            <h3 className="font-display text-lg font-bold mt-1">Exportar preliquidación</h3>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => exportar('xlsx')} disabled={!!trabajando}>
              <FileSpreadsheet className="w-4 h-4" /> Excel
            </Button>
            <Button variant="secondary" onClick={() => exportar('csv')} disabled={!!trabajando}>
              <FileText className="w-4 h-4" /> CSV
            </Button>
            <Button variant="secondary" onClick={() => exportar('pdf')} disabled={!!trabajando}>
              <FileType className="w-4 h-4" /> PDF
            </Button>
          </div>
        </div>
      </Card>

      {/* Detalle por empleado */}
      {loading ? <Spinner /> : resumen.length === 0 ? (
        <EmptyState icon={CalendarCheck} title="Sin empleados activos en el período" />
      ) : (
        <Card className="overflow-x-auto">
          <table className="w-full min-w-[900px]">
            <thead className="bg-ink-800">
              <tr>
                <th className="text-left px-4 py-3 text-xs uppercase text-ink-300 font-semibold">Legajo</th>
                <th className="text-left px-4 py-3 text-xs uppercase text-ink-300 font-semibold">Empleado</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Trab.</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Aus.J.</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Aus.I.</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Tard.</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">HE 50%</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">HE 100%</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Lic.</th>
                <th className="text-center px-3 py-3 text-xs uppercase text-ink-300 font-semibold">Vac.</th>
              </tr>
            </thead>
            <tbody>
              {resumen.map((r, i) => (
                <tr key={r.empleadoId} className={`border-t border-ink-700 ${i % 2 ? 'bg-ink-900' : 'bg-ink-900/50'}`}>
                  <td className="px-4 py-3 font-mono text-accent text-sm">{r.legajo}</td>
                  <td className="px-4 py-3 text-sm text-white">{r.nombreCompleto}</td>
                  <td className="px-3 py-3 text-center font-mono">{r.diasTrabajados}</td>
                  <td className="px-3 py-3 text-center font-mono text-ink-300">{r.diasAusenteJustificado}</td>
                  <td className={`px-3 py-3 text-center font-mono ${r.diasAusenteInjustificado ? 'text-bad' : 'text-ink-300'}`}>{r.diasAusenteInjustificado}</td>
                  <td className={`px-3 py-3 text-center font-mono ${r.minutosTardanza ? 'text-warn' : 'text-ink-300'}`}>{r.minutosTardanza}m</td>
                  <td className={`px-3 py-3 text-center font-mono ${r.minutosHorasExtra50 ? 'text-accent' : 'text-ink-300'}`}>{minutosAHoras(r.minutosHorasExtra50)}</td>
                  <td className={`px-3 py-3 text-center font-mono ${r.minutosHorasExtra100 ? 'text-good' : 'text-ink-300'}`}>{minutosAHoras(r.minutosHorasExtra100)}</td>
                  <td className="px-3 py-3 text-center font-mono text-ink-300">{r.diasLicencia}</td>
                  <td className="px-3 py-3 text-center font-mono text-ink-300">{r.diasVacaciones}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </>
  )
}

function TotalCard({ label, value, accent = false }: { label: string; value: any; accent?: boolean }) {
  return (
    <Card className="p-4">
      <div className="text-xs text-ink-300 uppercase tracking-wider">{label}</div>
      <div className={`font-display font-bold mt-1 text-2xl ${accent ? 'text-accent' : 'text-white'}`}>{value}</div>
    </Card>
  )
}
