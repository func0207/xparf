import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { api } from '../lib/api'

type SaleLine = { id: number; description?: string; quantity: number; unit: string; unitPrice: number; discount: number; lineTotal: number }
type SalePayment = { id: number; amount: number; paymentMethod: number; note?: string }
type Sale = { id: number; saleNumber: string; saleDate: string; subtotal: number; discount: number; tax: number; total: number; paidAmount: number; changeAmount: number; outstandingAmount: number; lines: SaleLine[]; payments: SalePayment[] }

const money = new Intl.NumberFormat('id-ID')

export function ReceiptPage() {
  const { id } = useParams()
  const sale = useQuery({ queryKey: ['sale', id], queryFn: async () => (await api.get<Sale>(`/sales/${id}`)).data, enabled: Boolean(id) })
  if (sale.isLoading) return <p>Loading...</p>
  if (!sale.data) return <p>Struk tidak ditemukan.</p>
  const row = sale.data
  return <div className="mx-auto max-w-sm bg-white p-4 text-black print:max-w-none print:p-0">
    <style>{`@media print { @page { size: 80mm auto; margin: 4mm; } body { background: white; } aside, header, nav, .no-print { display: none !important; } main { margin: 0 !important; } .receipt { width: 72mm; font-family: ui-monospace, SFMono-Regular, Menlo, monospace; font-size: 11px; } }`}</style>
    <button className="no-print mb-4 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white" onClick={() => window.print()}>Print thermal</button>
    <div className="receipt">
      <div className="text-center"><h1 className="text-base font-bold">XPARF STORE</h1><p>Invoice / Struk Penjualan</p></div>
      <div className="my-2 border-y border-dashed border-black py-2"><p>No: {row.saleNumber}</p><p>Tgl: {new Date(row.saleDate).toLocaleString('id-ID')}</p></div>
      <table className="w-full"><tbody>{row.lines.map((line) => <tr key={line.id}><td className="py-1"><div>{line.description}</div><div>{line.quantity} {line.unit} x {money.format(line.unitPrice)}</div></td><td className="py-1 text-right align-bottom">{money.format(line.lineTotal)}</td></tr>)}</tbody></table>
      <div className="my-2 border-y border-dashed border-black py-2 text-right"><p>Subtotal {money.format(row.subtotal)}</p><p>Discount {money.format(row.discount)}</p><p>Tax {money.format(row.tax)}</p><p className="font-bold">Total {money.format(row.total)}</p><p>Bayar {money.format(row.paidAmount)}</p><p>Kembali {money.format(row.changeAmount)}</p><p>Piutang {money.format(row.outstandingAmount)}</p></div>
      <div className="text-center"><p>Terima kasih</p><p>Barang yang sudah dibeli mengikuti kebijakan toko.</p></div>
    </div>
  </div>
}
