import { useQuery } from '@tanstack/react-query'
import { api, type DashboardSummary } from '../lib/api'
import { StatCard } from '../components/StatCard'

const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })

export function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => (await api.get<DashboardSummary>('/reports/dashboard')).data,
  })

  if (isLoading) return <p>Loading dashboard...</p>
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Dashboard</h2>
        <p className="text-slate-500">Ringkasan penjualan, pembelian, stok, dan coin.</p>
      </div>
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <StatCard title="Sales hari ini" value={money.format(data?.salesTotal ?? 0)} description={`${data?.salesCount ?? 0} transaksi`} />
        <StatCard title="Purchase hari ini" value={money.format(data?.purchaseTotal ?? 0)} description={`${data?.purchaseCount ?? 0} transaksi`} />
        <StatCard title="Coin balance" value={`${data?.coinBalance ?? 0}`} description="Saldo coin aktif" />
        <StatCard title="Low stock" value={`${data?.lowStockCount ?? 0}`} description="Item di bawah minimum" />
      </div>
    </div>
  )
}
