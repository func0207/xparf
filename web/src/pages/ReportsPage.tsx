import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { api } from '../lib/api'
import { StatCard } from '../components/StatCard'

type ReportKind = 'sales' | 'purchases' | 'stock'
type MoneyReport = { from: string; to: string; count: number; total: number; paidAmount: number; outstandingAmount: number; rows: MoneyRow[] }
type MoneyRow = { id: number; saleNumber?: string; purchaseNumber?: string; saleDate?: string; purchaseDate?: string; branchId: number; total: number; paidAmount: number; outstandingAmount: number; paymentStatus: number }
type StockRow = { branchId: number; branchName: string; itemId: number; sku: string; itemName: string; quantityOnHand: number; minimumStock: number; averageCost: number; sellingPrice: number; isLowStock: boolean }

const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })
const number = new Intl.NumberFormat('id-ID')

export function ReportsPage() {
  const today = new Date().toISOString().slice(0, 10)
  const [kind, setKind] = useState<ReportKind>('sales')
  const [from, setFrom] = useState(today)
  const [to, setTo] = useState(today)
  const [branchId, setBranchId] = useState('')

  const moneyReportQuery = useQuery({
    queryKey: ['reports', kind, from, to],
    enabled: kind !== 'stock',
    queryFn: async () => (await api.get<MoneyReport>(`/reports/${kind}`, { params: { from, to } })).data,
  })

  const stockReportQuery = useQuery({
    queryKey: ['reports', 'stock', branchId],
    enabled: kind === 'stock',
    queryFn: async () => (await api.get<StockRow[]>('/reports/stock', { params: branchId ? { branchId } : {} })).data,
  })

  const moneyData = moneyReportQuery.data
  const stockRows = stockReportQuery.data ?? []

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Reports</h2>
        <p className="text-slate-500">Preview laporan sales, purchases, dan stock dari API.</p>
      </div>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <div className="grid gap-4 md:grid-cols-4">
          <label className="block">
            <span className="text-sm font-medium text-slate-700">Jenis report</span>
            <select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={kind} onChange={(e) => setKind(e.target.value as ReportKind)}>
              <option value="sales">Sales</option>
              <option value="purchases">Purchases</option>
              <option value="stock">Stock</option>
            </select>
          </label>
          {kind === 'stock' ? (
            <label className="block">
              <span className="text-sm font-medium text-slate-700">Branch ID optional</span>
              <input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={branchId} onChange={(e) => setBranchId(e.target.value)} placeholder="Semua cabang" />
            </label>
          ) : (
            <>
              <label className="block"><span className="text-sm font-medium text-slate-700">From</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
              <label className="block"><span className="text-sm font-medium text-slate-700">To</span><input className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
            </>
          )}
        </div>
      </section>

      {kind !== 'stock' && (
        <>
          <div className="grid gap-4 md:grid-cols-4">
            <StatCard title="Total" value={money.format(moneyData?.total ?? 0)} description={`${moneyData?.count ?? 0} transaksi`} />
            <StatCard title="Paid" value={money.format(moneyData?.paidAmount ?? 0)} />
            <StatCard title="Outstanding" value={money.format(moneyData?.outstandingAmount ?? 0)} />
            <StatCard title="Periode" value={from} description={`s/d ${to}`} />
          </div>
          <MoneyTable rows={moneyData?.rows ?? []} isLoading={moneyReportQuery.isLoading} />
        </>
      )}

      {kind === 'stock' && (
        <>
          <div className="grid gap-4 md:grid-cols-3">
            <StatCard title="Total item" value={number.format(stockRows.length)} />
            <StatCard title="Low stock" value={number.format(stockRows.filter((row) => row.isLowStock).length)} />
            <StatCard title="Total qty" value={number.format(stockRows.reduce((sum, row) => sum + row.quantityOnHand, 0))} />
          </div>
          <StockTable rows={stockRows} isLoading={stockReportQuery.isLoading} />
        </>
      )}
    </div>
  )
}

function MoneyTable({ rows, isLoading }: { rows: MoneyRow[]; isLoading: boolean }) {
  return <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Nomor</th><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-left">Branch</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-right">Paid</th><th className="px-5 py-3 text-right">Outstanding</th></tr></thead><tbody className="divide-y divide-slate-100">{isLoading && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={6}>Loading report...</td></tr>}{rows.map((row) => <tr key={row.id} className="hover:bg-slate-50"><td className="px-5 py-3">{row.saleNumber ?? row.purchaseNumber}</td><td className="px-5 py-3">{new Date(row.saleDate ?? row.purchaseDate ?? '').toLocaleString('id-ID')}</td><td className="px-5 py-3">#{row.branchId}</td><td className="px-5 py-3 text-right">{money.format(row.total)}</td><td className="px-5 py-3 text-right text-emerald-600">{money.format(row.paidAmount)}</td><td className="px-5 py-3 text-right text-red-600">{money.format(row.outstandingAmount)}</td></tr>)}{!isLoading && rows.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={6}>Data kosong</td></tr>}</tbody></table></div></section>
}

function StockTable({ rows, isLoading }: { rows: StockRow[]; isLoading: boolean }) {
  return <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Cabang</th><th className="px-5 py-3 text-left">SKU</th><th className="px-5 py-3 text-left">Barang</th><th className="px-5 py-3 text-right">Qty</th><th className="px-5 py-3 text-right">Min</th><th className="px-5 py-3 text-right">Harga</th><th className="px-5 py-3 text-left">Status</th></tr></thead><tbody className="divide-y divide-slate-100">{isLoading && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={7}>Loading stock...</td></tr>}{rows.map((row) => <tr key={`${row.branchId}-${row.itemId}`} className="hover:bg-slate-50"><td className="px-5 py-3">{row.branchName}</td><td className="px-5 py-3">{row.sku}</td><td className="px-5 py-3">{row.itemName}</td><td className="px-5 py-3 text-right">{number.format(row.quantityOnHand)}</td><td className="px-5 py-3 text-right">{number.format(row.minimumStock)}</td><td className="px-5 py-3 text-right">{money.format(row.sellingPrice)}</td><td className="px-5 py-3">{row.isLowStock ? <span className="rounded-full bg-red-50 px-2 py-1 text-xs font-semibold text-red-700">Low</span> : <span className="rounded-full bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">OK</span>}</td></tr>)}{!isLoading && rows.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={7}>Data kosong</td></tr>}</tbody></table></div></section>
}
