using Xparf.Core.Entities;

namespace Xparf.Infrastructure.Persistence;

public static class SeedData
{
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Permission[] Permissions =
    [
        new() { Id = 1, Code = "company.read", Name = "Lihat Company", Module = "Company", CreatedAt = SeedCreatedAt },
        new() { Id = 2, Code = "company.update", Name = "Ubah Company", Module = "Company", CreatedAt = SeedCreatedAt },
        new() { Id = 3, Code = "users.manage", Name = "Kelola User", Module = "Users", CreatedAt = SeedCreatedAt },
        new() { Id = 4, Code = "roles.manage", Name = "Kelola Role", Module = "Roles", CreatedAt = SeedCreatedAt },
        new() { Id = 5, Code = "branches.manage", Name = "Kelola Branch", Module = "Branches", CreatedAt = SeedCreatedAt },
        new() { Id = 6, Code = "items.manage", Name = "Kelola Item", Module = "Items", CreatedAt = SeedCreatedAt },
        new() { Id = 7, Code = "inventory.read", Name = "Lihat Inventory", Module = "Inventory", CreatedAt = SeedCreatedAt },
        new() { Id = 8, Code = "inventory.adjust", Name = "Adjustment Stok", Module = "Inventory", CreatedAt = SeedCreatedAt },
        new() { Id = 9, Code = "customers.manage", Name = "Kelola Customer", Module = "Customers", CreatedAt = SeedCreatedAt },
        new() { Id = 10, Code = "suppliers.manage", Name = "Kelola Supplier", Module = "Suppliers", CreatedAt = SeedCreatedAt },
        new() { Id = 11, Code = "purchases.manage", Name = "Kelola Purchase", Module = "Purchases", CreatedAt = SeedCreatedAt },
        new() { Id = 12, Code = "purchases.post", Name = "Posting Purchase", Module = "Purchases", CreatedAt = SeedCreatedAt },
        new() { Id = 13, Code = "sales.manage", Name = "Kelola Sale", Module = "Sales", CreatedAt = SeedCreatedAt },
        new() { Id = 14, Code = "sales.post", Name = "Posting Sale", Module = "Sales", CreatedAt = SeedCreatedAt },
        new() { Id = 15, Code = "sales.void", Name = "Void Sale", Module = "Sales", CreatedAt = SeedCreatedAt },
        new() { Id = 16, Code = "billing.read", Name = "Lihat Billing", Module = "Billing", CreatedAt = SeedCreatedAt },
        new() { Id = 17, Code = "billing.topup", Name = "Topup Coin", Module = "Billing", CreatedAt = SeedCreatedAt },
        new() { Id = 18, Code = "reports.read", Name = "Lihat Report", Module = "Reports", CreatedAt = SeedCreatedAt },
        new() { Id = 19, Code = "admin.manage", Name = "Kelola Super Admin", Module = "Admin", CreatedAt = SeedCreatedAt }
    ];

    public static readonly PlatformSetting[] PlatformSettings =
    [
        new() { Id = 1, Key = "coin.sale_posted_deduction", Value = "1", DataType = "decimal", Description = "Jumlah coin yang dipotong setiap sale posted.", CreatedAt = SeedCreatedAt },
        new() { Id = 2, Key = "coin.enable_deduction", Value = "true", DataType = "bool", Description = "Aktif/nonaktif potong coin pada sale posted.", CreatedAt = SeedCreatedAt },
        new() { Id = 3, Key = "coin.minimum_balance_to_sell", Value = "1", DataType = "decimal", Description = "Minimal coin balance agar sale bisa diposting.", CreatedAt = SeedCreatedAt }
    ];

    public static readonly TopupPackage[] TopupPackages =
    [
        new() { Id = 1, Name = "Starter", MoneyAmount = 50000m, CoinAmount = 500m, SortOrder = 1, CreatedAt = SeedCreatedAt },
        new() { Id = 2, Name = "Growth", MoneyAmount = 100000m, CoinAmount = 1100m, SortOrder = 2, CreatedAt = SeedCreatedAt },
        new() { Id = 3, Name = "Business", MoneyAmount = 250000m, CoinAmount = 3000m, SortOrder = 3, CreatedAt = SeedCreatedAt }
    ];
}
