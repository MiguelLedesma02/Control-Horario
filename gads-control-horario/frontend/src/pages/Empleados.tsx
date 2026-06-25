import { useEffect, useRef, useState } from 'react'
import { Plus, Users, Pencil, Power, Trash2, Upload, Download, AlertTriangle } from 'lucide-react'
import { PageHeader, Card, Button, Badge, Spinner, Input, Select, Modal, EmptyState } from '../components/UI'
import { empleadoService, horarioService } from '../services/services'
import { fmtFecha } from '../utils/format'
import type { Empleado, Horario } from '../types'
import * as XLSX from 'xlsx'

const formVacio = (horarios: Horario[]) => ({
  legajo: '', nombre: '', apellido: '', dni: '', cuil: '',
  fechaIngreso: new Date().toISOString().split('T')[0],
  categoriaLaboral: '', convenioColectivo: '', tipoJornada: 'Completa',
  horarioId: horarios[0]?.id || 0, email: '', telefono: ''
})

export default function EmpleadosPage() {
  const [lista, setLista] = useState<Empleado[]>([])
  const [horarios, setHorarios] = useState<Horario[]>([])
  const [loading, setLoading] = useState(true)
  const [modalAbierto, setModalAbierto] = useState(false)
  const [modalImportar, setModalImportar] = useState(false)
  const [editando, setEditando] = useState<Empleado | null>(null)
  const [form, setForm] = useState(formVacio([]))

  // Importación Excel
  const inputFileRef = useRef<HTMLInputElement>(null)
  const [importPreview, setImportPreview] = useState<any[]>([])
  const [importError, setImportError] = useState('')
  const [importResult, setImportResult] = useState<{ importados: number; omitidos: string[] } | null>(null)
  const [importando, setImportando] = useState(false)

  async function cargar() {
    setLoading(true)
    const [e, h] = await Promise.all([empleadoService.getAll(), horarioService.getAll()])
    setLista(e); setHorarios(h)
    setLoading(false)
  }
  useEffect(() => { cargar() }, [])

  function abrirNuevo() {
    setEditando(null)
    setForm(formVacio(horarios))
    setModalAbierto(true)
  }

  function abrirEditar(e: Empleado) {
    setEditando(e)
    setForm({
      legajo: e.legajo, nombre: e.nombre, apellido: e.apellido,
      dni: e.dni, cuil: e.cuil,
      fechaIngreso: e.fechaIngreso.split('T')[0],
      categoriaLaboral: e.categoriaLaboral,
      convenioColectivo: e.convenioColectivo || '',
      tipoJornada: e.tipoJornada, horarioId: e.horarioId,
      email: e.email || '', telefono: e.telefono || ''
    })
    setModalAbierto(true)
  }

  async function guardar(ev: React.FormEvent) {
    ev.preventDefault()
    const datos: any = { ...form, horarioId: Number(form.horarioId) }
    try {
      if (editando) await empleadoService.actualizar(editando.id, datos)
      else await empleadoService.crear(datos)
      setModalAbierto(false)
      cargar()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Error al guardar')
    }
  }

  async function desactivar(e: Empleado) {
    if (!confirm(`¿Desactivar a ${e.apellido}, ${e.nombre}?`)) return
    await empleadoService.desactivar(e.id)
    cargar()
  }

  async function eliminar(e: Empleado) {
    if (!confirm(`⚠️ ¿Eliminar PERMANENTEMENTE a ${e.apellido}, ${e.nombre}?\n\nEsta acción no se puede deshacer y borrará todas sus fichadas y novedades.`)) return
    try {
      await empleadoService.eliminar(e.id)
      cargar()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al eliminar el empleado')
    }
  }

  // ── Importación Excel ──────────────────────────────────────────────
  function descargarPlantilla() {
    const ws = XLSX.utils.aoa_to_sheet([
      ['legajo','nombre','apellido','dni','cuil','fechaIngreso','categoriaLaboral','convenioColectivo','tipoJornada','horarioId','email','telefono'],
      ['001','Juan','Pérez','12345678','20-12345678-9','2024-01-15','Operario','CCT 130/75','Completa', horarios[0]?.id || 1,'juan@empresa.com','1122334455'],
    ])
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'Empleados')
    XLSX.writeFile(wb, 'plantilla-empleados.xlsx')
  }

  const JORNADAS_VALIDAS = ['Completa', 'Parcial', 'Flexible', 'Rotativa']

  function normalizarFecha(val: any): string {
    if (typeof val === 'number') {
      // número serial de Excel → fecha
      const fecha = new Date(Math.round((val - 25569) * 86400 * 1000))
      return fecha.toISOString().split('T')[0]
    }
    const s = String(val).trim()
    // admite dd/mm/yyyy o dd-mm-yyyy → convierte a yyyy-mm-dd
    const dmY = s.match(/^(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})$/)
    if (dmY) return `${dmY[3]}-${dmY[2].padStart(2,'0')}-${dmY[1].padStart(2,'0')}`
    return s // ya en formato yyyy-mm-dd u otro
  }

  function validarFilas(rows: any[]): string[] {
    const errores: string[] = []
    rows.forEach((r, i) => {
      const fila = `Fila ${i + 2}`
      if (!r.legajo) errores.push(`${fila}: falta "legajo"`)
      if (!r.nombre) errores.push(`${fila}: falta "nombre"`)
      if (!r.apellido) errores.push(`${fila}: falta "apellido"`)
      if (!r.dni) errores.push(`${fila}: falta "dni"`)
      if (!r.cuil) errores.push(`${fila}: falta "cuil"`)
      if (!r.categoriaLaboral) errores.push(`${fila}: falta "categoriaLaboral"`)
      if (!r.horarioId || isNaN(Number(r.horarioId))) errores.push(`${fila}: "horarioId" debe ser un número`)
      const jornadaRaw = String(r.tipoJornada || '').trim()
      const jornada = jornadaRaw.charAt(0).toUpperCase() + jornadaRaw.slice(1).toLowerCase()
      if (!JORNADAS_VALIDAS.includes(jornada)) errores.push(`${fila}: "tipoJornada" inválido ("${jornadaRaw}"). Debe ser: ${JORNADAS_VALIDAS.join(', ')}`)
      const fechaNorm = normalizarFecha(r.fechaIngreso)
      if (!fechaNorm || isNaN(Date.parse(fechaNorm))) errores.push(`${fila}: "fechaIngreso" inválida ("${r.fechaIngreso}"). Usá formato YYYY-MM-DD`)
    })
    return errores
  }

  function leerExcel(e: React.ChangeEvent<HTMLInputElement>) {
    setImportError(''); setImportPreview([]); setImportResult(null)
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = (ev) => {
      try {
        const wb = XLSX.read(ev.target?.result, { type: 'binary' })
        const ws = wb.Sheets[wb.SheetNames[0]]
        const rows: any[] = XLSX.utils.sheet_to_json(ws, { defval: '' })
        if (rows.length === 0) { setImportError('El archivo no contiene datos.'); return }

        // Normalizar campos antes de validar
        const parsed = rows.map(r => {
          const jornadaRaw = String(r.tipoJornada || 'Completa').trim()
          const jornada = jornadaRaw.charAt(0).toUpperCase() + jornadaRaw.slice(1).toLowerCase()
          return {
            ...r,
            fechaIngreso: normalizarFecha(r.fechaIngreso),
            horarioId: Number(r.horarioId),
            tipoJornada: jornada,
          }
        })

        const errores = validarFilas(rows)
        if (errores.length > 0) {
          setImportError('El archivo tiene errores:\n• ' + errores.join('\n• '))
          return
        }

        setImportPreview(parsed)
      } catch {
        setImportError('No se pudo leer el archivo. Asegurate de que sea un .xlsx válido.')
      }
    }
    reader.readAsBinaryString(file)
  }

  async function confirmarImportar() {
    setImportando(true)
    setImportError('')
    try {
      const result = await empleadoService.importar(importPreview)
      setImportResult(result)
      if (result.importados > 0) cargar()
    } catch (err: any) {
      // Intentar extraer el mensaje de error más descriptivo posible
      const data = err.response?.data
      let msg = 'Error al importar. Revisá la consola para más detalles.'
      if (typeof data === 'string' && data.length < 300) msg = data
      else if (data?.message) msg = data.message
      else if (data?.errors) {
        // errores de validación de ASP.NET (400 con ProblemDetails)
        const detalles = Object.entries(data.errors as Record<string, string[]>)
          .flatMap(([campo, msgs]) => msgs.map((m: string) => `${campo}: ${m}`))
        msg = 'Errores de validación:\n• ' + detalles.join('\n• ')
      } else if (err.message) msg = err.message
      setImportError(msg)
    } finally {
      setImportando(false)
    }
  }

  function cerrarImportar() {
    setModalImportar(false)
    setImportPreview([]); setImportError(''); setImportResult(null)
    if (inputFileRef.current) inputFileRef.current.value = ''
  }

  if (loading) return <Spinner />

  return (
    <>
      <PageHeader title="Empleados" subtitle={`${lista.length} registrados en total`}
        action={
          <div className="flex gap-2">
            <Button variant="ghost" onClick={() => setModalImportar(true)}>
              <Upload className="w-4 h-4" /> Importar Excel
            </Button>
            <Button onClick={abrirNuevo}><Plus className="w-4 h-4" /> Nuevo empleado</Button>
          </div>
        } />

      {lista.length === 0 ? (
        <EmptyState icon={Users} title="Aún no cargaste empleados"
          hint="Empezá agregando el primero o importá desde Excel" />
      ) : (
        <Card className="overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-ink-800 text-left">
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Legajo</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Empleado</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Categoría</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Horario</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Ingreso</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider">Estado</th>
                <th className="text-xs uppercase text-ink-300 px-5 py-3 font-semibold tracking-wider"></th>
              </tr>
            </thead>
            <tbody>
              {lista.map((e, i) => (
                <tr key={e.id} className={`border-t border-ink-700 hover:bg-ink-800/50 ${i % 2 ? 'bg-ink-900' : ''}`}>
                  <td className="px-5 py-3 font-mono text-sm text-accent">{e.legajo}</td>
                  <td className="px-5 py-3">
                    <div className="text-sm text-white">{e.apellido}, {e.nombre}</div>
                    <div className="text-xs text-ink-300">DNI {e.dni}</div>
                  </td>
                  <td className="px-5 py-3 text-sm">{e.categoriaLaboral}</td>
                  <td className="px-5 py-3 text-sm text-ink-300">{e.horarioNombre}</td>
                  <td className="px-5 py-3 text-sm text-ink-300">{fmtFecha(e.fechaIngreso)}</td>
                  <td className="px-5 py-3">
                    <Badge className={
                      e.estado === 'Activo' ? 'bg-good/15 text-good border-good/30' :
                      e.estado === 'Suspendido' ? 'bg-warn/15 text-warn border-warn/30' :
                      'bg-bad/15 text-bad border-bad/30'
                    }>{e.estado}</Badge>
                  </td>
                  <td className="px-5 py-3 text-right flex items-center justify-end gap-0.5">
                    <button onClick={() => abrirEditar(e)} title="Editar"
                      className="p-2 hover:bg-ink-700 rounded text-ink-300 hover:text-white">
                      <Pencil className="w-4 h-4" />
                    </button>
                    {e.estado === 'Activo' && (
                      <button onClick={() => desactivar(e)} title="Desactivar"
                        className="p-2 hover:bg-ink-700 rounded text-ink-300 hover:text-warn">
                        <Power className="w-4 h-4" />
                      </button>
                    )}
                    <button onClick={() => eliminar(e)} title="Eliminar permanentemente"
                      className="p-2 hover:bg-ink-700 rounded text-ink-300 hover:text-bad">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      {/* Modal Nuevo / Editar */}
      <Modal open={modalAbierto} onClose={() => setModalAbierto(false)}
        title={editando ? 'Editar empleado' : 'Nuevo empleado'}>
        <form onSubmit={guardar} className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <Input label="Legajo" value={form.legajo} required disabled={!!editando}
              onChange={e => setForm({ ...form, legajo: e.target.value })} />
            <Input label="DNI" value={form.dni} required
              onChange={e => setForm({ ...form, dni: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Nombre" value={form.nombre} required
              onChange={e => setForm({ ...form, nombre: e.target.value })} />
            <Input label="Apellido" value={form.apellido} required
              onChange={e => setForm({ ...form, apellido: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="CUIL" value={form.cuil} required
              onChange={e => setForm({ ...form, cuil: e.target.value })} />
            <Input label="Fecha ingreso" type="date" value={form.fechaIngreso} required
              onChange={e => setForm({ ...form, fechaIngreso: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Categoría laboral" value={form.categoriaLaboral} required
              onChange={e => setForm({ ...form, categoriaLaboral: e.target.value })} />
            <Input label="Convenio" value={form.convenioColectivo}
              onChange={e => setForm({ ...form, convenioColectivo: e.target.value })} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Select label="Tipo de jornada" value={form.tipoJornada}
              onChange={e => setForm({ ...form, tipoJornada: e.target.value })}>
              <option>Completa</option><option>Parcial</option>
              <option>Flexible</option><option>Rotativa</option>
            </Select>
            <Select label="Horario" value={form.horarioId}
              onChange={e => setForm({ ...form, horarioId: Number(e.target.value) })}>
              {horarios.map(h => <option key={h.id} value={h.id}>{h.nombre}</option>)}
            </Select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Email" type="email" value={form.email}
              onChange={e => setForm({ ...form, email: e.target.value })} />
            <Input label="Teléfono" value={form.telefono}
              onChange={e => setForm({ ...form, telefono: e.target.value })} />
          </div>
          <div className="flex justify-end gap-2 pt-3">
            <Button variant="ghost" onClick={() => setModalAbierto(false)}>Cancelar</Button>
            <Button type="submit">{editando ? 'Guardar cambios' : 'Crear empleado'}</Button>
          </div>
        </form>
      </Modal>

      {/* Modal Importar Excel */}
      <Modal open={modalImportar} onClose={cerrarImportar} title="Importar empleados desde Excel">
        <div className="space-y-4">
          {/* Paso 1: Plantilla */}
          <div className="bg-ink-800 rounded-lg p-4 flex items-start gap-3">
            <Download className="w-5 h-5 text-accent mt-0.5 shrink-0" />
            <div>
              <p className="text-sm text-white font-medium mb-1">Paso 1 — Descargá la plantilla</p>
              <p className="text-xs text-ink-300 mb-2">Completá el Excel con los datos de tus empleados. La columna <code className="bg-ink-700 px-1 rounded">horarioId</code> debe contener el ID de un horario existente.</p>
              <div className="flex flex-wrap gap-2 text-xs text-ink-400 mb-3">
                {horarios.map(h => (
                  <span key={h.id} className="bg-ink-700 px-2 py-0.5 rounded font-mono">
                    {h.id} = {h.nombre}
                  </span>
                ))}
              </div>
              <Button variant="ghost" onClick={descargarPlantilla}>
                <Download className="w-4 h-4" /> Descargar plantilla.xlsx
              </Button>
            </div>
          </div>

          {/* Paso 2: Subir archivo */}
          <div>
            <p className="text-sm text-white font-medium mb-2">Paso 2 — Seleccioná el archivo</p>
            <label className="flex items-center justify-center gap-2 border-2 border-dashed border-ink-600 hover:border-accent rounded-lg p-6 cursor-pointer transition-colors">
              <Upload className="w-5 h-5 text-ink-300" />
              <span className="text-sm text-ink-300">Hacé clic o arrastrá un archivo .xlsx</span>
              <input ref={inputFileRef} type="file" accept=".xlsx,.xls" className="hidden" onChange={leerExcel} />
            </label>
          </div>

          {importError && (
            <div className="flex gap-2 items-start bg-bad/10 border border-bad/30 rounded-lg p-3">
              <AlertTriangle className="w-4 h-4 text-bad mt-0.5 shrink-0" />
              <div className="text-sm text-bad">
                {importError.split('\n').map((line, i) => (
                  <p key={i} className={i > 0 ? 'mt-0.5' : ''}>{line}</p>
                ))}
              </div>
            </div>
          )}

          {/* Vista previa */}
          {importPreview.length > 0 && !importResult && (
            <div>
              <p className="text-sm text-white font-medium mb-2">
                Vista previa — {importPreview.length} empleado{importPreview.length !== 1 ? 's' : ''} encontrado{importPreview.length !== 1 ? 's' : ''}
              </p>
              <div className="overflow-x-auto rounded border border-ink-700 max-h-52">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="bg-ink-800">
                      {['Legajo','Nombre','Apellido','DNI','Categoría','Horario ID'].map(c => (
                        <th key={c} className="text-ink-300 px-3 py-2 text-left font-semibold">{c}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {importPreview.map((r, i) => (
                      <tr key={i} className={`border-t border-ink-700 ${i % 2 ? 'bg-ink-900' : ''}`}>
                        <td className="px-3 py-1.5 font-mono text-accent">{r.legajo}</td>
                        <td className="px-3 py-1.5">{r.nombre}</td>
                        <td className="px-3 py-1.5">{r.apellido}</td>
                        <td className="px-3 py-1.5">{r.dni}</td>
                        <td className="px-3 py-1.5">{r.categoriaLaboral}</td>
                        <td className="px-3 py-1.5">{r.horarioId}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Resultado */}
          {importResult && (
            <div className="space-y-2">
              <div className="bg-good/10 border border-good/30 rounded-lg p-3">
                <p className="text-sm text-good font-medium">
                  ✓ {importResult.importados} empleado{importResult.importados !== 1 ? 's' : ''} importado{importResult.importados !== 1 ? 's' : ''} correctamente
                </p>
              </div>
              {importResult.omitidos.length > 0 && (
                <div className="bg-warn/10 border border-warn/30 rounded-lg p-3">
                  <p className="text-sm text-warn font-medium mb-1">
                    {importResult.omitidos.length} omitido{importResult.omitidos.length !== 1 ? 's' : ''}:
                  </p>
                  <ul className="text-xs text-warn space-y-0.5">
                    {importResult.omitidos.map((o, i) => <li key={i}>• {o}</li>)}
                  </ul>
                </div>
              )}
            </div>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="ghost" onClick={cerrarImportar}>Cerrar</Button>
            {importPreview.length > 0 && !importResult && (
              <Button onClick={confirmarImportar} disabled={importando}>
                {importando ? 'Importando...' : `Importar ${importPreview.length} empleado${importPreview.length !== 1 ? 's' : ''}`}
              </Button>
            )}
          </div>
        </div>
      </Modal>
    </>
  )
}
