import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import type { PageResponse } from '../components/DataTable'
import { api } from '../lib/api'

type Branch = { id: number; code: string; name: string }
type Item = { id: number; sku: string; name: string; baseUnit: string }
type BranchItem = { id: number; branchId: number; itemId: number; itemSku: string; itemName: string; quantityOnHand: number; minimumStock: number; averageCost: number; lastCost: number; sellingPrice: number; wholesalePrice: number; isAvailable: boolean }
type StockLedger = { id: number; branchId: number; itemId: number; movementType: number; referenceType: string; referenceId?: number; quantityIn: number; quantityOut: number; balanceAfter: number; unitCost: number; note?: string; createdAt: string }
type StockForm = { id?: number; branchId: number; itemId: number; minimumStock: number; averageCost: number; lastCost: number; sellingPrice: number; wholesalePrice: number; isAvailable: boolean }
type AdjustmentForm = { branchId: number; itemId: number; quantityChange: number; unitCost: number; note: string }

const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })
const number = new Intl.NumberFormat('id-ID')
const blankStock: StockForm = { branchId: 0, itemId: 0, minimumStock: 0, averageCost: 0, lastCost: 0, sellingPrice: 0, wholesalePrice: 0, isAvailable: true }
const blankAdjustment: AdjustmentForm = { branchId: 0, itemId: 0, quantityChange: 0, unitCost: 0, note: '' }

export function InventoryPage() {
  const qc = useQueryClient()
  const [branchFilter, setBranchFilter] = useState(0)
  const [itemFilter, setItemFilter] = useState(0)
  const [stockForm, setStockForm] = useState<StockForm>(blankStock)
  const [adjustment, setAdjustment] = useState<AdjustmentForm>(blankAdjustment)

  const branches = useQuery({ queryKey: ['branches', 'inventory'], queryFn: async () => (await api.get<PageResponse<Branch>>('/branches', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const items = useQuery({ queryKey: ['items', 'inventory'], queryFn: async () => (await api.get<PageResponse<Item>>('/items', { params: { page: 1, pageSize: 200, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const branchItems = useQuery({ queryKey: ['branch-items'], queryFn: async () => (await api.get<BranchItem[]>('/branch-items')).data })
  const ledgers = useQuery({ queryKey: ['stock-ledgers', branchFilter, itemFilter], queryFn: async () => (await api.get<StockLedger[]>('/stock-ledgers', { params: { branchId: branchFilter || undefined, itemId: itemFilter || undefined } })).data })

  const filteredBranchItems = useMemo(() => (branchItems.data ?? []).filter((row) => (!branchFilter || row.branchId === branchFilter) && (!itemFilter || row.itemId === itemFilter)), [branchItems.data, branchFilter, itemFilter])

  const saveStock = useMutation({
    mutationFn: async () => {
      const payload = { minimumStock: stockForm.minimumStock, averageCost: stockForm.averageCost, lastCost: stockForm.lastCost, sellingPrice: stockForm.sellingPrice, wholesalePrice: stockForm.wholesalePrice, isAvailable: stockForm.isAvailable }
      if (stockForm.id) return (await api.put<BranchItem>(`/branch-items/${stockForm.id}`, payload)).data
      return (await api.post<BranchItem>('/branch-items', { ...payload, branchId: stockForm.branchId, itemId: stockForm.itemId })).data
    },
    onSuccess: () => { setStockForm(blankStock); qc.invalidateQueries({ queryKey: ['branch-items'] }) },
  })
  const deleteStock = useMutation({ mutationFn: async (id: number) => api.delete(`/branch-items/${id}`), onSuccess: () => qc.invalidateQueries({ queryKey: ['branch-items'] }) })
  const createAdjustment = useMutation({
    mutationFn: async () => (await api.post<StockLedger>('/stock-adjustments', { branchId: adjustment.branchId, itemId: adjustment.itemId, quantityChange: adjustment.quantityChange, unitCost: adjustment.unitCost, note: adjustment.note })).data,
    onSuccess: () => { setAdjustment(blankAdjustment); qc.invalidateQueries({ queryKey: ['stock-ledgers'] }); qc.invalidateQueries({ queryKey: ['branch-items'] }) },
  })

  function edit(row: BranchItem) {
    setStockForm({ id: row.id, branchId: row.branchId, itemId: row.itemId, minimumStock: row.minimumStock, averageCost: row.averageCost, lastCost: row.lastCost, sellingPrice: row.sellingPrice, wholesalePrice: row.wholesalePrice, isAvailable: row.isAvailable })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Stok & Harga Cabang</h2>
        <p className="text-slate-500">Kelola barang per cabang, harga jual/grosir, minimum stock, dan stock adjustment.</p>
      </div>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-xl font-bold">Filter</h3>
        <div className="mt-5 grid gap-4 md:grid-cols-3">
          <Select label="Cabang" value={branchFilter} onChange={(v) => setBranchFilter(Number(v))} options={[{ value: 0, label: 'Semua cabang' }, ...(branches.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))]} />
          <Select label="Barang" value={itemFilter} onChange={(v) => setItemFilter(Number(v))} options={[{ value: 0, label: 'Semua barang' }, ...(items.data ?? []).map((i) => ({ value: i.id, label: `${i.sku} - ${i.name}` }))]} />
        </div>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-xl font-bold">{stockForm.id ? 'Edit stok/harga cabang' : 'Tambah stok/harga cabang'}</h3>
        <div className="mt-5 grid gap-4 md:grid-cols-4">
          <Select label="Cabang" value={stockForm.branchId} onChange={(v) => setStockForm({ ...stockForm, branchId: Number(v) })} options={[{ value: 0, label: 'Pilih cabang' }, ...(branches.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))]} />
          <Select label="Barang" value={stockForm.itemId} onChange={(v) => setStockForm({ ...stockForm, itemId: Number(v) })} options={[{ value: 0, label: 'Pilih barang' }, ...(items.data ?? []).map((i) => ({ value: i.id, label: `${i.sku} - ${i.name}` }))]} />
          <Input label="Minimum Stock" type="number" value={String(stockForm.minimumStock)} onChange={(v) => setStockForm({ ...stockForm, minimumStock: Number(v) })} />
          <Input label="Average Cost" type="number" value={String(stockForm.averageCost)} onChange={(v) => setStockForm({ ...stockForm, averageCost: Number(v) })} />
          <Input label="Last Cost" type="number" value={String(stockForm.lastCost)} onChange={(v) => setStockForm({ ...stockForm, lastCost: Number(v) })} />
          <Input label="Selling Price" type="number" value={String(stockForm.sellingPrice)} onChange={(v) => setStockForm({ ...stockForm, sellingPrice: Number(v) })} />
          <Input label="Wholesale Price" type="number" value={String(stockForm.wholesalePrice)} onChange={(v) => setStockForm({ ...stockForm, wholesalePrice: Number(v) })} />
          <label className="mt-7 inline-flex items-center gap-2 text-sm font-semibold"><input type="checkbox" checked={stockForm.isAvailable} onChange={(e) => setStockForm({ ...stockForm, isAvailable: e.target.checked })} />Available</label>
        </div>
        <div className="mt-5 flex gap-3"><button disabled={!stockForm.branchId || !stockForm.itemId || saveStock.isPending} className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => saveStock.mutate()}>{stockForm.id ? 'Update' : 'Simpan'}</button><button className="rounded-xl border border-slate-200 px-4 py-2 font-semibold" onClick={() => setStockForm(blankStock)}>Reset</button></div>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="border-b border-slate-200 p-5"><h3 className="text-xl font-bold">Daftar stok/harga</h3></div><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Barang</th><th className="px-5 py-3 text-right">Qty</th><th className="px-5 py-3 text-right">Min</th><th className="px-5 py-3 text-right">Last Cost</th><th className="px-5 py-3 text-right">Jual</th><th className="px-5 py-3 text-right">Grosir</th><th className="px-5 py-3 text-left">Status</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{filteredBranchItems.map((row) => <tr key={row.id}><td className="px-5 py-3"><p className="font-semibold">{row.itemSku}</p><p className="text-slate-500">{row.itemName}</p></td><td className="px-5 py-3 text-right">{number.format(row.quantityOnHand)}</td><td className="px-5 py-3 text-right">{number.format(row.minimumStock)}</td><td className="px-5 py-3 text-right">{money.format(row.lastCost)}</td><td className="px-5 py-3 text-right">{money.format(row.sellingPrice)}</td><td className="px-5 py-3 text-right">{money.format(row.wholesalePrice)}</td><td className="px-5 py-3">{row.isAvailable ? 'Available' : 'Nonaktif'}</td><td className="px-5 py-3"><div className="flex gap-2"><button className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-semibold" onClick={() => edit(row)}>Edit</button><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600" onClick={() => confirm('Hapus stok/harga cabang ini?') && deleteStock.mutate(row.id)}>Hapus</button></div></td></tr>)}{filteredBranchItems.length === 0 && <tr><td colSpan={8} className="px-5 py-8 text-center text-slate-500">Belum ada data</td></tr>}</tbody></table></div></section>

      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-xl font-bold">Stock adjustment</h3>
        <div className="mt-5 grid gap-4 md:grid-cols-5">
          <Select label="Cabang" value={adjustment.branchId} onChange={(v) => setAdjustment({ ...adjustment, branchId: Number(v) })} options={[{ value: 0, label: 'Pilih cabang' }, ...(branches.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))]} />
          <Select label="Barang" value={adjustment.itemId} onChange={(v) => setAdjustment({ ...adjustment, itemId: Number(v) })} options={[{ value: 0, label: 'Pilih barang' }, ...(items.data ?? []).map((i) => ({ value: i.id, label: `${i.sku} - ${i.name}` }))]} />
          <Input label="Qty Change" type="number" value={String(adjustment.quantityChange)} onChange={(v) => setAdjustment({ ...adjustment, quantityChange: Number(v) })} />
          <Input label="Unit Cost" type="number" value={String(adjustment.unitCost)} onChange={(v) => setAdjustment({ ...adjustment, unitCost: Number(v) })} />
          <Input label="Note" value={adjustment.note} onChange={(v) => setAdjustment({ ...adjustment, note: v })} />
        </div>
        <button disabled={!adjustment.branchId || !adjustment.itemId || !adjustment.quantityChange || createAdjustment.isPending} className="mt-5 rounded-xl bg-emerald-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => createAdjustment.mutate()}>Buat adjustment</button>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="border-b border-slate-200 p-5"><h3 className="text-xl font-bold">Stock ledger</h3></div><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-left">Ref</th><th className="px-5 py-3 text-right">In</th><th className="px-5 py-3 text-right">Out</th><th className="px-5 py-3 text-right">Balance</th><th className="px-5 py-3 text-right">Cost</th><th className="px-5 py-3 text-left">Note</th></tr></thead><tbody className="divide-y divide-slate-100">{ledgers.data?.map((row) => <tr key={row.id}><td className="px-5 py-3">{new Date(row.createdAt).toLocaleString('id-ID')}</td><td className="px-5 py-3">{row.referenceType} {row.referenceId ?? ''}</td><td className="px-5 py-3 text-right">{number.format(row.quantityIn)}</td><td className="px-5 py-3 text-right">{number.format(row.quantityOut)}</td><td className="px-5 py-3 text-right">{number.format(row.balanceAfter)}</td><td className="px-5 py-3 text-right">{money.format(row.unitCost)}</td><td className="px-5 py-3">{row.note ?? '-'}</td></tr>)}{!ledgers.data?.length && <tr><td colSpan={7} className="px-5 py-8 text-center text-slate-500">Belum ada ledger</td></tr>}</tbody></table></div></section>
    </div>
  )
}

function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; type?: string; onChange: (value: string) => void }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input type={type} className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={value} onChange={(e) => onChange(e.target.value)} /></label>
}
function Select({ label, value, onChange, options }: { label: string; value: string | number; onChange: (value: string | number) => void; options: { value: string | number; label: string }[] }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={value} onChange={(e) => onChange(Number.isNaN(Number(e.target.value)) ? e.target.value : Number(e.target.value))}>{options.map((option) => <option key={String(option.value)} value={option.value}>{option.label}</option>)}</select></label>
}
