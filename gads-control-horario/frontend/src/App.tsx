import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import EmpleadosPage from './pages/Empleados'
import HorariosPage from './pages/Horarios'
import FichadasPage from './pages/Fichadas'
import NovedadesPage from './pages/Novedades'
import CierrePage from './pages/Cierre'
import { MisFichadas, MisNovedades } from './pages/Empleado'
import { ReactNode } from 'react'
import type { Rol } from './types'

function PrivateRoute({ children, roles }: { children: ReactNode; roles?: Rol[] }) {
  const { user, loading } = useAuth()
  if (loading) return null
  if (!user) return <Navigate to="/login" replace />
  if (roles && !roles.includes(user.rol)) return <Navigate to="/" replace />
  return <Layout>{children}</Layout>
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route path="/" element={<PrivateRoute><Dashboard /></PrivateRoute>} />

          <Route path="/empleados" element={
            <PrivateRoute roles={['Administrador']}><EmpleadosPage /></PrivateRoute>
          } />
          <Route path="/horarios" element={
            <PrivateRoute roles={['Administrador']}><HorariosPage /></PrivateRoute>
          } />
          <Route path="/fichadas" element={
            <PrivateRoute roles={['Administrador']}><FichadasPage /></PrivateRoute>
          } />
          <Route path="/novedades" element={
            <PrivateRoute roles={['Administrador']}><NovedadesPage /></PrivateRoute>
          } />
          <Route path="/cierre" element={
            <PrivateRoute roles={['Administrador', 'Contador']}><CierrePage /></PrivateRoute>
          } />

          <Route path="/mis-fichadas" element={
            <PrivateRoute roles={['Empleado']}><MisFichadas /></PrivateRoute>
          } />
          <Route path="/mis-novedades" element={
            <PrivateRoute roles={['Empleado']}><MisNovedades /></PrivateRoute>
          } />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
