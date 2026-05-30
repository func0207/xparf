import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import type { PageResponse } from '../components/DataTable'

type Branch = { id: number; code: string; name: string }
type Customer = { id: number; code: string; name: string }
type BranchItem = { id: number; branchId: number; itemId: number; itemSku: string; itemName: string; quantityOnHand: number; sellingPrice: number; wholesalePrice: number; isAvailable: boolean }
type Sale = { id: number; branchId: number; customerId?: number; saleNumber: string; saleDate: string; saleType: number; subtotal: number; discount: number; tax: number; total: number; paidAmount: number; changeAmount: number; outstandingAmount: number; coinDeducted: number; status: number; paymentStatus: number; lines: SaleLine[]; payments: SalePayment[] }
type SaleLine = { id: number; itemId: number; description?: string; quantity: number; unit: string; unitPrice: number; discount: number; lineTotal: number }
type SalePayment = { id: number; paymentDate: string; amount: number; paymentMethod: number; note?: string }

type HeaderForm = { branchId: number; customerId: string; saleNumber: string; saleDate: string; saleType: number; discount: number; tax: number }
type LineForm = { itemId: number; description: string; quantity: number; unit: string; unitPrice: number; discount: number }
type PaymentForm = { amount: number; paymentMethod: number; note: string }

const today = () => new Date().toISOString().slice(0, 10)
const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })
const number = new Intl.NumberFormat('id-ID')

export function SalesWizardPage() {
  const queryClient = useQueryClient()
  const [step, setStep] = useState(1)
  const [sale, setSale] = useState<Sale | null>(null)
  const [header, setHeader] = useState<HeaderForm>({ branchId: 0, customerId: '', saleNumber: `SO-${Date.now()}`, saleDate: today(), saleType: 0, discount: 0, tax: 0 })
  const [line, setLine] = useState<LineForm>({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitPrice: 0, discount: 0 })
  const [payment, setPayment] = useState<PaymentForm>({ amount: 0, paymentMethod: 0, note: '' })

  const branchesQuery = useQuery({ queryKey: ['branches', 'sales'], queryFn: async () => (await api.get<PageResponse<Branch>>('/branches', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const customersQuery = useQuery({ queryKey: ['customers', 'sales'], queryFn: async () => (await api.get<PageResponse<Customer>>('/customers', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const branchItemsQuery = useQuery({ queryKey: ['branch-items'], queryFn: async () => (await api.get<BranchItem[]>('/branch-items')).data })
  const salesQuery = useQuery({ queryKey: ['sales'], queryFn: async () => (await api.get<Sale[]>('/sales')).data })

  const availableItems = useMemo(() => branchItemsQuery.data?.filter((item) => item.isAvailable && (!header.branchId || item.branchId === header.branchId)) ?? [], [branchItemsQuery.data, header.branchId])

  const createSale = useMutation({
    mutationFn: async () => (await api.post<Sale>('/sales', { branchId: header.branchId, customerId: header.customerId ? Number(header.customerId) : null, saleNumber: header.saleNumber, saleDate: header.saleDate, saleType: header.saleType, discount: header.discount, tax: header.tax, idempotencyKey: crypto.randomUUID() })).data,
    onSuccess: (data) => { setSale(data); setStep(2); queryClient.invalidateQueries({ queryKey: ['sales'] }) },
  })

  const addLine = useMutation({
    mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/lines`, line)).data,
    onSuccess: (data) => { setSale(data); setLine({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitPrice: 0, discount: 0 }) },
  })

  const deleteLine = useMutation({ mutationFn: async (lineId: number) => (await api.delete<Sale>(`/sales/${sale?.id}/lines/${lineId}`)).data, onSuccess: setSale })
  const addPayment = useMutation({ mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/payments`, { paymentDate: new Date().toISOString(), amount: payment.amount, paymentMethod: payment.paymentMethod, note: payment.note })).data, onSuccess: (data) => { setSale(data); setPayment({ amount: 0, paymentMethod: 0, note: '' }) } })
  const postSale = useMutation({ mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/post`)).data, onSuccess: (data) => { setSale(data); setStep(3); queryClient.invalidateQueries({ queryKey: ['sales'] }) } })
  const voidSale = useMutation({ mutationFn: async (saleId: number) => (await api.post<Sale>(`/sales/${saleId}/void`)).data, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sales'] }) })

  function selectItem(itemId: number) {
    const selected = availableItems.find((item) => item.itemId === itemId)
    setLine({ ...line, itemId, description: selected?.itemName ?? '', unitPrice: header.saleType === 1 ? selected?.wholesalePrice ?? 0 : selected?.sellingPrice ?? 0 })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Penjualan</h2>
        <p className="text-slate-500">Wizard penjualan retail/grosir: header, detail, pembayaran, posting.</p>
      </div>

      <div className="grid gap-3 md:grid-cols-3">
        {['Header', 'Detail', 'Konfirmasi'].map((label, index) => <div key={label} className={`rounded-2xl border p-4 ${step === index + 1 ? 'border-indigo-300 bg-indigo-50 text-indigo-800' : 'border-slate-200 bg-white text-slate-500'}`}><p className="text-sm font-semibold">Step {index + 1}</p><p className="text-lg font-bold">{label}</p></div>)}
      </div>

      {step === 1 && <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><h3 className="text-xl font-bold">Header penjualan</h3><div className="mt-5 grid gap-4 md:grid-cols-3"><Select label="Cabang" value={header.branchId} onChange={(branchId) => setHeader({ ...header, branchId: Number(branchId) })} options={(branchesQuery.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))} /><Select label="Konsumen" value={header.customerId} onChange={(customerId) => setHeader({ ...header, customerId: String(customerId) })} options={[{ value: '', label: 'Cash / tanpa konsumen' }, ...(customersQuery.data ?? []).map((c) => ({ value: c.id, label: `${c.code} - ${c.name}` }))]} /><Input label="Nomor" value={header.saleNumber} onChange={(saleNumber) => setHeader({ ...header, saleNumber })} /><Input label="Tanggal" type="date" value={header.saleDate} onChange={(saleDate) => setHeader({ ...header, saleDate })} /><Select label="Tipe" value={header.saleType} onChange={(saleType) => setHeader({ ...header, saleType: Number(saleType) })} options={[{ value: 0, label: 'Retail' }, { value: 1, label: 'Grosir' }]} /><Input label="Discount" type="number" value={String(header.discount)} onChange={(discount) => setHeader({ ...header, discount: Number(discount) })} /><Input label="Tax" type="number" value={String(header.tax)} onChange={(tax) => setHeader({ ...header, tax: Number(tax) })} /></div><button disabled={!header.branchId || createSale.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => createSale.mutate()}>{createSale.isPending ? 'Creating...' : 'Buat draft'}</button>{createSale.isError && <p className="mt-3 rounded-xl bg-red-50 p-3 text-sm text-red-700">Gagal membuat draft penjualan.</p>}</section>}

      {step === 2 && sale && <section className="space-y-5"><SaleSummary sale={sale} /><div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><h3 className="text-xl font-bold">Tambah item</h3><div className="mt-5 grid gap-4 md:grid-cols-6"><label className="block md:col-span-2"><span className="text-sm font-medium text-slate-700">Barang</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={line.itemId} onChange={(e) => selectItem(Number(e.target.value))}><option value={0}>Pilih barang</option>{availableItems.map((item) => <option key={`${item.branchId}-${item.itemId}`} value={item.itemId}>{item.itemSku} - {item.itemName} ({number.format(item.quantityOnHand)})</option>)}</select></label><Input label="Qty" type="number" value={String(line.quantity)} onChange={(quantity) => setLine({ ...line, quantity: Number(quantity) })} /><Input label="Unit" value={line.unit} onChange={(unit) => setLine({ ...line, unit })} /><Input label="Harga" type="number" value={String(line.unitPrice)} onChange={(unitPrice) => setLine({ ...line, unitPrice: Number(unitPrice) })} /><Input label="Discount" type="number" value={String(line.discount)} onChange={(discount) => setLine({ ...line, discount: Number(discount) })} /></div><button disabled={!line.itemId || addLine.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => addLine.mutate()}>Tambah line</button></div><LinesTable lines={sale.lines} onDelete={(lineId) => deleteLine.mutate(lineId)} /><div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><h3 className="text-xl font-bold">Pembayaran</h3><div className="mt-5 grid gap-4 md:grid-cols-3"><Input label="Amount" type="number" value={String(payment.amount)} onChange={(amount) => setPayment({ ...payment, amount: Number(amount) })} /><Select label="Method" value={payment.paymentMethod} onChange={(paymentMethod) => setPayment({ ...payment, paymentMethod: Number(paymentMethod) })} options={[{ value: 0, label: 'Cash' }, { value: 1, label: 'Transfer' }, { value: 2, label: 'Card' }, { value: 3, label: 'QRIS' }]} /><Input label="Note" value={payment.note} onChange={(note) => setPayment({ ...payment, note })} /></div><div className="mt-5 flex gap-3"><button disabled={!payment.amount || addPayment.isPending} className="rounded-xl bg-emerald-600 px-4 py-2 font-semibold text-white hover:bg-emerald-700 disabled:opacity-60" onClick={() => addPayment.mutate()}>Tambah payment</button><button disabled={!sale.lines.length || postSale.isPending} className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => postSale.mutate()}>Post penjualan</button></div></div></section>}

      {step === 3 && sale && <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6"><h3 className="text-2xl font-bold text-emerald-900">Penjualan selesai</h3><p className="mt-2 text-emerald-800">{sale.saleNumber} total {money.format(sale.total)}. Coin deducted {number.format(sale.coinDeducted)}.</p><button className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white" onClick={() => { setSale(null); setStep(1); setHeader({ branchId: 0, customerId: '', saleNumber: `SO-${Date.now()}`, saleDate: today(), saleType: 0, discount: 0, tax: 0 }) }}>Transaksi baru</button></section>}

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="border-b border-slate-200 p-5"><h3 className="text-xl font-bold">Riwayat penjualan</h3></div><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Nomor</th><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-right">Outstanding</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{salesQuery.data?.map((row) => <tr key={row.id} className="hover:bg-slate-50"><td className="px-5 py-3">{row.saleNumber}</td><td className="px-5 py-3">{new Date(row.saleDate).toLocaleString('id-ID')}</td><td className="px-5 py-3 text-right">{money.format(row.total)}</td><td className="px-5 py-3 text-right">{money.format(row.outstandingAmount)}</td><td className="px-5 py-3"><div className="flex gap-2"><Link className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-semibold" to={`/sales/${row.id}/receipt`}>Struk</Link><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600 disabled:opacity-40" disabled={row.status !== 1} onClick={() => confirm('Void penjualan ini?') && voidSale.mutate(row.id)}>Void</button></div></td></tr>)}</tbody></table></div></section>
    </div>
  )
}

function SaleSummary({ sale }: { sale: Sale }) {
  return <div className="grid gap-4 md:grid-cols-4"><Card label="Subtotal" value={money.format(sale.subtotal)} /><Card label="Total" value={money.format(sale.total)} /><Card label="Paid" value={money.format(sale.paidAmount)} /><Card label="Outstanding" value={money.format(sale.outstandingAmount)} /></div>
}

function LinesTable({ lines, onDelete }: { lines: SaleLine[]; onDelete: (lineId: number) => void }) {
  return <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Item</th><th className="px-5 py-3 text-right">Qty</th><th className="px-5 py-3 text-right">Harga</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{lines.map((line) => <tr key={line.id}><td className="px-5 py-3">{line.description ?? line.itemId}</td><td className="px-5 py-3 text-right">{number.format(line.quantity)}</td><td className="px-5 py-3 text-right">{money.format(line.unitPrice)}</td><td className="px-5 py-3 text-right">{money.format(line.lineTotal)}</td><td className="px-5 py-3"><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600" onClick={() => onDelete(line.id)}>Hapus</button></td></tr>)}{lines.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={5}>Belum ada line</td></tr>}</tbody></table></div></section>
}

function Card({ label, value }: { label: string; value: string }) {
  return <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><p className="text-sm text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold">{value}</p></div>
}

function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; type?: string; onChange: (value: string) => void }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input type={type} className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={value} onChange={(e) => onChange(e.target.value)} /></label>
}

function Select({ label, value, onChange, options }: { label: string; value: string | number; onChange: (value: number | string) => void; options: { value: string | number; label: string }[] }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={value} onChange={(e) => onChange(Number.isNaN(Number(e.target.value)) ? e.target.value : Number(e.target.value))}>{options.map((option) => <option key={String(option.value)} value={option.value}>{option.label}</option>)}</select></label>
}
