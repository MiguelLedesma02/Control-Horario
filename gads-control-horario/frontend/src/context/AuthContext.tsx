import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import type { LoginResponse, Rol } from '../types'
import { authService } from '../services/services'

interface AuthState {
  user: { nombre: string; rol: Rol; empleadoId: number | null } | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const Ctx = createContext<AuthState>(null!)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthState['user']>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const raw = localStorage.getItem('user')
    if (raw) setUser(JSON.parse(raw))
    setLoading(false)
  }, [])

  async function login(email: string, password: string) {
    const resp: LoginResponse = await authService.login(email, password)
    localStorage.setItem('token', resp.token)
    const u = { nombre: resp.nombre, rol: resp.rol, empleadoId: resp.empleadoId }
    localStorage.setItem('user', JSON.stringify(u))
    setUser(u)
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
    window.location.href = '/login'
  }

  return <Ctx.Provider value={{ user, loading, login, logout }}>{children}</Ctx.Provider>
}

export const useAuth = () => useContext(Ctx)
