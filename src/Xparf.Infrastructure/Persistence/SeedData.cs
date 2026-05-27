using Xparf.Core.Entities;

namespace Xparf.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Permission[] Permissions =
    [
        new() { Id = 1, Code = "company.read", Name = "Lihat Company", Module = "Company" },
        new() { Id = 2, Code = "company.update", Name = "Ubah Company", Module = "Company" },
        new() { Id = 3, Code = "users.manage", Name = "Kelola User", Module = "Users" },
        new() { Id = 4, Code = "roles.manage", Name = "Kelola Role", Module = "Roles" },
        new() { Id = 5, Code = "branches.manage", Name = "Kelola Branch", Module = "Branches" },
        new() { Id = 6, Code = "items.manage", Name = "Kelola Item", Module = "Items" },
        new() { Id = 7, Code = "inventory.read", Name = "Lihat Inventory", Module = "Inventory" },
        new() { Id = 8, Code = "inventory.adjust", Name = "Adjustment Stok", Module = "Inventory" },
        new() { Id = 9, Code = "customers.manage", Name = "Kelola Customer", Module = "Customers" },
        new() { Id = 10, Code = "suppliers.manage", Name = "Kelola Supplier", Module = "Suppliers" },
        new() { Id = 11, Code = "purchases.manage", Name = "Kelola Purchase", Module = "Purchases" },
        new() { Id = 12, Code = "purchases.post", Name = "Posting Purchase", Module = "Purchases" },
        new() { Id = 13, Code = "sales.manage", Name = "Kelola Sale", Module = "Sales" },
        new() { Id = 14, Code = "sales.post", Name = "Posting Sale", Module = "Sales" },
        new() { Id = 15, Code = "sales.void", Name = "Void Sale", Module = "Sales" },
        new() { Id = 16, Code = "billing.read", Name = "Lihat Billing", Module = "Billing" },
        new() { Id = 17, Code = "billing.topup", Name = "Topup Coin", Module = "Billing" },
        new() { Id = 18, Code = "reports.read", Name = "Lihat Report", Module = "Reports" },
        new() { Id = 19, Code = "admin.manage", Name = "Kelola Super Admin", Module = "Admin" }
    ];

    public static readonly PlatformSetting[] PlatformSettings =
    [
        new() { Id = 1, Key = "coin.sale_posted_deduction", Value = "1", DataType = "decimal", Description = "Jumlah coin yang dipotong setiap sale posted." },
        new() { Id = 2, Key = "coin.enable_deduction", Value = "true", DataType = "bool", Description = "Aktif/nonaktif potong coin pada sale posted." },
        new() { Id = 3, Key = "coin.minimum_balance_to_sell", Value = "1", DataType = "decimal", Description = "Minimal coin balance agar sale bisa diposting." }
    ];

    public static readonly TopupPackage[] TopupPackages =
    [
        new() { Id = 1, Name = "Starter", MoneyAmount = 50000m, CoinAmount = 500m, SortOrder = 1 },
        new() { Id = 2, Name = "Growth", MoneyAmount = 100000m, CoinAmount = 1100m, SortOrder = 2 },
        new() { Id = 3, Name = "Business", MoneyAmount = 250000m, CoinAmount = 3000m, SortOrder = 3 }
    ];
}
