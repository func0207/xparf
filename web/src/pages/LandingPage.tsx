import { Link } from 'react-router-dom'

export function LandingPage() {
  return (
    <div className="min-h-screen bg-white text-slate-900">
      <header className="mx-auto flex max-w-6xl items-center justify-between px-6 py-6">
        <div className="text-2xl font-bold text-indigo-600">XPARF</div>
        <nav className="flex items-center gap-3">
          <Link className="rounded-xl px-4 py-2 font-medium text-slate-600 hover:bg-slate-100" to="/login">Login</Link>
          <Link className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700" to="/register">Register</Link>
        </nav>
      </header>
      <main className="mx-auto grid max-w-6xl gap-12 px-6 py-20 lg:grid-cols-2 lg:items-center">
        <section>
          <p className="mb-4 inline-block rounded-full bg-indigo-50 px-4 py-2 text-sm font-semibold text-indigo-700">Modern ERP for retail & wholesale</p>
          <h1 className="text-5xl font-bold tracking-tight">Kelola sales, purchase, stock, billing, report dalam satu dashboard.</h1>
          <p className="mt-6 text-lg leading-8 text-slate-600">Migrasi Great.ERP.Website ke stack baru: API modern, multi-company, multi-branch, role permission, QRIS topup, dan laporan realtime.</p>
          <div className="mt-8 flex gap-3">
            <Link className="rounded-xl bg-indigo-600 px-6 py-3 font-semibold text-white hover:bg-indigo-700" to="/register">Mulai sekarang</Link>
            <Link className="rounded-xl border border-slate-300 px-6 py-3 font-semibold hover:bg-slate-50" to="/login">Masuk</Link>
          </div>
        </section>
        <section className="rounded-3xl border border-slate-200 bg-slate-50 p-6 shadow-sm">
          <div className="grid gap-4 sm:grid-cols-2">
            {['Dashboard informatif', 'Server paging table', 'Role permission', 'QRIS coin topup', 'Stock ledger', 'Sales & purchase posting'].map((item) => <div key={item} className="rounded-2xl bg-white p-5 font-semibold shadow-sm">{item}</div>)}
          </div>
        </section>
      </main>
    </div>
  )
}
