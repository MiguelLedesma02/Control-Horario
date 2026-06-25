import { ReactNode } from 'react'

export function Card({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <div className={`relative bg-ink-900 border border-ink-700 rounded-xl ${className}`}>
      {children}
    </div>
  )
}

export function PageHeader({ title, subtitle, action }: {
  title: string; subtitle?: string; action?: ReactNode
}) {
  return (
    <div className="flex items-end justify-between mb-8 pb-6 border-b border-ink-700">
      <div>
        <h1 className="font-display text-3xl font-bold text-white">{title}</h1>
        {subtitle && <p className="text-ink-300 mt-1 text-sm">{subtitle}</p>}
      </div>
      {action}
    </div>
  )
}

export function Badge({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <span className={`inline-flex items-center px-2 py-0.5 text-xs font-semibold rounded border ${className}`}>
      {children}
    </span>
  )
}

export function Button({
  children, variant = 'primary', onClick, disabled, type = 'button', className = ''
}: {
  children: ReactNode
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  onClick?: () => void
  disabled?: boolean
  type?: 'button' | 'submit'
  className?: string
}) {
  const base = 'inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold transition-colors disabled:opacity-50 disabled:cursor-not-allowed'
  const styles = {
    primary: 'bg-accent text-ink-950 hover:bg-accent-muted',
    secondary: 'bg-ink-700 text-white hover:bg-ink-600',
    ghost: 'text-ink-300 hover:text-white hover:bg-ink-800',
    danger: 'bg-bad/15 text-bad border border-bad/30 hover:bg-bad/25'
  }
  return (
    <button type={type} disabled={disabled} onClick={onClick}
      className={`${base} ${styles[variant]} ${className}`}>
      {children}
    </button>
  )
}

export function Input(props: React.InputHTMLAttributes<HTMLInputElement> & { label?: string }) {
  const { label, className = '', ...rest } = props
  return (
    <label className="block">
      {label && <span className="block text-xs text-ink-300 uppercase tracking-wider mb-1.5">{label}</span>}
      <input
        {...rest}
        className={`w-full bg-ink-800 border border-ink-700 rounded-lg px-3 py-2 text-sm text-white placeholder-ink-300/50 focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/20 ${className}`}
      />
    </label>
  )
}

export function Select(props: React.SelectHTMLAttributes<HTMLSelectElement> & { label?: string }) {
  const { label, children, className = '', ...rest } = props
  return (
    <label className="block">
      {label && <span className="block text-xs text-ink-300 uppercase tracking-wider mb-1.5">{label}</span>}
      <select
        {...rest}
        className={`w-full bg-ink-800 border border-ink-700 rounded-lg px-3 py-2 text-sm text-white focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/20 ${className}`}
      >
        {children}
      </select>
    </label>
  )
}

export function Stat({ label, value, hint, accent = false }: {
  label: string; value: string | number; hint?: string; accent?: boolean
}) {
  return (
    <Card className="p-5">
      <div className="text-xs text-ink-300 uppercase tracking-wider">{label}</div>
      <div className={`font-display font-bold mt-2 ${accent ? 'text-accent text-4xl' : 'text-white text-3xl'}`}>
        {value}
      </div>
      {hint && <div className="text-xs text-ink-300 mt-2">{hint}</div>}
    </Card>
  )
}

export function EmptyState({ icon: Icon, title, hint }: {
  icon: any; title: string; hint?: string
}) {
  return (
    <div className="text-center py-16">
      <Icon className="w-12 h-12 text-ink-600 mx-auto mb-4" />
      <div className="text-ink-300">{title}</div>
      {hint && <div className="text-xs text-ink-300/60 mt-1">{hint}</div>}
    </div>
  )
}

export function Spinner() {
  return (
    <div className="flex items-center justify-center py-12">
      <div className="w-8 h-8 border-2 border-ink-700 border-t-accent rounded-full animate-spin" />
    </div>
  )
}

export function Modal({ open, onClose, title, children }: {
  open: boolean; onClose: () => void; title: string; children: ReactNode
}) {
  if (!open) return null
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-ink-950/80 backdrop-blur-sm" onClick={onClose}>
      <div className="bg-ink-900 border border-ink-700 rounded-xl max-w-lg w-full p-6" onClick={e => e.stopPropagation()}>
        <div className="flex items-center justify-between mb-5">
          <h3 className="font-display text-xl font-bold text-white">{title}</h3>
          <button onClick={onClose} className="text-ink-300 hover:text-white">×</button>
        </div>
        {children}
      </div>
    </div>
  )
}
