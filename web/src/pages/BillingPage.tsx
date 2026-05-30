import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import { StatCard } from '../components/StatCard'

type BillingCoinBalance = { coinBalance: number; isFrozen: boolean }
type TopupPackage = { id: number; name: string; moneyAmount: number; coinAmount: number; sortOrder: number }
type CoinLedger = { id: number; transactionType: number; referenceType: string; referenceId?: number; coinIn: number; coinOut: number; balanceBefore: number; balanceAfter: number; note?: string; createdAt: string }
type CoinTopup = { id: number; topupNumber: string; moneyAmount: number; coinAmount: number; status: number; paymentProvider: string; providerReference: string; qrCodeText?: string; qrCodeImageUrl?: string; expiredAt: string; paidAt?: string }

const money = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' })
const number = new Intl.NumberFormat('id-ID')

export function BillingPage() {
  const queryClient = useQueryClient()
  const balanceQuery = useQuery({ queryKey: ['billing', 'coin-balance'], queryFn: async () => (await api.get<BillingCoinBalance>('/billing/coin-balance')).data })
  const packagesQuery = useQuery({ queryKey: ['billing', 'topup-packages'], queryFn: async () => (await api.get<TopupPackage[]>('/billing/topup-packages')).data })
  const ledgersQuery = useQuery({ queryKey: ['billing', 'coin-ledgers'], queryFn: async () => (await api.get<CoinLedger[]>('/billing/coin-ledgers')).data })

  const topupMutation = useMutation({
    mutationFn: async (topupPackageId: number) => (await api.post<CoinTopup>('/billing/topups', { topupPackageId })).data,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['billing'] })
    },
  })

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold">Billing & Coin</h2>
        <p className="text-slate-500">Topup QRIS, package coin, dan ledger saldo perusahaan.</p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <StatCard title="Coin balance" value={number.format(balanceQuery.data?.coinBalance ?? 0)} description="Saldo coin saat ini" />
        <StatCard title="Status akun" value={balanceQuery.data?.isFrozen ? 'Frozen' : 'Aktif'} description="Freeze jika saldo/aturan billing bermasalah" />
        <StatCard title="Package tersedia" value={String(packagesQuery.data?.length ?? 0)} description="Paket topup aktif" />
      </div>

      {topupMutation.data && (
        <section className="rounded-2xl border border-indigo-200 bg-indigo-50 p-5">
          <h3 className="text-lg font-bold text-indigo-900">Topup dibuat: {topupMutation.data.topupNumber}</h3>
          <p className="mt-1 text-sm text-indigo-800">Bayar {money.format(topupMutation.data.moneyAmount)} untuk {number.format(topupMutation.data.coinAmount)} coin. Expired {new Date(topupMutation.data.expiredAt).toLocaleString('id-ID')}.</p>
          {topupMutation.data.qrCodeImageUrl && <img className="mt-4 h-48 w-48 rounded-xl bg-white object-contain p-2" src={topupMutation.data.qrCodeImageUrl} alt="QRIS topup" />}
          {topupMutation.data.qrCodeText && <pre className="mt-4 overflow-auto rounded-xl bg-white p-3 text-xs text-slate-700">{topupMutation.data.qrCodeText}</pre>}
        </section>
      )}

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 p-5">
          <h3 className="text-xl font-bold">Pilih package topup</h3>
          <p className="text-sm text-slate-500">Generate invoice/topup QRIS dari API.</p>
        </div>
        <div className="grid gap-4 p-5 md:grid-cols-3">
          {packagesQuery.isLoading && <p className="text-slate-500">Loading package...</p>}
          {packagesQuery.data?.map((item) => (
            <div key={item.id} className="rounded-2xl border border-slate-200 p-5">
              <h4 className="font-bold">{item.name}</h4>
              <p className="mt-2 text-2xl font-bold text-indigo-600">{number.format(item.coinAmount)} coin</p>
              <p className="text-sm text-slate-500">{money.format(item.moneyAmount)}</p>
              <button disabled={topupMutation.isPending} className="mt-4 w-full rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60" onClick={() => topupMutation.mutate(item.id)}>Topup</button>
            </div>
          ))}
          {!packagesQuery.isLoading && packagesQuery.data?.length === 0 && <p className="text-slate-500">Package belum tersedia.</p>}
        </div>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 p-5">
          <h3 className="text-xl font-bold">Coin ledger</h3>
          <p className="text-sm text-slate-500">Riwayat topup, sale deduction, adjustment.</p>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-slate-200 text-sm">
            <thead className="bg-slate-50"><tr><th className="px-5 py-3 text-left">Tanggal</th><th className="px-5 py-3 text-left">Reference</th><th className="px-5 py-3 text-right">In</th><th className="px-5 py-3 text-right">Out</th><th className="px-5 py-3 text-right">Balance</th><th className="px-5 py-3 text-left">Note</th></tr></thead>
            <tbody className="divide-y divide-slate-100">
              {ledgersQuery.isLoading && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={6}>Loading ledger...</td></tr>}
              {ledgersQuery.data?.map((row) => (
                <tr key={row.id} className="hover:bg-slate-50">
                  <td className="px-5 py-3">{new Date(row.createdAt).toLocaleString('id-ID')}</td>
                  <td className="px-5 py-3">{row.referenceType}{row.referenceId ? ` #${row.referenceId}` : ''}</td>
                  <td className="px-5 py-3 text-right text-emerald-600">{number.format(row.coinIn)}</td>
                  <td className="px-5 py-3 text-right text-red-600">{number.format(row.coinOut)}</td>
                  <td className="px-5 py-3 text-right font-semibold">{number.format(row.balanceAfter)}</td>
                  <td className="px-5 py-3">{row.note ?? '-'}</td>
                </tr>
              ))}
              {!ledgersQuery.isLoading && ledgersQuery.data?.length === 0 && <tr><td className="px-5 py-8 text-center text-slate-500" colSpan={6}>Ledger kosong</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
