import { LogOut } from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/auth-store'

const links = [
  { to: '/', label: 'Dashboard' },
  { to: '/master/items', label: 'Barang' },
  { to: '/master/customers', label: 'Konsumen' },
  { to: '/master/suppliers', label: 'Distributor' },
  { to: '/master/branches', label: 'Cabang Toko' },
  { to: '/inventory', label: 'Stok & Harga' },
  { to: '/transactions/sales', label: 'Penjualan' },
  { to: '/transactions/purchases', label: 'Pembelian' },
  { to: '/billing', label: 'Billing' },
  { to: '/reports', label: 'Reports' },
  { to: '/settings', label: 'Settings' },
]

export function MainLayout() {
  const navigate = useNavigate()
  const user = useAuthStore((state) => state.user)
  const logout = useAuthStore((state) => state.logout)

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <aside className="fixed inset-y-0 left-0 w-64 border-r border-slate-200 bg-white p-5">
        <h1 className="text-2xl font-bold text-indigo-600">XPARF</h1>
        <p className="mt-1 text-sm text-slate-500">ERP modernisasi</p>
        <nav className="mt-8 space-y-1">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `block rounded-xl px-4 py-3 text-sm font-medium ${isActive ? 'bg-indigo-50 text-indigo-700' : 'text-slate-600 hover:bg-slate-100'}`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="ml-64 min-h-screen">
        <header className="flex h-16 items-center justify-between border-b border-slate-200 bg-white px-8">
          <div>
            <p className="text-sm text-slate-500">Login sebagai</p>
            <p className="font-semibold">{user?.userName ?? '-'}</p>
          </div>
          <button
            className="inline-flex items-center gap-2 rounded-xl border border-slate-200 px-4 py-2 text-sm font-medium hover:bg-slate-100"
            onClick={() => {
              logout()
              navigate('/login')
            }}
          >
            <LogOut size={16} /> Logout
          </button>
        </header>
        <div className="p-8">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
