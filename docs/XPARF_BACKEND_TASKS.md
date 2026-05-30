# XPARF Backend Task List & Checkpoint

Backend dikerjakan dulu sampai endpoint stabil. Frontend setelah backend selesai.

## Prinsip
- Nama domain/schema pakai Inggris standard.
- Catatan bisnis pakai Indonesia.
- DB target PostgreSQL `xparf`.
- Backend .NET 10 Web API.
- Multi-tenant: data bisnis wajib punya `CompanyId`.
- Stok wajib lewat `StockLedger`.
- Coin/topup wajib lewat `CoinLedger`.
- Operasi penting wajib audit.
- Migrasi DB ditunda sampai model final.

## Checkpoint Selesai
- [x] Analisa struktur aplikasi lama Web Forms.
- [x] Buat `REFACTOR_NOTES.md`.
- [x] Buat `REFACTOR_COMPLETION.md`.
- [x] Buat folder solution `xparf`.
- [x] Buat `Xparf.Core`.
- [x] Buat `Xparf.Infrastructure`.
- [x] Buat `Xparf.Api`.
- [x] Buat `Xparf.Core.Tests`.
- [x] Tambah reference antar project.
- [x] Tambah EF Core + Npgsql + Serilog awal.
- [x] Tambah endpoint `GET /api/health`.
- [x] Build backend sukses.
- [x] Refactor entity skeleton ke nama standard Inggris.
- [x] Buat domain entity utama.
- [x] Update `XparfDbContext` ke schema standard.
- [x] Build sukses setelah refactor domain.
- [x] Tambah current user context untuk audit.
- [x] Tambah global soft delete filter.
- [x] Tambah concurrency token.
- [x] Tambah seed permissions/platform settings/topup packages.
- [x] Hapus template WeatherForecast bawaan.
- [x] Build sukses setelah audit + seed.
- [x] Tambah auth foundation JWT + refresh token.
- [x] Tambah global exception middleware.
- [x] Tambah FluentValidation auth request.
- [x] Build sukses setelah auth foundation.
- [x] Tambah endpoint company profile dan coin balance.
- [x] Build sukses setelah company endpoint.
- [x] Tambah endpoint roles dan permissions.
- [x] Build sukses setelah roles/permissions endpoint.
- [x] Tambah endpoint users/employees.
- [x] Build sukses setelah users endpoint.
- [x] Tambah CRUD endpoint branches.
- [x] Build sukses setelah branches endpoint.
- [x] Siapkan local dev env: .NET 10 SDK, PostgreSQL 17, DB `xparf`, API LAN health check.
- [x] Analisa posisi terakhir dari `origin/main`: next backend chunk adalah master/inventory.
- [x] Build dan test sukses sebelum lanjut implementasi master/inventory.

## Analisa Terakhir (2026-05-30)
- `origin/main` terakhir: `36c1076 Add branches endpoints`.
- Local `main` ahead 1 commit: `9730b71 docs: record local development environment`.
- Endpoint selesai sampai sekarang: health, auth register/login/refresh, company profile/coin balance, roles/permissions, users/employees, branches.
- Endpoint berikutnya sesuai urutan backend: master/inventory.
- `CRUD /api/items` selesai: kontrak, validator, service, controller, DI registration.
- Build command sukses: `dotnet build xparf.slnx`.
- Test command sukses: `dotnet test xparf.slnx`.
- Warning tersisa: `NU1903 Microsoft.Build.Tasks.Core 17.7.2` high severity advisory via transitive dependency.
- Migration EF belum dibuat; tetap tunda sampai model/endpoint master-inventory stabil.

## Langkah Berikutnya
1. Tambah kontrak, validator, service, controller untuk `CRUD /api/items`.
2. Tambah kontrak, validator, service, controller untuk `CRUD /api/customers`.
3. Tambah kontrak, validator, service, controller untuk `CRUD /api/suppliers`.
4. Tambah `CRUD /api/branch-items` setelah item dan branch stabil.
5. Tambah `GET /api/stock-ledgers` untuk audit pergerakan stok.
6. Tambah `POST /api/stock-adjustments` wajib update `BranchItem.QuantityOnHand` dan insert `StockLedger` dalam 1 transaksi.
7. Jalankan `dotnet build xparf.slnx` dan `dotnet test xparf.slnx` tiap chunk.
8. Update checklist docs dan commit tiap endpoint chunk.

## Mapping Lama ke Baru
| Lama | Baru |
|---|---|
| CabangToko | Branch |
| Barang | Item |
| Barang per cabang | BranchItem |
| Konsumen | Customer |
| Distributor | Supplier |
| Pembelian | Purchase |
| DetilPembelianBarang | PurchaseLine |
| Penjualan | Sale |
| DetilPenjualan | SaleLine |
| CicilanPenjualan | SalePayment |
| CicilanPembelian | PurchasePayment |
| TunggakanKonsumen | Receivable |
| TunggakanPembelian | Payable |
| KartuBarang | StockLedger |
| PerubahanHargaJual | ItemPriceHistory |
| ComplainPenjualan | SaleComplaint |
| Module | Permission/MenuItem |

## Flow Lama Dipertahankan
- [ ] Login user.
- [ ] Role-based access.
- [ ] Multi cabang.
- [ ] Master item/customer/supplier.
- [ ] Pembelian + detail + pembayaran.
- [ ] Penjualan retail/grosir + detail + pembayaran.
- [ ] Tunggakan/piutang.
- [ ] Upload Excel item/pembelian/stok opname.
- [ ] Update harga jual.
- [ ] Laporan stok/pembelian/penjualan.
- [ ] Komplain/retur penjualan.

## Flow Baru
### Multi Tenant
- [ ] Owner daftar email.
- [ ] Buat `Company`, owner `User`, role default, branch default.
- [ ] Owner tambah branch dan employee.

### Employee + Role + Branch
- [ ] Employee punya role.
- [ ] Employee bisa dibatasi branch.
- [ ] Permission granular: `sales.create`, `items.read`, `billing.topup`.

### POS + Coin Deduction
- [ ] Sale draft.
- [ ] Sale post/checkout.
- [ ] Validasi stok cukup.
- [ ] Validasi coin cukup.
- [ ] Saat posted: deduct stock, buat `StockLedger`, deduct coin, buat `CoinLedger`.
- [ ] Semua dalam 1 DB transaction.

### Topup QRIS
- [ ] Owner pilih paket coin.
- [ ] Buat `CoinTopup` pending.
- [ ] Generate QR provider.
- [ ] Webhook QRIS masuk.
- [ ] Validasi signature/amount/reference.
- [ ] Update paid, tambah `Company.CoinBalance`, buat `CoinLedger`.
- [ ] Simpan `PaymentWebhookLog`.

### Super Admin
- [ ] Login super admin.
- [ ] Lihat company.
- [ ] Freeze/unfreeze company.
- [ ] Setting `coin.sale_posted_deduction`.
- [ ] Manage topup package.
- [ ] Manual coin adjustment.
- [ ] Lihat webhook log.

## Domain Model Target
### Audit
- [x] `AuditableEntity`: Id, CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, DeletedAt, DeletedByUserId, IsDeleted, RowVersion.
- [x] Global query filter soft delete.
- [x] Concurrency token.

### Identity/Tenant
- [x] `Company`
- [x] `User`
- [x] `Role`
- [x] `Permission`
- [x] `RolePermission`
- [x] `UserBranch`

### Branch/Inventory
- [x] `Branch`
- [x] `Item`
- [x] `BranchItem`
- [x] `StockLedger`
- [x] `ItemPriceHistory`

### Master
- [x] `Customer`
- [x] `Supplier`

### Purchase
- [x] `Purchase`
- [x] `PurchaseLine`
- [x] `PurchasePayment`

### Sale/POS
- [x] `Sale`
- [x] `SaleLine`
- [x] `SalePayment`
- [x] `SaleComplaint`

### Coin/Billing
- [x] `CoinTopup`
- [x] `CoinLedger`
- [x] `TopupPackage`
- [x] `PaymentWebhookLog`
- [x] `PlatformSetting`

## Endpoint Target
### Health
- [x] `GET /api/health`

### Auth
- [x] `POST /api/auth/register-company`
- [x] `POST /api/auth/login`
- [x] `POST /api/auth/refresh`
- [ ] `POST /api/auth/logout`
- [ ] `POST /api/auth/confirm-email`
- [ ] `POST /api/auth/forgot-password`
- [ ] `POST /api/auth/reset-password`

### Company
- [x] `GET /api/company/me`
- [x] `PUT /api/company/me`
- [x] `GET /api/company/me/coin-balance`

### Users/Roles
- [x] CRUD `/api/users`
- [x] `POST /api/users/invite`
- [x] `PUT /api/users/{id}/roles`
- [x] `PUT /api/users/{id}/branches`
- [x] CRUD `/api/roles`
- [x] `GET /api/permissions`
- [x] `PUT /api/roles/{id}/permissions`

### Master/Inventory
- [x] CRUD `/api/branches`
- [x] CRUD `/api/items`
- [ ] CRUD `/api/branch-items`
- [ ] `GET /api/stock-ledgers`
- [ ] `POST /api/stock-adjustments`
- [ ] CRUD `/api/customers`
- [ ] CRUD `/api/suppliers`

### Purchase
- [ ] CRUD `/api/purchases`
- [ ] Line CRUD `/api/purchases/{id}/lines`
- [ ] `POST /api/purchases/{id}/post`
- [ ] `POST /api/purchases/{id}/payments`
- [ ] `POST /api/purchases/{id}/cancel`

### Sale/POS
- [ ] CRUD `/api/sales`
- [ ] Line CRUD `/api/sales/{id}/lines`
- [ ] `POST /api/sales/{id}/post`
- [ ] `POST /api/sales/{id}/payments`
- [ ] `POST /api/sales/{id}/void`
- [ ] `GET /api/sales/{id}/receipt`

### Billing/Coin
- [ ] `GET /api/billing/coin-balance`
- [ ] `GET /api/billing/coin-ledgers`
- [ ] `GET /api/billing/topup-packages`
- [ ] `POST /api/billing/topups`
- [ ] `GET /api/billing/topups/{id}`
- [ ] `POST /api/payment-webhooks/qris`

### Super Admin
- [ ] `GET /api/admin/companies`
- [ ] `PUT /api/admin/companies/{id}/freeze`
- [ ] `PUT /api/admin/companies/{id}/unfreeze`
- [ ] CRUD `/api/admin/topup-packages`
- [ ] CRUD `/api/admin/platform-settings`
- [ ] `GET /api/admin/coin-ledgers`
- [ ] `POST /api/admin/coin-adjustments`
- [ ] `GET /api/admin/payment-webhook-logs`

## Urutan Backend
1. Refactor entity skeleton ke nama standard.
2. Finalisasi domain entity.
3. Finalisasi DbContext, index, relationship.
4. Buat repository/unit-of-work atau service langsung via DbContext.
5. Buat auth JWT.
6. Buat endpoint tenant/company/user/role.
7. Buat endpoint branch/item/inventory.
8. Buat endpoint sale + coin deduction.
9. Buat endpoint topup QR + webhook.
10. Buat endpoint purchase.
11. Buat endpoint report minimal.
12. Test dan migration PostgreSQL.
