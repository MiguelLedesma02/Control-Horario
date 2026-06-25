import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Users, Clock, FileWarning, CheckCircle2, ArrowRight, AlertTriangle } from 'lucide-react'
import { Card, PageHeader, Stat, Badge, Spinner, Button } from '../components/UI'
import { empleadoService, novedadService, fichadaService } from '../services/services'
import { useAuth } from '../context/AuthContext'
import { fmtHora, fmtFecha, labelNovedad, colorEstado, minutosAHoras } from '../utils/format'
import type { Empleado, Novedad, Fichada } from '../types'

export default function Dashboard() {
  const { user } = useAuth()
  const [empleados, setEmpleados] = useState<Empleado[]>([])
  const [pendientes, setPendientes] = useState<Novedad[]>([])
  const [fichadasHoy, setFichadasHoy] = useState<Fichada[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (user?.rol !== 'Administrador') { setLoading(false); return }
    const hoy = new Date().toISOString().split('T')[0]
    Promise.all([
      empleadoService.getAll(true),
      novedadService.getPendientes(),
      fichadaService.getByRango(hoy, hoy)
    ]).then(([e, n, f]) => {
      setEmpleados(e); setPendientes(n); setFichadasHoy(f)
    }).finally(() => setLoading(false))
  }, [user])

  if (user?.rol === 'Empleado') return <DashboardEmpleado />
  if (user?.rol === 'Contador') return <DashboardContador />
  if (loading) return <Spinner />

  const totalHE50 = pendientes.filter(n => n.tipo === 'HoraExtra50').reduce((s, n) => s + n.cantidad, 0)
  const totalTardanzas = pendientes.filter(n => n.tipo === 'Tardanza').length

  return (
    <>
      <PageHeader
        title={`Buenas, ${user!.nombre.split(' ')[0]}.`}
        subtitle={`Hoy es ${new Date().toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long' })}`}
      />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <Stat label="Empleados activos" value={empleados.length} hint={`${empleados.filter(e => e.estado === 'Activo').length} en planta`} />
        <Stat label="Fichadas hoy" value={fichadasHoy.length} hint="Eventos registrados" accent />
        <Stat label="Novedades pendientes" value={pendientes.length} hint={`${totalTardanzas} tardanzas detectadas`} />
        <Stat label="HS extra a aprobar" value={minutosAHoras(totalHE50)} hint="Sumadas de pendientes 50%" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Novedades pendientes */}
        <Card className="lg:col-span-2 p-6">
          <div className="flex items-center justify-between mb-5">
            <div>
              <div className="font-mono text-xs text-accent uppercase tracking-widest">→ Bandeja</div>
              <h2 className="font-display text-xl font-bold text-white mt-1">Novedades pendientes</h2>
            </div>
            <Link to="/novedades">
              <Button variant="ghost">Ver todas <ArrowRight className="w-3 h-3" /></Button>
            </Link>
          </div>

          {pendientes.length === 0 ? (
            <div className="text-center py-10">
              <CheckCircle2 className="w-10 h-10 text-good mx-auto mb-3" />
              <div className="text-ink-300">Sin novedades pendientes</div>
            </div>
          ) : (
            <div className="space-y-2">
              {pendientes.slice(0, 6).map(n => (
                <div key={n.id} className="flex items-center justify-between p-3 bg-ink-800/50 rounded-lg hover:bg-ink-800 transition-colors">
                  <div className="flex-1">
                    <div className="text-sm text-white font-medium">{n.empleadoNombre}</div>
                    <div className="text-xs text-ink-300">
                      {labelNovedad[n.tipo]} · {fmtFecha(n.fechaDesde)}
                    </div>
                  </div>
                  <Badge className={colorEstado[n.estado]}>{n.estado}</Badge>
                </div>
              ))}
            </div>
          )}
        </Card>

        {/* Acciones rápidas */}
        <Card className="p-6">
          <div className="font-mono text-xs text-accent uppercase tracking-widest">→ Acciones</div>
          <h2 className="font-display text-xl font-bold text-white mt-1 mb-5">Operaciones rápidas</h2>

          <div className="space-y-3">
            <QuickAction to="/empleados" icon={Users} label="Gestionar empleados" />
            <QuickAction to="/fichadas" icon={Clock} label="Cargar fichada manual" />
            <QuickAction to="/novedades" icon={FileWarning} label="Aprobar novedades" />
            <QuickAction to="/cierre" icon={CheckCircle2} label="Iniciar cierre mensual" />
          </div>

          {pendientes.length > 5 && (
            <div className="mt-5 p-3 bg-warn/10 border border-warn/30 rounded-lg flex gap-2">
              <AlertTriangle className="w-4 h-4 text-warn shrink-0 mt-0.5" />
              <div className="text-xs text-warn">
                Tenés {pendientes.length} novedades sin revisar. No vas a poder cerrar el mes hasta resolverlas.
              </div>
            </div>
          )}
        </Card>
      </div>
    </>
  )
}

function QuickAction({ to, icon: Icon, label }: { to: string; icon: any; label: string }) {
  return (
    <Link to={to} className="flex items-center gap-3 p-3 bg-ink-800/50 rounded-lg hover:bg-ink-800 hover:border-accent border border-transparent transition-all group">
      <Icon className="w-4 h-4 text-accent" />
      <span className="text-sm text-white flex-1">{label}</span>
      <ArrowRight className="w-4 h-4 text-ink-300 group-hover:text-accent transition-colors" />
    </Link>
  )
}

function DashboardEmpleado() {
  const { user } = useAuth()
  const [misFichadas, setMisFichadas] = useState<Fichada[]>([])
  const [misNovedades, setMisNovedades] = useState<Novedad[]>([])
  const [loading, setLoading] = useState(true)
  const [marcando, setMarcando] = useState(false)

  useEffect(() => {
    if (!user?.empleadoId) return
    const hoy = new Date().toISOString().split('T')[0]
    const inicioMes = new Date()
    inicioMes.setDate(1)
    Promise.all([
      fichadaService.getByEmpleado(user.empleadoId, hoy, hoy),
      novedadService.getByEmpleado(user.empleadoId, inicioMes.toISOString().split('T')[0], hoy)
    ]).then(([f, n]) => { setMisFichadas(f); setMisNovedades(n) })
      .finally(() => setLoading(false))
  }, [user, marcando])

  async function marcar(tipo: any) {
    if (!user?.empleadoId) return
    setMarcando(true)
    await fichadaService.crear(user.empleadoId, tipo, 'PIN')
    setMarcando(false)
  }

  if (loading) return <Spinner />

  const yaMarcoEntrada = misFichadas.some(f => f.tipo === 'Entrada')
  const yaMarcoSalida = misFichadas.some(f => f.tipo === 'Salida')

  return (
    <>
      <PageHeader title={`Hola, ${user!.nombre.split(' ')[0]}`} subtitle="Tu panel personal" />

      <Card className="p-8 mb-8 text-center bg-gradient-to-br from-ink-900 to-ink-800">
        <div className="font-mono text-xs text-accent uppercase tracking-widest mb-3">→ Marcá tu fichada</div>
        <div className="font-display text-6xl font-bold text-white mb-2">
          {new Date().toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })}
        </div>
        <div className="text-ink-300 mb-6">{new Date().toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}</div>
        <div className="flex gap-3 justify-center flex-wrap">
          <Button onClick={() => marcar('Entrada')} disabled={marcando || yaMarcoEntrada}>Entrada</Button>
          <Button variant="secondary" onClick={() => marcar('SalidaDescanso')} disabled={marcando}>Salida descanso</Button>
          <Button variant="secondary" onClick={() => marcar('RegresoDescanso')} disabled={marcando}>Regreso descanso</Button>
          <Button onClick={() => marcar('Salida')} disabled={marcando || yaMarcoSalida}>Salida</Button>
        </div>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card className="p-6">
          <h3 className="font-display text-lg font-bold mb-4">Fichadas de hoy</h3>
          {misFichadas.length === 0 ? (
            <div className="text-ink-300 text-sm">Aún no fichaste hoy</div>
          ) : (
            <div className="space-y-2">
              {misFichadas.map(f => (
                <div key={f.id} className="flex items-center justify-between p-3 bg-ink-800/50 rounded-lg">
                  <div>
                    <div className="text-sm text-white">{f.tipo}</div>
                    <div className="text-xs text-ink-300">{f.origen}</div>
                  </div>
                  <div className="font-mono text-accent">{fmtHora(f.timestamp)}</div>
                </div>
              ))}
            </div>
          )}
        </Card>

        <Card className="p-6">
          <h3 className="font-display text-lg font-bold mb-4">Mis novedades del mes</h3>
          {misNovedades.length === 0 ? (
            <div className="text-ink-300 text-sm">Sin novedades este mes</div>
          ) : (
            <div className="space-y-2">
              {misNovedades.slice(0, 5).map(n => (
                <div key={n.id} className="flex items-center justify-between p-3 bg-ink-800/50 rounded-lg">
                  <div className="flex-1 min-w-0">
                    <div className="text-sm text-white truncate">{labelNovedad[n.tipo]}</div>
                    <div className="text-xs text-ink-300">{fmtFecha(n.fechaDesde)}</div>
                  </div>
                  <Badge className={colorEstado[n.estado]}>{n.estado}</Badge>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>
    </>
  )
}

function DashboardContador() {
  return (
    <>
      <PageHeader title="Panel del contador" subtitle="Acceso de solo lectura a preliquidaciones" />
      <Card className="p-8 text-center">
        <CheckCircle2 className="w-12 h-12 text-good mx-auto mb-4" />
        <h3 className="font-display text-xl font-bold mb-2">Todo en orden</h3>
        <p className="text-ink-300 mb-6">Accedé al módulo de Preliquidaciones para ver y exportar los cierres mensuales.</p>
        <Link to="/cierre"><Button>Ir a Preliquidaciones <ArrowRight className="w-4 h-4" /></Button></Link>
      </Card>
    </>
  )
}
