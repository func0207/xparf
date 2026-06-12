import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts'
import { api, type DashboardSummary } from '../lib/api'
import { StatCard } from '../components/StatCard'

const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 })
const number = new Intl.NumberFormat('id-ID')

const CHART_COLORS = ['#6366f1', '#10b981', '#f59e0b', '#ef4444']

type ChartData = { name: string; value: number; fill: string }[]

export function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => (await api.get<DashboardSummary>('/reports/dashboard')).data,
  })

  if (isLoading) return <div className="flex min-h-[60vh] items-center justify-center"><p className="text-lg text-slate-500">Loading dashboard...</p></div>

  const chartData: ChartData = [
    { name: 'Sales', value: data?.salesTotal ?? 0, fill: CHART_COLORS[0] },
    { name: 'Purchase', value: data?.purchaseTotal ?? 0, fill: CHART_COLORS[1] },
    { name: 'Coin', value: data?.coinBalance ?? 0, fill: CHART_COLORS[2] },
  ]

  const transactionChart: ChartData = [
    { name: 'Sales', value: data?.salesCount ?? 0, fill: CHART_COLORS[0] },
    { name: 'Purchase', value: data?.purchaseCount ?? 0, fill: CHART_COLORS[1] },
  ]

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold">Dashboard</h2>
          <p className="text-slate-500">Ringkasan penjualan, pembelian, stok, dan coin.</p>
        </div>
        <Link className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-indigo-600 hover:bg-indigo-50" to="/transactions/sales">+ Transaksi baru</Link>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <StatCard title="Sales hari ini" value={money.format(data?.salesTotal ?? 0)} description={`${data?.salesCount ?? 0} transaksi`} />
        <StatCard title="Purchase hari ini" value={money.format(data?.purchaseTotal ?? 0)} description={`${data?.purchaseCount ?? 0} transaksi`} />
        <StatCard title="Coin balance" value={number.format(data?.coinBalance ?? 0)} description="Saldo coin aktif" />
        <StatCard title="Low stock" value={number.format(data?.lowStockCount ?? 0)} description="Item di bawah minimum" />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="text-lg font-bold">Nilai transaksi hari ini</h3>
          <p className="text-sm text-slate-500">Perbandingan sales vs purchase vs coin</p>
          <div className="mt-4 h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} tickFormatter={(v: number) => money.format(v)} />
                <Tooltip formatter={(value) => money.format(Number(value))} />
                <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                  {chartData.map((entry, index) => <Cell key={index} fill={entry.fill} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="text-lg font-bold">Jumlah transaksi</h3>
          <p className="text-sm text-slate-500">Total transaksi sales vs purchase</p>
          <div className="mt-4 h-64">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={transactionChart} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90} label={({ name, value }) => `${name ?? ''}: ${value}`}>
                  {transactionChart.map((entry, index) => <Cell key={index} fill={entry.fill} />)}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-lg font-bold">Quick test flow</h3>
        <p className="text-sm text-slate-500">Link cepat untuk testing alur ERP</p>
        <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {[
            { label: 'Master Barang', to: '/master/items' },
            { label: 'Master Konsumen', to: '/master/customers' },
            { label: 'Stok & Harga', to: '/inventory' },
            { label: 'Penjualan', to: '/transactions/sales' },
            { label: 'Pembelian', to: '/transactions/purchases' },
            { label: 'Billing Coin', to: '/billing' },
            { label: 'Reports', to: '/reports' },
            { label: 'Settings', to: '/settings' },
          ].map((link) => (
            <Link key={link.to} to={link.to} className="rounded-xl border border-slate-200 px-4 py-3 text-center font-medium hover:bg-indigo-50 hover:border-indigo-200 hover:text-indigo-700 transition">
              {link.label}
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
