# GREAT.ERP.Website → XPARF Refactor Notes

## Existing Tech Stack
- .NET Framework 4.0 Web Forms
- Entity Framework 4.0 (EDMX), SQL Server
- Crystal Reports / RDLC
- jQuery, Forms Auth
- Web.config hardcoded config

## Arch Pattern
- Controller/ → class library (not MVC)
- View/ → .aspx (Form/View/Pick), code-behind .cs
- Context/ → .edmx (2 models)
- Helper/ → DTOs
- Report/ → .rpt files

## Modules (from Controllers)
| Controller | Area |
|---|---|
| LoginController | Auth, forgot password |
| UserController | User CRUD |
| RoleController | Role |
| CabangTokoController | Branch |
| BarangController | Product (CRUD, paging, filter, stock, harga) |
| KonsumenController | Customer |
| DistributorController | Supplier |
| PembelianController | Purchase (header+detil+cicilan) |
| PenjualanController | Sales (retail+grosir+detil+cicilan) |
| ComplainPenjualanController | Sales complaint |
| KartuBarangController | Price history |
| PerubahanHargaJualController | Sales price change |
| ModuleController | Menu/role access |
| MasterController | General master data |
| GraphicFactoryController | Sales charts (JSON) |

## Entities (from Controllers + DTOs)
- User (ID, Username, Password, Email, Role_id, CabangToko_id)
- Role (ID, Nama)
- CabangToko (ID, Nama, Kode)
- Barang (ID, Kode, Nama, Keterangan, TipeBarang, JumlahTotal, Satuan, HargaRata, HargaTerendah, HargaTertinggi, KodeCabangToko, isDiscontinue, StokMinimum, UpdateBy, UpdateDate, UpdateTerminal)
- Konsumen (ID, Nama, KodeCabangToko)
- Distributor (ID, Nama, KodeCabangToko)
- Pembelian (ID, UserEntry_id, ...)
- DetilPembelianBarang (Pembelian_id, Barang_id, NamaBarang, Satuan, Harga, CreateDate)
- Penjualan (ID, ...)
- DetilPenjualan (ID, ...)
- ComplainPenjualan (ID, ...)
- PerubahanHargaJual (ID, Barang_id, HargaBaru, CreateDate)
- CicilanPembelian, CicilanPenjualan
- TunggakanKonsumen, TunggakanPembelian
- Module (ID, Nama, ...)

## Workflow
1. Login → auth → session → Home
2. Admin manage CabangToko, Role, User
3. Master Barang per cabang (tipe: PARFUM CAIR, BOTOL KOSONG, dll), filter stok kritis/discontinue
4. Konsumen & Distributor per cabang
5. Pembelian (header+detil, upload Excel, automatic stock update)
6. Penjualan (retail/grosir, mixing botol, cicilan, tunggakan)
7. Harga: calc persentase perubahan harga beli, mass update
8. Report: Crystal (PDF)
9. Module access per role

## Kebutuhan Baru
- Registrasi multi-user via email untuk owner company.
- Owner bisa mendaftarkan karyawan dengan role dan akses branch.
- Platform POS parfum multi-tenant.
- Topup coin memakai QRIS/QR code.
- Coin balance otomatis bertambah setelah webhook payment valid.
- Setiap transaksi penjualan posted akan memotong coin sebesar X.
- Nilai X bisa disetting dari halaman super admin lewat `PlatformSetting` key `coin.sale_posted_deduction`.
- Semua perubahan coin wajib masuk `CoinLedger`.
- Super admin bisa manage company, setting platform, paket topup, manual adjustment coin, freeze/unfreeze company.

## Target Stack
- Backend: .NET 10 Web API
- ORM: EF Core 10 with PostgreSQL (Npgsql)
- Auth: JWT with refresh tokens
- Frontend: React 18 + TypeScript + Vite + Tailwind CSS + React Query + Zustand
- Report: QuestPDF (server-side PDF generation)
- Database: PostgreSQL 16

## Standardisasi Nama Domain
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

## Skema Baru Utama
- `Company`: tenant/platform customer.
- `User`: login account.
- `Role`, `Permission`, `RolePermission`, `UserBranch`: akses karyawan.
- `Branch`: cabang/toko.
- `Item`: master barang company-wide.
- `BranchItem`: stok dan harga item per branch.
- `StockLedger`: sumber kebenaran pergerakan stok.
- `Customer`, `Supplier`: master pelanggan dan pemasok.
- `Purchase`, `PurchaseLine`, `PurchasePayment`: pembelian.
- `Sale`, `SaleLine`, `SalePayment`: POS/penjualan.
- `CoinTopup`: request topup QR.
- `CoinLedger`: history coin masuk/keluar.
- `TopupPackage`: paket coin.
- `PaymentWebhookLog`: log webhook provider QRIS.
- `PlatformSetting`: setting super admin, termasuk `coin.sale_posted_deduction`.

## Perubahan Penting dari Skema Lama
- Ganti `KodeCabangToko` string menjadi `BranchId` FK.
- Semua data bisnis punya `CompanyId`.
- Pisahkan `Item` dan `BranchItem` agar stok/harga per cabang rapi.
- Semua mutasi stok lewat `StockLedger`, tidak update stok tanpa ledger.
- Semua mutasi coin lewat `CoinLedger`, tidak update saldo tanpa ledger.
- Tambah FK proper antar entity.
- Tambah enum: `ItemType`, `SaleStatus`, `PaymentStatus`, `StockMovementType`, `CoinTransactionType`, `TopupStatus`.
- Tambah index untuk query POS dan report.
- Tambah soft delete.
- Tambah audit standard: CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, DeletedAt, DeletedByUserId.
- Gunakan auto-increment bigint IDs.
- Tambah idempotency untuk checkout sale dan payment webhook.

## Migration Strategy
- Reverse engineer existing SQL Server schema to EF Core
- Then map to PostgreSQL with adjusted data types
- Use EF Core migrations
- One-time script to migrate data from SQL Server to PostgreSQL

## Deployment
- Docker Compose: API + React (nginx) + PostgreSQL
- CI/CD: GitHub Actions → push to registry → deploy

## Risk & Mitigations
- Existing stored procedures: rewrite to LINQ
- Crystal Reports: replace with QuestPDF templates (incremental)
- Data type differences (SQL Server → PostgreSQL): test edge cases
- Large code-behind files: extract separate services/use cases
- User migration: password hash (old Forms Auth → IdentityPasswordHasher)

## Catatan Developer
- Backend endpoint dikerjakan sampai stabil dulu, frontend belakangan.
- Gunakan FluentValidation untuk request validation.
- Global exception middleware → ProblemDetails.
- Pagination pakai cursor/keyset untuk data besar.
- File upload: local disk untuk dev, S3-compatible untuk prod.
- Logging: Serilog.
- Background jobs: Quartz.NET jika nanti ada proses terjadwal.
- Tests: xUnit + Testcontainers + Moq.
- Lihat `XPARF_BACKEND_TASKS.md` untuk task detail dan checkpoint.
EOF