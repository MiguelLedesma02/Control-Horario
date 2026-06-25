import { ReactNode } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import {
  LayoutDashboard, Users, Clock, FileWarning,
  CalendarCheck, LogOut, Building2, Calendar
} from 'lucide-react'

const navAdmin = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/empleados', label: 'Empleados', icon: Users },
  { to: '/horarios', label: 'Horarios', icon: Calendar },
  { to: '/fichadas', label: 'Fichadas', icon: Clock },
  { to: '/novedades', label: 'Novedades', icon: FileWarning },
  { to: '/cierre', label: 'Cierre Mensual', icon: CalendarCheck }
]
const navEmpleado = [
  { to: '/', label: 'Mi Panel', icon: LayoutDashboard },
  { to: '/mis-fichadas', label: 'Mis Fichadas', icon: Clock },
  { to: '/mis-novedades', label: 'Mis Novedades', icon: FileWarning }
]
const navContador = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/cierre', label: 'Preliquidaciones', icon: CalendarCheck }
]

export default function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  if (!user) { navigate('/login'); return null }

  const nav = user.rol === 'Administrador' ? navAdmin
            : user.rol === 'Empleado' ? navEmpleado
            : navContador

  return (
    <div className="min-h-screen flex">
      <aside className="w-60 shrink-0 bg-ink-900 border-r border-ink-700 flex flex-col">
        <div className="p-5 border-b border-ink-700">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-accent flex items-center justify-center">
              <Building2 className="w-5 h-5 text-ink-950" />
            </div>
            <div>
              <div className="font-display font-bold text-sm leading-none">CONTROL</div>
              <div className="font-display text-[10px] text-accent leading-none mt-1">HORARIO · SaaS</div>
            </div>
          </div>
        </div>
        <nav className="flex-1 p-3 space-y-1">
          {nav.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to} to={to} end
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                  isActive
                    ? 'bg-accent text-ink-950 font-semibold'
                    : 'text-ink-300 hover:bg-ink-800 hover:text-white'
                }`
              }
            >
              <Icon className="w-4 h-4" />
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="p-3 border-t border-ink-700">
          <div className="px-3 py-2 mb-2">
            <div className="text-xs text-ink-300 font-mono uppercase tracking-wider">{user.rol}</div>
            <div className="text-sm text-white truncate">{user.nombre}</div>
          </div>
          <button
            onClick={logout}
            className="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-ink-300 hover:bg-ink-800 hover:text-bad transition-colors"
          >
            <LogOut className="w-4 h-4" /> Salir
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-x-hidden">
        <div className="max-w-7xl mx-auto p-8 animate-fade-up">
          {children}
        </div>
      </main>
    </div>
  )
}
