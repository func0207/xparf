import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, getApiErrorMessage } from '../lib/api'
import { ConfirmDialog } from '../components/ConfirmDialog'
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
  const [editingLineId, setEditingLineId] = useState<number | null>(null)
  const [editForm, setEditForm] = useState<LineForm>({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitPrice: 0, discount: 0 })
  const [error, setError] = useState('')

  const branchesQuery = useQuery({ queryKey: ['branches', 'sales'], queryFn: async () => (await api.get<PageResponse<Branch>>('/branches', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const customersQuery = useQuery({ queryKey: ['customers', 'sales'], queryFn: async () => (await api.get<PageResponse<Customer>>('/customers', { params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc' } })).data.items })
  const branchItemsQuery = useQuery({ queryKey: ['branch-items'], queryFn: async () => (await api.get<BranchItem[]>('/branch-items')).data })
  const salesQuery = useQuery({ queryKey: ['sales'], queryFn: async () => (await api.get<Sale[]>('/sales')).data })

  const availableItems = useMemo(() => branchItemsQuery.data?.filter((item) => item.isAvailable && (!header.branchId || item.branchId === header.branchId)) ?? [], [branchItemsQuery.data, header.branchId])

  const createSale = useMutation({
    mutationFn: async () => (await api.post<Sale>('/sales', { branchId: header.branchId, customerId: header.customerId ? Number(header.customerId) : null, saleNumber: header.saleNumber, saleDate: header.saleDate, saleType: header.saleType, discount: header.discount, tax: header.tax, idempotencyKey: crypto.randomUUID() })).data,
    onSuccess: (data) => { setSale(data); setStep(2); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal membuat draft penjualan.')),
  })

  const addLine = useMutation({
    mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/lines`, line)).data,
    onSuccess: (data) => { setSale(data); setLine({ itemId: 0, description: '', quantity: 1, unit: 'PCS', unitPrice: 0, discount: 0 }); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal menambah item.')),
  })

  const updateLine = useMutation({
    mutationFn: async ({ lineId, form }: { lineId: number; form: LineForm }) => (await api.put<Sale>(`/sales/${sale?.id}/lines/${lineId}`, { description: form.description, quantity: form.quantity, unit: form.unit, unitPrice: form.unitPrice, discount: form.discount })).data,
    onSuccess: (data) => { setSale(data); setEditingLineId(null); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal update line.')),
  })

  const deleteLine = useMutation({
    mutationFn: async (lineId: number) => (await api.delete<Sale>(`/sales/${sale?.id}/lines/${lineId}`)).data,
    onSuccess: setSale,
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal hapus line.')),
  })

  const addPayment = useMutation({
    mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/payments`, { paymentDate: new Date().toISOString(), amount: payment.amount, paymentMethod: payment.paymentMethod, note: payment.note })).data,
    onSuccess: (data) => { setSale(data); setPayment({ amount: 0, paymentMethod: 0, note: '' }); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal tambah pembayaran.')),
  })

  const postSale = useMutation({
    mutationFn: async () => (await api.post<Sale>(`/sales/${sale?.id}/post`)).data,
    onSuccess: (data) => { setSale(data); setStep(3); queryClient.invalidateQueries({ queryKey: ['sales'] }); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal posting penjualan.')),
  })

  const voidSale = useMutation({
    mutationFn: async (saleId: number) => (await api.post<Sale>(`/sales/${saleId}/void`)).data,
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['sales'] }); setError('') },
    onError: (err) => setError(getApiErrorMessage(err, 'Gagal void penjualan.')),
  })

  function selectItem(itemId: number) {
    const selected = availableItems.find((item) => item.itemId === itemId)
    setLine({ ...line, itemId, description: selected?.itemName ?? '', unitPrice: header.saleType === 1 ? selected?.wholesalePrice ?? 0 : selected?.sellingPrice ?? 0 })
  }

  function startEdit(line: SaleLine) {
    setEditingLineId(line.id)
    setEditForm({ itemId: line.itemId, description: line.description ?? '', quantity: line.quantity, unit: line.unit, unitPrice: line.unitPrice, discount: line.discount })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Penjualan</h2>
        <p className="text-slate-500">Wizard penjualan retail/grosir: header, detail, pembayaran, posting.</p>
      </div>

      {error && <div className="rounded-2xl bg-red-50 p-4 text-sm font-medium text-red-700">{error}<button className="float-right font-bold" onClick={() => setError('')}>×</button></div>}

      <div className="grid gap-3 md:grid-cols-3">
        {['Header', 'Detail', 'Konfirmasi'].map((label, index) => <div key={label} className={`rounded-2xl border p-4 ${step === index + 1 ? 'border-indigo-300 bg-indigo-50 text-indigo-800' : 'border-slate-200 bg-white text-slate-500'}`}><p className="text-sm font-semibold">Step {index + 1}</p><p className="text-lg font-bold">{label}</p></div>)}
      </div>

      {step === 1 && (
        <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="text-xl font-bold">Header penjualan</h3>
          <div className="mt-5 grid gap-4 md:grid-cols-3">
            <Select label="Cabang" value={header.branchId} onChange={(branchId) => setHeader({ ...header, branchId: Number(branchId) })} options={(branchesQuery.data ?? []).map((b) => ({ value: b.id, label: `${b.code} - ${b.name}` }))} />
            <Select label="Konsumen" value={header.customerId} onChange={(customerId) => setHeader({ ...header, customerId: String(customerId) })} options={[{ value: '', label: 'Cash / tanpa konsumen' }, ...(customersQuery.data ?? []).map((c) => ({ value: c.id, label: `${c.code} - ${c.name}` }))]} />
            <Input label="Nomor" value={header.saleNumber} onChange={(saleNumber) => setHeader({ ...header, saleNumber })} />
            <Input label="Tanggal" type="date" value={header.saleDate} onChange={(saleDate) => setHeader({ ...header, saleDate })} />
            <Select label="Tipe" value={header.saleType} onChange={(saleType) => setHeader({ ...header, saleType: Number(saleType) })} options={[{ value: 0, label: 'Retail' }, { value: 1, label: 'Grosir' }]} />
            <Input label="Discount" type="number" value={String(header.discount)} onChange={(discount) => setHeader({ ...header, discount: Number(discount) })} />
            <Input label="Tax" type="number" value={String(header.tax)} onChange={(tax) => setHeader({ ...header, tax: Number(tax) })} />
          </div>
          <button disabled={!header.branchId || createSale.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => createSale.mutate()}>{createSale.isPending ? 'Creating...' : 'Buat draft'}</button>
        </section>
      )}

      {step === 2 && sale && (
        <section className="space-y-5">
          <SaleSummary sale={sale} />
          {editingLineId ? (
            <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5 shadow-sm">
              <h3 className="text-xl font-bold">Edit line #{editingLineId}</h3>
              <div className="mt-5 grid gap-4 md:grid-cols-5">
                <Input label="Qty" type="number" value={String(editForm.quantity)} onChange={(quantity) => setEditForm({ ...editForm, quantity: Number(quantity) })} />
                <Input label="Unit" value={editForm.unit} onChange={(unit) => setEditForm({ ...editForm, unit })} />
                <Input label="Harga" type="number" value={String(editForm.unitPrice)} onChange={(unitPrice) => setEditForm({ ...editForm, unitPrice: Number(unitPrice) })} />
                <Input label="Discount" type="number" value={String(editForm.discount)} onChange={(discount) => setEditForm({ ...editForm, discount: Number(discount) })} />
              </div>
              <div className="mt-4 flex gap-3">
                <button disabled={updateLine.isPending} className="rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => updateLine.mutate({ lineId: editingLineId, form: editForm })}>Simpan</button>
                <button className="rounded-xl border px-4 py-2 font-semibold" onClick={() => setEditingLineId(null)}>Batal</button>
              </div>
            </div>
          ) : (
            <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
              <h3 className="text-xl font-bold">Tambah item</h3>
              <div className="mt-5 grid gap-4 md:grid-cols-6">
                <label className="block md:col-span-2"><span className="text-sm font-medium text-slate-700">Barang</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={line.itemId} onChange={(e) => selectItem(Number(e.target.value))}><option value={0}>Pilih barang</option>{availableItems.map((item) => <option key={`${item.branchId}-${item.itemId}`} value={item.itemId}>{item.itemSku} - {item.itemName} ({number.format(item.quantityOnHand)})</option>)}</select></label>
                <Input label="Qty" type="number" value={String(line.quantity)} onChange={(quantity) => setLine({ ...line, quantity: Number(quantity) })} />
                <Input label="Unit" value={line.unit} onChange={(unit) => setLine({ ...line, unit })} />
                <Input label="Harga" type="number" value={String(line.unitPrice)} onChange={(unitPrice) => setLine({ ...line, unitPrice: Number(unitPrice) })} />
                <Input label="Discount" type="number" value={String(line.discount)} onChange={(discount) => setLine({ ...line, discount: Number(discount) })} />
              </div>
              <button disabled={!line.itemId || addLine.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => addLine.mutate()}>Tambah line</button>
            </div>
          )}
          <LinesTable lines={sale.lines} onEdit={startEdit} onDelete={(lineId) => deleteLine.mutate(lineId)} />
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><h3 className="text-xl font-bold">Pembayaran</h3><div className="mt-5 grid gap-4 md:grid-cols-3"><Input label="Amount" type="number" value={String(payment.amount)} onChange={(amount) => setPayment({ ...payment, amount: Number(amount) })} /><Select label="Method" value={payment.paymentMethod} onChange={(paymentMethod) => setPayment({ ...payment, paymentMethod: Number(paymentMethod) })} options={[{ value: 0, label: 'Cash' }, { value: 1, label: 'Transfer' }, { value: 2, label: 'QRIS' }, { value: 3, label: 'Coin' }]} /><Input label="Note" value={payment.note} onChange={(note) => setPayment({ ...payment, note })} /></div><button disabled={!payment.amount || addPayment.isPending} className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => addPayment.mutate()}>Tambah pembayaran</button></div>
          <div className="flex gap-3">
            <button disabled={postSale.isPending || !sale.lines.length} className="rounded-xl bg-emerald-600 px-4 py-3 font-semibold text-white hover:bg-emerald-700 disabled:opacity-60" onClick={() => postSale.mutate()}>{postSale.isPending ? 'Posting...' : 'Post penjualan'}</button>
            <Link className="rounded-xl border border-slate-200 px-4 py-3 font-semibold hover:bg-slate-50" to={`/sales/${sale.id}/receipt`}>Struk</Link>
          </div>
        </section>
      )}

      {step === 3 && sale && (
        <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6">
          <h3 className="text-2xl font-bold text-emerald-900">Penjualan selesai</h3>
          <p className="mt-2 text-emerald-800">{sale.saleNumber} total {money.format(sale.total)}. Coin deducted {number.format(sale.coinDeducted)}.</p>
          <button className="mt-5 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white" onClick={() => { setSale(null); setStep(1); setHeader({ branchId: 0, customerId: '', saleNumber: `SO-${Date.now()}`, saleDate: today(), saleType: 0, discount: 0, tax: 0 }) }}>Transaksi baru</button>
        </section>
      )}

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 p-5"><h3 className="text-xl font-bold">Riwayat penjualan</h3></div>
        <div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Nomor</th><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-right">Outstanding</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{salesQuery.data?.map((row) => <tr key={row.id} className="hover:bg-slate-50"><td className="px-5 py-3">{row.saleNumber}</td><td className="px-5 py-3">{new Date(row.saleDate).toLocaleString('id-ID')}</td><td className="px-5 py-3 text-right">{money.format(row.total)}</td><td className="px-5 py-3 text-right">{money.format(row.outstandingAmount)}</td><td className="px-5 py-3"><div className="flex gap-2"><Link className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-semibold" to={`/sales/${row.id}/receipt`}>Struk</Link><ConfirmDialog title="Void penjualan?" description={`Void ${row.saleNumber} total ${money.format(row.total)}? Tidak bisa dibatalkan.`} onConfirm={() => voidSale.mutate(row.id)}><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600 disabled:opacity-40" disabled={row.status !== 1}>Void</button></ConfirmDialog></div></td></tr>)}</tbody></table></div>
      </section>
    </div>
  )
}

function SaleSummary({ sale }: { sale: Sale }) {
  return <div className="grid gap-4 md:grid-cols-4"><Card label="Subtotal" value={money.format(sale.subtotal)} /><Card label="Total" value={money.format(sale.total)} /><Card label="Paid" value={money.format(sale.paidAmount)} /><Card label="Outstanding" value={money.format(sale.outstandingAmount)} /></div>
}

function LinesTable({ lines, onEdit, onDelete }: { lines: SaleLine[]; onEdit: (line: SaleLine) => void; onDelete: (lineId: number) => void }) {
  return <section className="rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="min-w-full divide-y divide-slate-200 text-sm"><thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Item</th><th className="px-5 py-3 text-right">Qty</th><th className="px-5 py-3 text-right">Harga</th><th className="px-5 py-3 text-right">Total</th><th className="px-5 py-3 text-left">Aksi</th></tr></thead><tbody className="divide-y divide-slate-100">{lines.map((line) => <tr key={line.id}><td className="px-5 py-3">{line.description ?? line.itemId}</td><td className="px-5 py-3 text-right">{number.format(line.quantity)}</td><td className="px-5 py-3 text-right">{money.format(line.unitPrice)}</td><td className="px-5 py-3 text-right">{money.format(line.lineTotal)}</td><td className="px-5 py-3"><div className="flex gap-2"><button className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-semibold hover:bg-slate-100" onClick={() => onEdit(line)}>Edit</button><ConfirmDialog title="Hapus line?" description="Line akan dihapus dari penjualan." onConfirm={() => onDelete(line.id)}><button className="rounded-lg border border-red-200 px-3 py-1 text-xs font-semibold text-red-600">Hapus</button></ConfirmDialog></div></td></tr>)}{lines.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={5}>Belum ada line</td></tr>}</tbody></table></div></section>
}

function Card({ label, value }: { label: string; value: string }) {
  return <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><p className="text-sm text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold">{value}</p></div>
}

function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; type?: string; onChange: (value: string) => void }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><input type={type} className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500" value={value} onChange={(event) => onChange(event.target.value)} /></label>
}

function Select({ label, value, onChange, options }: { label: string; value: string | number; onChange: (value: number | string) => void; options: { value: string | number; label: string }[] }) {
  return <label className="block"><span className="text-sm font-medium text-slate-700">{label}</span><select className="mt-1 w-full rounded-xl border border-slate-300 px-4 py-3" value={value} onChange={(event) => onChange(Number.isNaN(Number(event.target.value)) ? event.target.value : Number(event.target.value))}>{options.map((option) => <option key={String(option.value)} value={option.value}>{option.label}</option>)}</select></label>
}
