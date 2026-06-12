import { Link, useLocation } from 'react-router-dom'

const labels: Record<string, string> = {
  dashboard: 'Dashboard',
  master: 'Master',
  items: 'Barang',
  customers: 'Konsumen',
  suppliers: 'Distributor',
  branches: 'Cabang',
  inventory: 'Stok & Harga',
  transactions: 'Transaksi',
  sales: 'Penjualan',
  purchases: 'Pembelian',
  complaints: 'Complain',
  billing: 'Billing',
  reports: 'Reports',
  settings: 'Settings',
  admin: 'Super Admin',
  receipt: 'Struk',
}

export function Breadcrumb() {
  const location = useLocation()
  const segments = location.pathname.split('/').filter(Boolean)
  if (segments.length === 0) return null

  return (
    <nav className="mb-5 flex flex-wrap items-center gap-2 text-sm text-slate-500">
      <Link className="font-medium text-indigo-600 hover:text-indigo-700" to="/dashboard">Dashboard</Link>
      {segments[0] !== 'dashboard' && segments.map((segment, index) => {
        const to = `/${segments.slice(0, index + 1).join('/')}`
        const isLast = index === segments.length - 1
        return (
          <span key={to} className="inline-flex items-center gap-2">
            <span>/</span>
            {isLast ? <span className="font-semibold text-slate-700">{labels[segment] ?? segment}</span> : <Link className="hover:text-indigo-600" to={to}>{labels[segment] ?? segment}</Link>}
          </span>
        )
      })}
    </nav>
  )
}
