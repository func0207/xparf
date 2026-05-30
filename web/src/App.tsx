import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { MainLayout } from './layouts/MainLayout'
import { DashboardPage } from './pages/DashboardPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { LandingPage } from './pages/LandingPage'
import { branchColumns, customerColumns, itemColumns, ListPage, supplierColumns } from './pages/ListPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'

function Placeholder({ title }: { title: string }) {
  return <div className="rounded-2xl border border-slate-200 bg-white p-8"><h2 className="text-2xl font-bold">{title}</h2><p className="mt-2 text-slate-500">Flow backend tersedia. UI detail/wizard akan dilanjutkan modul per modul.</p></div>
}

export default function App() {
  return (
    <Routes>
      <Route path="/welcome" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="master/items" element={<ListPage title="Master Barang" endpoint="/items" columns={itemColumns} />} />
          <Route path="master/customers" element={<ListPage title="Master Konsumen" endpoint="/customers" columns={customerColumns} />} />
          <Route path="master/suppliers" element={<ListPage title="Master Distributor" endpoint="/suppliers" columns={supplierColumns} />} />
          <Route path="master/branches" element={<ListPage title="Master Cabang Toko" endpoint="/branches" columns={branchColumns} />} />
          <Route path="transactions/sales" element={<Placeholder title="Penjualan" />} />
          <Route path="transactions/purchases" element={<Placeholder title="Pembelian" />} />
          <Route path="billing" element={<Placeholder title="Billing & Coin Topup" />} />
          <Route path="reports" element={<Placeholder title="Reports" />} />
          <Route path="settings" element={<Placeholder title="Settings" />} />
        </Route>
      </Route>
      <Route path="*" element={<LandingPage />} />
    </Routes>
  )
}
