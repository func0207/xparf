import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import type { PageResponse } from '../components/DataTable'
import { api } from '../lib/api'

type Branch = { id: number; code: string; name: string }
type Supplier = { id: number; code: string; name: string }
type Item = { id: number; sku: string; name: string; baseUnit: string }
type PurchaseLine = { id: number; itemId: number; description?: string; quantity: number; unit: string; unitCost: number; discount: number; lineTotal: number }
type Purchase = { id: number; branchId: number; supplierId?: number; purchaseNumber: string; purchaseDate: string; subtotal: number; discount: number; tax: number; total: number; paidAmount: number; outstandingAmount: number; status: number; lines: PurchaseLine[] }

type Header = { branchId: number; supplierId: string; purchaseNumber: string; purchaseDate: string; discount: number; tax: number }
type Line = { itemId: number; description: string; quantity: number; unit: string; unitCost: number; discount: number }
type Payment = { amount: number; paymentMethod: number; note: string }

const today = () => new Date().toISOString().slice(0, 10)
const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })

export function PurchaseWizardPage() {
  const qc = useQueryClient()
  const [step, setStep] = useState(1)
  const [purchase, setPurchase] = useState<Purchase | null>(null)
  const [header, setHeader] = useState<Header>({ branchId: 0, supplierId: '', purchaseNumber: `PO-${Date.now()}`, purchaseDate: today(), discount: 0, tax: 0 })
  const [line, setLine] = useState<Line>({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitCost: 0, discount: 0 })
  const [payment, setPayment] = useState<Payment>({ amount: 0, paymentMethod: 0, note: '' })

  const branches = useQuery({ queryKey: ['branches', 'purchase'], queryFn: async () => (await api.get<PageResponse<Branch>>('/branches', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const suppliers = useQuery({ queryKey: ['suppliers', 'purchase'], queryFn: async () => (await api.get<PageResponse<Supplier>>('/suppliers', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const items = useQuery({ queryKey: ['items', 'purchase'], queryFn: async () => (await api.get<PageResponse<Item>>('/items', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const purchases = useQuery({ queryKey: ['purchases'], queryFn: async () => (await api.get<Purchase[]>('/purchases')).data })
  const selectedItem = useMemo(() => items.data?.find((i) => i.id === line.itemId), [items.data, line.itemId])

  const createPurchase = useMutation({
    mutationFn: async () => (await api.post<Purchase>('/purchases', { branchId: header.branchId, supplierId: header.supplierId ? Number(header.supplierId) : null, purchaseNumber: header.purchaseNumber, purchaseDate: header.purchaseDate, discount: header.discount, tax: header.tax })).data,
    onSuccess: (data) => { setPurchase(data); setStep(2); qc.invalidateQueries({ queryKey: ['purchases'] }) },
  })
  const addLine = useMutation({
    mutationFn: async () => (await api.post<Purchase>(`/purchases/${purchase?.id}/lines`, { ...line, description: line.description || selectedItem?.name })).data,
    onSuccess: (data) => { setPurchase(data); setLine({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitCost: 0, discount: 0 }) },
  })
  const deleteLine = useMutation({ mutationFn: async (lineId: number) => (await api.delete<Purchase>(`/purchases/${purchase?.id}/lines/${lineId}`)).data, onSuccess: setPurchase })
  const addPayment = useMutation({ mutationFn: async () => (await api.post<Purchase>(`/purchases/${purchase?.id}/payments`, { paymentDate: new Date().toISOString(), amount: payment.amount, paymentMethod: payment.paymentMethod, note: payment.note })).data, onSuccess: (data) => { setPurchase(data); setPayment({ amount: 0, paymentMethod: 0, note: '' }) } })
  const postPurchase = useMutation({ mutationFn: async () => (await api.post<Purchase>(`/purchases/${purchase?.id}/post`)).data, onSuccess: (data) => { setPurchase(data); setStep(3); qc.invalidateQueries({ queryKey: ['purchases'] }) } })
  const cancelPurchase = useMutation({ mutationFn: async (id: number) => (await api.post<Purchase>(`/purchases/${id}/cancel`)).data, onSuccess: () => qc.invalidateQueries({ queryKey: ['purchases'] }) })

  function reset() {
    setPurchase(null)
    setStep(1)
    setHeader({ branchId: 0, supplierId: '', purchaseNumber: `PO-${Date.now()}`, purchaseDate: today(), discount: 0, tax: 0 })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Pembelian</h2>
        <p className="text-slate-500">Wizard pembelian: header, detail, pembayaran, konfirmasi.</p>
      </div>
      <div className="grid gap-3 md:grid-cols-3">{['Header', 'Detail', 'Konfirmasi'].map((x, i) => <div key={x} className={`rounded-2xl border p-4 ${step === i + 1 ? 'border-indigo-300 bg-indigo-50 text-indigo-800' : 'border-slate-200 bg-white text-slate-500'}`}><p className="text-sm font-semibold">Step {i + 1}</p><p className="text-lg font-bold">{x}</p></div>)}</div>

      {step === 1 && <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 className="text-xl font-bold">Header pembelian</h3>
        <div className="mt-5 grid gap-4 md:grid-cols-3">
          <Select label="Cabang" value={header.branchId} onChange={(v) => setHeader({ ...header, branchId: Number(v) })} options={(branches.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))} />
          <Select label="Supplier" value={header.supplierId} onChange={(v) => setHeader({ ...header, supplierId: String(v) })} options={[{ value: '', label: 'Tanpa supplier' }, ...(suppliers.data ?? []).map((s) => ({ value: s.id, label: `${s.code} - ${s.name}` }))]} />
          <Input label="Nomor" value={header.purchaseNumber} onChange={(v) => setHeader({ ...header, purchaseNumber: v })} />
          <Input label="Tanggal" type="date" value={header.purchaseDate} onChange={(v) => setHeader({ ...header, purchaseDate: v })} />
          <Input label="Discount" type="number" value={String(header.discount)} onChange={(v) => setHeader({ ...header, discount: Number(v) })} />
          <Input label="Tax" type="number" value={String(header.tax)} onChange={(v) => setHeader({ ...header, tax: Number(v) })} />
        </div>
        <button disabled={!header.branchId || createPurchase.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => createPurchase.mutate()}>Buat draft</button>
      </section>}

      {step === 2 && purchase && <section className="space-y-5">
        <Summary p={purchase} />
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="text-xl font-bold">Tambah item</h3>
          <div className="mt-5 grid gap-4 md:grid-cols-6">
            <Select label="Barang" value={line.itemId} onChange={(v) => { const item = items.data?.find((i) => i.id === Number(v)); setLine({ ...line, itemId: Number(v), description: item?.name ?? '', unit: item?.baseUnit ?? 'PCS' }) }} options={[{ value: 0, label: 'Pilih barang' }, ...(items.data ?? []).map((i) => ({ value: i.id, label: `${i.sku} - ${i.name}` }))]} />
            <Input label="Qty" type="number" value={String(line.quantity)} onChange={(v) => setLine({ ...line, quantity: Number(v) })} />
            <Input label="Unit" value={line.unit} onChange={(v) => setLine({ ...line, unit: v })} />
            <Input label="Cost" type="number" value={String(line.unitCost)} onChange={(v) => setLine({ ...line, unitCost: Number(v) })} />
            <Input label="Discount" type="number" value={String(line.discount)} onChange={(v) => setLine({ ...line, discount: Number(v) })} />
          </div>
          <button disabled={!line.itemId || addLine.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => addLine.mutate()}>Tambah line</button>
        </div>
        <Lines lines={purchase.lines} onDelete={(id) => deleteLine.mutate(id)} />
        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="text-xl font-bold">Pembayaran</h3>
          <div className="mt-5 grid gap-4 md:grid-cols-3">
            <Input label="Amount" type="number" value={String(payment.amount)} onChange={(v) => setPayment({ ...payment, amount: Number(v) })} />
            <Select label="Method" value={payment.paymentMethod} onChange={(v) => setPayment({ ...payment, paymentMethod: Number(v) })} options={[{ value: 0, label: 'Cash' }, { value: 1, label: 'Transfer' }, { value: 2, label: 'Card' }, { value: 3, label: 'QRIS' }]} />
            <Input label="Note" value={payment.note} onChange={(v) => setPayment({ ...payment, note: v })} />
          </div>
          <div className="mt-5 flex gap-3"><button disabled={!payment.amount} className="rounded-xl bg-emerald-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => addPayment.mutate()}>Tambah payment</button><button disabled={!purchase.lines.length} className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white disabled:opacity-60" onClick={() => postPurchase.mutate()}>Post pembelian</button></div>
        </div>
      </section>}

      {step === 3 && purchase && <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6"><h3 className="text-2xl font-bold text-emerald-900">Pembelian selesai</h3><p className="mt-2 text-emerald-800">{purchase.purchaseNumber} total {money.format(purchase.total)}.</p><button className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white" onClick={reset}>Transaksi baru</button></section>}

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="border-b border-slate-200 p-5"><h3 className="text-xl font-bold">Riwayat pembelian</h3></div><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Nomor</th><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-right">Outstanding</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{purchases.data?.map((p) => <tr key={p.id}><td className="px-5 py-3">{p.purchaseNumber}</td><td className="px-5 py-3">{new Date(p.purchaseDate).toLocaleString('id-ID')}</td><td className="px-5 py-3 text-right">{money.format(p.total)}</td><td className="px-5 py-3 text-right">{money.format(p.outstandingAmount)}</td><td className="px-5 py-3"><button disabled={p.status !== 1} className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600 disabled:opacity-40" onClick={() => confirm('Cancel pembelian ini?') && cancelPurchase.mutate(p.id)}>Cancel</button></td></tr>)}</tbody></table></div></section>
    </div>
  )
}

function Summary({ p }: { p: Purchase }) {
  return <div className="grid gap-4 md:grid-cols-4"><Card label="Subtotal" value={money.format(p.subtotal)} /><Card label="Total" value={money.format(p.total)} /><Card label="Paid" value={money.format(p.paidAmount)} /><Card label="Outstanding" value={money.format(p.outstandingAmount)} /></div>
}
function Lines({ lines, onDelete }: { lines: PurchaseLine[]; onDelete: (id: number) => void }) {
  return <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Item</th><th className="px-5 py-3 text-right">Qty</th><th className="px-5 py-3 text-right">Cost</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{lines.map((l) => <tr key={l.id}><td className="px-5 py-3">{l.description ?? l.itemId}</td><td className="px-5 py-3 text-right">{l.quantity}</td><td className="px-5 py-3 text-right">{money.format(l.unitCost)}</td><td className="px-5 py-3 text-right">{money.format(l.lineTotal)}</td><td className="px-5 py-3"><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600" onClick={() => onDelete(l.id)}>Hapus</button></td></tr>)}{lines.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={5}>Belum ada line</td></tr>}</tbody></table></div></section>
}
function Card({ label, value }: { label: string; value: string }) { return <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><p className="text-sm text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold">{value}</p></div> }
function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; type?: string; onChange: (v: string) => void }) { return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input type={type} className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={value} onChange={(e) => onChange(e.target.value)} /></label> }
function Select({ label, value, onChange, options }: { label: string; value: string | number; onChange: (v: string | number) => void; options: { value: string | number; label: string }[] }) { return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={value} onChange={(e) => onChange(Number.isNaN(Number(e.target.value)) ? e.target.value : Number(e.target.value))}>{options.map((o) => <option key={String(o.value)} value={o.value}>{o.label}</option>)}</select></label> }
