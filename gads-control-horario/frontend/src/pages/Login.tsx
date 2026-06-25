import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { Building2, ArrowRight } from 'lucide-react'

const usuariosDemo = [
  { rol: 'Administrador', email: 'admin@pyme.com', password: 'Admin123!' },
  { rol: 'Empleado', email: 'jperez@pyme.com', password: 'Empleado1!' },
  { rol: 'Contador', email: 'contador@estudio.com', password: 'Contador1!' }
]

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@pyme.com')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(''); setLoading(true)
    try {
      await login(email, password)
      navigate('/')
    } catch {
      setError('Credenciales inválidas')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen grid grid-cols-1 lg:grid-cols-2">
      {/* Panel izquierdo: branding */}
      <div className="hidden lg:flex relative overflow-hidden bg-ink-900 p-12 flex-col justify-between">
        <div className="absolute inset-0 opacity-30 bg-gradient-to-br from-accent/20 via-transparent to-good/10" />
        <div className="absolute top-0 right-0 w-96 h-96 rounded-full bg-accent/5 blur-3xl" />
        <div className="absolute bottom-20 left-10 w-64 h-64 rounded-full bg-good/5 blur-3xl" />

        <div className="relative">
          <div className="flex items-center gap-3 mb-12">
            <div className="w-10 h-10 rounded-lg bg-accent flex items-center justify-center">
              <Building2 className="w-6 h-6 text-ink-950" />
            </div>
            <div>
              <div className="font-display font-bold text-white text-lg leading-none">CONTROL HORARIO</div>
              <div className="font-display text-xs text-accent leading-none mt-1">SaaS para pymes</div>
            </div>
          </div>

          <h1 className="font-display text-5xl font-bold text-white leading-tight">
            Tu pyme, sin<br/>papeles ni Excel.
          </h1>
          <p className="text-ink-300 mt-6 max-w-md">
            Centralizá fichadas, interpretá novedades y entregale al contador
            una preliquidación lista para liquidar. Cero discusiones a fin de mes.
          </p>
        </div>

        <div className="relative space-y-3">
          <div className="text-xs font-mono text-ink-300 uppercase tracking-widest">
            ◆ Credenciales de demo
          </div>
          {usuariosDemo.map(u => (
            <button key={u.email}
              onClick={() => { setEmail(u.email); setPassword(u.password) }}
              className="w-full text-left p-3 bg-ink-800/60 border border-ink-700 rounded-lg hover:border-accent transition-colors group">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-xs text-accent font-mono uppercase">{u.rol}</div>
                  <div className="text-sm text-white">{u.email}</div>
                </div>
                <ArrowRight className="w-4 h-4 text-ink-300 group-hover:text-accent transition-colors" />
              </div>
            </button>
          ))}
        </div>
      </div>

      {/* Panel derecho: form */}
      <div className="flex items-center justify-center p-8">
        <form onSubmit={onSubmit} className="w-full max-w-sm">
          <div className="lg:hidden flex items-center gap-2 mb-8 justify-center">
            <div className="w-10 h-10 rounded-lg bg-accent flex items-center justify-center">
              <Building2 className="w-6 h-6 text-ink-950" />
            </div>
            <div className="font-display font-bold text-white text-lg">CONTROL HORARIO</div>
          </div>

          <div className="font-mono text-xs text-accent uppercase tracking-widest mb-2">→ Iniciar sesión</div>
          <h2 className="font-display text-3xl font-bold text-white mb-8">
            Ingresá a tu cuenta
          </h2>

          {error && (
            <div className="mb-4 p-3 bg-bad/10 border border-bad/30 rounded-lg text-sm text-bad">
              {error}
            </div>
          )}

          <div className="space-y-4">
            <label className="block">
              <span className="block text-xs text-ink-300 uppercase tracking-wider mb-1.5">Email</span>
              <input value={email} onChange={e => setEmail(e.target.value)}
                type="email" required
                className="w-full bg-ink-800 border border-ink-700 rounded-lg px-3 py-2.5 text-sm text-white focus:border-accent focus:outline-none" />
            </label>

            <label className="block">
              <span className="block text-xs text-ink-300 uppercase tracking-wider mb-1.5">Contraseña</span>
              <input value={password} onChange={e => setPassword(e.target.value)}
                type="password" required
                className="w-full bg-ink-800 border border-ink-700 rounded-lg px-3 py-2.5 text-sm text-white focus:border-accent focus:outline-none" />
            </label>
          </div>

          <button type="submit" disabled={loading}
            className="w-full mt-6 bg-accent text-ink-950 font-bold py-3 rounded-lg hover:bg-accent-muted transition-colors disabled:opacity-50">
            {loading ? 'Ingresando…' : 'Iniciar sesión'}
          </button>

          <p className="text-xs text-ink-300/60 text-center mt-8">
            UNLaM · Ingeniería en Informática · Trabajo Práctico GADS
          </p>
        </form>
      </div>
    </div>
  )
}
