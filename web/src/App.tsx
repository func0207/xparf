import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { MainLayout } from './layouts/MainLayout'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { PageStub } from './pages/PageStub'
import { RegisterPage } from './pages/RegisterPage'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="master/items" element={<PageStub title="Master Barang" description="CRUD barang, stok, harga retail/grosir." />} />
          <Route path="transactions/sales" element={<PageStub title="Penjualan" description="Wizard penjualan retail/grosir dan coin deduction." />} />
          <Route path="billing" element={<PageStub title="Billing" description="Topup QRIS, package coin, ledger." />} />
          <Route path="reports" element={<PageStub title="Reports" description="Preview dan download PDF dari API." />} />
          <Route path="settings" element={<PageStub title="Settings" description="Company profile, subscription, employees." />} />
        </Route>
      </Route>
    </Routes>
  )
}
