# XPARF Refactor Completion Checklist

## FASE 0: PERSIAPAN & SCAFFOLD
- [x] Buat REFACTOR_NOTES.md
- [x] Buat REFACTOR_COMPLETION.md
- [x] Buat solution `xparf` di direktori ini
- [x] Buat folder structure awal:
  ```
  xparf/
  ├── src/
  │   ├── Xparf.Api/
  │   ├── Xparf.Core/
  │   ├── Xparf.Infrastructure/
  │   └── xparf-web/
  ├── tests/
  │   ├── Xparf.Api.Tests/
  │   ├── Xparf.Core.Tests/
  │   └── xparf-e2e/
  ├── database/
  │   └── migrations/
  └── docs/
  ```

## FASE 1: DOMAIN & INFRASTRUCTURE
- [ ] 1.1 Refactor entity skeleton ke nama standard Inggris (`Branch`, `Item`, `Customer`, `Supplier`, dll).
- [ ] 1.2 Buat `AuditableEntity` standard.
- [ ] 1.3 Buat domain identity/tenant: `Company`, `User`, `Role`, `Permission`, `RolePermission`, `UserBranch`.
- [ ] 1.4 Buat domain inventory: `Branch`, `Item`, `BranchItem`, `StockLedger`, `ItemPriceHistory`.
- [ ] 1.5 Buat domain sales: `Sale`, `SaleLine`, `SalePayment`, `SaleComplaint`.
- [ ] 1.6 Buat domain purchase: `Purchase`, `PurchaseLine`, `PurchasePayment`.
- [ ] 1.7 Buat domain billing: `CoinTopup`, `CoinLedger`, `TopupPackage`, `PaymentWebhookLog`, `PlatformSetting`.
- [x] 1.8 Setup awal EF Core DbContext dengan PostgreSQL.
- [ ] 1.9 Finalisasi DbContext, relationship, index, query filter soft delete.
- [x] 1.10 Create initial migration PostgreSQL setelah schema final.
- [x] 1.11 Add seed data: permissions, topup packages, platform settings.

## FASE 2: API LAYER (.NET 10 Web API)
- [x] 2.1 Create `Xparf.Api` project
- [ ] 2.2 Setup Program.cs with:
  - Serilog logging
  - PostgreSQL connection
  - JWT authentication
  - CORS (dev: http://localhost:5173)
  - Swagger/OpenAPI
  - Global exception handling
- [ ] 2.3 Create DTOs/records: Auth, Company, User, Role, Branch, Item, Customer, Supplier, Purchase, Sale, Billing.
- [ ] 2.4 Create service layer untuk business logic.
- [ ] 2.5 Create Controllers:
  - AuthController
  - CompanyController
  - UsersController
  - RolesController
  - BranchesController
  - ItemsController
  - BranchItemsController
  - CustomersController
  - SuppliersController
  - PurchasesController
  - SalesController
  - BillingController
  - PaymentWebhooksController
  - AdminController
- [ ] 2.6 Implement JWT Auth middleware + refresh token.
- [ ] 2.7 Implement permission policy.
- [ ] 2.8 Implement pagination.
- [ ] 2.9 Implement file upload endpoint (Excel import).
- [ ] 2.10 Add FluentValidation validators.
- [ ] 2.11 Add idempotency untuk sale post dan QRIS webhook.

## FASE 3: DATABASE (PostgreSQL)
- [x] 3.1 Create database `xparf` in local PostgreSQL
- [x] 3.2 Run initial migration
- [ ] 3.3 Add indexes:
  - IX_Users_Email (unique)
  - IX_Barang_CabangTokoId_Kode (unique)
  - IX_Barang_TipeBarang
  - IX_Penjualan_CreatedAt
  - IX_Konsumen_CabangTokoId
  - IX_CreditTopup_CompanyId
- [ ] 3.4 Create schema adjustments for new features:
  - Companies table
  - CreditTopups table
  - MonthlyDeductions table
  - Employees table (relation User ↔ Company → Role, CabangToko)
  - Add CompanyId FK to User
  - Add IsOwner bit to User
  - Replace string KodeCabangToko with int CabangTokoId FK
- [ ] 3.5 Create data migration script from old SQL Server
- [ ] 3.6 Create background job for monthly deduction (Quartz.NET)

## FASE 4: REACT FRONTEND
- [ ] 4.1 Initialize Vite + React 18 + TypeScript project
- [ ] 4.2 Install dependencies:
  - tailwindcss, react-router-dom, @tanstack/react-query, zustand
  - axios, react-hook-form, zod
  - recharts (charts)
  - @radix-ui/react-dialog, @radix-ui/react-select (shadcn/ui)
- [ ] 4.3 Setup Tailwind + shadcn/ui component library
- [ ] 4.4 Create auth flow:
  - LoginPage
  - RegisterPage (company owner registers with email)
  - ProtectedRoute component
  - Auth hook + Axios interceptor
- [ ] 4.5 Create layout:
  - MainLayout (sidebar navigation, header, user dropdown)
  - Sidebar: dynamic based on role modules
- [ ] 4.6 Create pages:
  - Dashboard: summary cards, sales chart, stock alerts
  - Master/Barang: DataTable + FormModal + PickPopup
  - Master/Konsumen: DataTable + FormModal
  - Master/Distributor: DataTable + FormModal
  - Master/CabangToko: DataTable + FormModal
  - Master/User: DataTable + FormModal (employees)
  - Transaction/Pembelian: wizard (header step, detil step, konfirmasi)
  - Transaction/Penjualan: wizard (retail/grosir toggle)
  - Transaction/Complain: form + list
  - Credit/Topup: form + list + balance
  - Report: filter form, preview, download PDF
  - Settings: company profile, subscription, employees
- [ ] 4.7 Create reusable components:
  - DataTable (sortable, filterable, paginated, selectable rows)
  - FormModal (generic CRUD dialog with react-hook-form)
  - LookupPick (modal popup for picking entity)
  - AutoComplete (search-as-you-type)
  - StatCard (for dashboard)
  - Breadcrumb
  - ConfirmDialog
- [ ] 4.8 Integrate PDF generation (download from API)

## FASE 5: REPORT SYSTEM (QuestPDF)
- [ ] 5.1 Add QuestPDF package in Api
- [ ] 5.2 Create report templates:
  - InvoicePenjualan
  - InvoicePembelian
  - KartuStok
  - RekapPenjualan
  - TunggakanKonsumen
- [ ] 5.3 Create ReportController returning PDF files
- [ ] 5.4 Migrate old Crystal Report logic to QuestPDF

## FASE 6: GRADUAL MIGRATION + TESTING
- [ ] 6.1 Unit tests for Core domain logic
- [ ] 6.2 Integration tests for repositories (Testcontainers PostgreSQL)
- [ ] 6.3 Integration tests for API endpoints
- [ ] 6.4 E2E tests for critical flows (login → create barang → penjualan)
- [ ] 6.5 Performance test: pagination with large dataset
- [ ] 6.6 Migration script: transfer old SQL Server data to new PostgreSQL
- [ ] 6.7 Run side-by-side: old site runs, new API runs, module-by-module switch
- [ ] 6.8 UAT with stakeholders
- [ ] 6.9 Deploy to staging → production

## FASE 7: NEW FEATURES (COIN TOPUP QRIS & MULTI-USER)
- [ ] 7.1 Company registration flow (email + password).
- [ ] 7.2 Owner invites employees via email.
- [ ] 7.3 Employee management (CRUD, assign role, assign branch).
- [ ] 7.4 Coin topup via QRIS/QR code.
- [ ] 7.5 Payment webhook updates coin balance.
- [ ] 7.6 Coin deduction per sale posted sebesar nilai setting `coin.sale_posted_deduction`.
- [ ] 7.7 CoinLedger history untuk topup, sale deduction, adjustment.
- [ ] 7.8 Super admin setting coin deduction.
- [ ] 7.9 Super admin manual coin adjustment.
- [ ] 7.10 Freeze account jika coin tidak cukup saat transaksi.
- [ ] 7.11 Email notification saat coin rendah.

## STATUS AUDIT FLOW INTI (2026-05-30)

Legend: ✅ endpoint + UI usable, 🟡 partial/basic, ❌ belum ada.

| Flow inti | Endpoint | UI | Status | Catatan |
|---|---:|---:|---|---|
| Auth register/login/refresh/logout | ✅ | 🟡 | 🟡 | Login/register/protected route ada. Forgot/reset/confirm email endpoint ada, UI belum wired ke route utama. |
| Company profile | ✅ | ✅ | ✅ | `/company/me` dan `/settings` update profile. |
| Role/permission | ✅ | 🟡 | 🟡 | Endpoint roles/permissions ada. UI baru assign role user; belum CRUD role/permission matrix. |
| User/employee management | ✅ | ✅ | ✅ | CRUD user, invite, assign role, assign branch ada di `/settings`. Email invite actual sending belum diverifikasi. |
| Master cabang | ✅ | ✅ | ✅ | CRUD via `/master/branches`. |
| Master barang | ✅ | ✅ | ✅ | CRUD via `/master/items`. Branch stock/pricing masih via endpoint branch-items, belum UI master khusus. |
| Master konsumen | ✅ | ✅ | ✅ | CRUD via `/master/customers`. |
| Master distributor/supplier | ✅ | ✅ | ✅ | CRUD via `/master/suppliers`. |
| Branch item / harga / stok per cabang | ✅ | ❌ | 🟡 | Endpoint `branch-items` ada. UI baru dipakai sebagai lookup sales; belum CRUD stock/pricing. |
| Stock ledger & adjustment | ✅ | ❌ | 🟡 | Endpoint ada. UI belum ada. |
| Penjualan retail/grosir | ✅ | 🟡 | 🟡 | Sales wizard ada: draft, add line, payment, post, void. Belum edit draft line/payment detail, receipt/PDF, validation UX lengkap. |
| Pembelian | ✅ | ❌ | 🟡 | Endpoint complete. UI masih placeholder `/transactions/purchases`. |
| Complain | ❌ | ❌ | ❌ | Domain disebut di BRD, endpoint/UI belum ada. |
| Billing coin topup QRIS | ✅ | ✅ | ✅ | Balance, packages, create topup, ledger UI ada di `/billing`. Payment provider real QRIS perlu environment/provider verification. |
| Payment webhook QRIS | ✅ | ❌ | 🟡 | Endpoint ada. No UI needed normally, but webhook signature/idempotency hardening perlu audit. |
| Coin deduction on sale post | ✅ | 🟡 | 🟡 | Backend sale post deduct coin; UI menampilkan `coinDeducted` setelah post. Low coin/freeze UX belum lengkap. |
| Admin company/topup/settings/coin adjustment | ✅ | ❌ | 🟡 | Endpoint admin ada. Super admin UI belum ada. |
| Dashboard summary | ✅ | 🟡 | 🟡 | Summary cards ada. Sales chart/stock alerts visual belum lengkap. |
| Reports preview | ✅ | 🟡 | 🟡 | Sales/purchase/stock preview ada. PDF download belum ada. |
| QuestPDF reports | ❌ | ❌ | ❌ | Package/templates/PDF endpoints belum ada. |
| Data migration old SQL Server → PostgreSQL | ❌ | ❌ | ❌ | Belum ada script migration. |
| Integration/E2E tests | ❌ | ❌ | ❌ | Baru 1 unit test Core. Belum API/UI E2E. |

### Kesimpulan audit
- Flow inti CRUD master, auth dasar, company, employee, billing coin, reports preview, dan sales draft→post sudah tersedia minimal end-to-end.
- Belum semua flow inti selesai. Blocker MVP terbesar: Purchase wizard UI, branch stock/pricing UI, complain module, PDF/QuestPDF, super-admin UI, migration script, integration/E2E tests.
- Endpoint lebih lengkap dari UI. UI masih basic dan beberapa form belum memakai `react-hook-form` + `zod` secara konsisten.
- Aplikasi belum production-ready sampai PDF, purchase, migration, dan test coverage selesai.

### Prioritas next chunk
1. Purchase wizard UI `/transactions/purchases` karena endpoint sudah complete dan mirip sales.
2. Branch item stock/pricing UI + stock adjustment UI.
3. QuestPDF PDF endpoints + frontend download buttons.
4. Super admin UI untuk topup packages, platform settings, coin adjustment, freeze/unfreeze.
5. E2E test login → create barang → create branch item → sale post.

## TIMELINE (estimated)
| Phase | Duration | Dependencies |
|---|---|---|
| F0 (Scaffold) | 1 week | None |
| F1 (Core+Infra) | 2 weeks | F0 |
| F2 (API) | 4 weeks | F1 |
| F3 (DB) | 2 weeks (parallel F2) | F1 |
| F4 (React) | 6 weeks (parallel F2) | none |
| F5 (Reports) | 2 weeks | F2 |
| F6 (Migration+Testing) | 4 weeks | F2+F4 |
| F7 (New Features) | 3 weeks | F4 |
  
Total: ~12-14 weeks untuk MVP. Backend endpoint diselesaikan dulu sebelum frontend.

Detail task backend ada di `XPARF_BACKEND_TASKS.md`.
EOF