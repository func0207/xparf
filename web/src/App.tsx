import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { MainLayout } from './layouts/MainLayout'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { ReportsPage } from './pages/ReportsPage'
import { SettingsPage } from './pages/SettingsPage'
import { SalesWizardPage } from './pages/SalesWizardPage'
import { PurchaseWizardPage } from './pages/PurchaseWizardPage'
import { InventoryPage } from './pages/InventoryPage'
import { BillingPage } from './pages/BillingPage'
import { AdminPage } from './pages/AdminPage'
import { ComplaintsPage } from './pages/ComplaintsPage'
import { ReceiptPage } from './pages/ReceiptPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { LandingPage } from './pages/LandingPage'
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
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="master/items" element={<CrudListPage title="Master Barang" endpoint="/items" columns={itemColumns} fields={itemFields} createDefaults={itemDefaults} updateShape={removeItemCreateOnly} sortDefault="sku" />} />
          <Route path="master/customers" element={<CrudListPage title="Master Konsumen" endpoint="/customers" columns={customerColumns} fields={customerFields} createDefaults={customerDefaults} updateShape={removeCodeAndId} />} />
          <Route path="master/suppliers" element={<CrudListPage title="Master Distributor" endpoint="/suppliers" columns={supplierColumns} fields={supplierFields} createDefaults={supplierDefaults} updateShape={removeCodeAndId} />} />
          <Route path="master/branches" element={<CrudListPage title="Master Cabang Toko" endpoint="/branches" columns={branchColumns} fields={branchFields} createDefaults={branchDefaults} updateShape={removeCodeAndId} />} />
          <Route path="inventory" element={<InventoryPage />} />
          <Route path="transactions/sales" element={<SalesWizardPage />} />
          <Route path="transactions/purchases" element={<PurchaseWizardPage />} />
          <Route path="transactions/complaints" element={<ComplaintsPage />} />
          <Route path="sales/:id/receipt" element={<ReceiptPage />} />
          <Route path="billing" element={<BillingPage />} />
          <Route path="admin" element={<AdminPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
      </Route>
    </Routes>
  )
}
