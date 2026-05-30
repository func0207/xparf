import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { MainLayout } from './layouts/MainLayout'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { PageStub } from './pages/PageStub'
import { RegisterPage } from './pages/RegisterPage'
import { ReportsPage } from './pages/ReportsPage'
import { BillingPage } from './pages/BillingPage'
import { CrudListPage } from './pages/CrudListPage'
import { branchColumns, customerColumns, itemColumns, supplierColumns } from './pages/ListPage'
import {
  branchDefaults,
  branchFields,
  customerDefaults,
  customerFields,
  itemDefaults,
  itemFields,
  removeCodeAndId,
  removeItemCreateOnly,
  supplierDefaults,
  supplierFields,
} from './pages/master-forms'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="master/items" element={<CrudListPage title="Master Barang" endpoint="/items" columns={itemColumns} fields={itemFields} createDefaults={itemDefaults} updateShape={removeItemCreateOnly} sortDefault="sku" />} />
          <Route path="master/customers" element={<CrudListPage title="Master Konsumen" endpoint="/customers" columns={customerColumns} fields={customerFields} createDefaults={customerDefaults} updateShape={removeCodeAndId} />} />
          <Route path="master/suppliers" element={<CrudListPage title="Master Distributor" endpoint="/suppliers" columns={supplierColumns} fields={supplierFields} createDefaults={supplierDefaults} updateShape={removeCodeAndId} />} />
          <Route path="master/branches" element={<CrudListPage title="Master Cabang Toko" endpoint="/branches" columns={branchColumns} fields={branchFields} createDefaults={branchDefaults} updateShape={removeCodeAndId} />} />
          <Route path="transactions/sales" element={<PageStub title="Penjualan" description="Wizard penjualan retail/grosir dan coin deduction." />} />
          <Route path="transactions/purchases" element={<PageStub title="Pembelian" description="Wizard pembelian header, detail, konfirmasi." />} />
          <Route path="billing" element={<BillingPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="settings" element={<PageStub title="Settings" description="Company profile, subscription, employees." />} />
        </Route>
      </Route>
    </Routes>
  )
}
