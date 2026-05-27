using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;

namespace Xparf.Infrastructure.Persistence;

public sealed class XparfDbContext(
    DbContextOptions<XparfDbContext> options,
    ICurrentUserContext currentUserContext) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<BranchItem> BranchItems => Set<BranchItem>();
    public DbSet<StockLedger> StockLedgers => Set<StockLedger>();
    public DbSet<ItemPriceHistory> ItemPriceHistories => Set<ItemPriceHistory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
    public DbSet<PurchasePayment> PurchasePayments => Set<PurchasePayment>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<SaleComplaint> SaleComplaints => Set<SaleComplaint>();
    public DbSet<CoinTopup> CoinTopups => Set<CoinTopup>();
    public DbSet<CoinLedger> CoinLedgers => Set<CoinLedger>();
    public DbSet<TopupPackage> TopupPackages => Set<TopupPackage>();
    public DbSet<PaymentWebhookLog> PaymentWebhookLogs => Set<PaymentWebhookLog>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentity(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureMasterData(modelBuilder);
        ConfigurePurchase(modelBuilder);
        ConfigureSale(modelBuilder);
        ConfigureBilling(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureSeedData(modelBuilder);
        ConfigureAuditableEntities(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditValues();
        return base.SaveChanges();
    }

    private void ApplyAuditValues()
    {
        var now = DateTime.UtcNow;
        var userId = currentUserContext.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedByUserId = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedByUserId = userId;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedByUserId = userId;
            }
        }
    }

    private static void ConfigureAuditableEntities(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            entityType.SetQueryFilter(lambda);
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.RowVersion))
                .IsRowVersion();
        }
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.CoinBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasOne(x => x.Company).WithMany(x => x.Users).HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<UserBranch>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.BranchId });
            entity.HasOne(x => x.User).WithMany(x => x.UserBranches).HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.Branch).WithMany(x => x.UserBranches).HasForeignKey(x => x.BranchId);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Company).WithMany(x => x.Branches).HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Sku }).IsUnique();
            entity.HasIndex(x => x.ItemType);
            entity.Property(x => x.Sku).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Barcode).HasMaxLength(120);
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.BaseUnit).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<BranchItem>(entity =>
        {
            entity.HasIndex(x => new { x.BranchId, x.ItemId }).IsUnique();
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.Property(x => x.MinimumStock).HasPrecision(18, 3);
            entity.Property(x => x.AverageCost).HasPrecision(18, 2);
            entity.Property(x => x.LastCost).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);
            entity.Property(x => x.WholesalePrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.Branch).WithMany(x => x.BranchItems).HasForeignKey(x => x.BranchId);
            entity.HasOne(x => x.Item).WithMany(x => x.BranchItems).HasForeignKey(x => x.ItemId);
        });

        modelBuilder.Entity<StockLedger>(entity =>
        {
            entity.HasIndex(x => new { x.BranchId, x.ItemId, x.CreatedAt });
            entity.Property(x => x.ReferenceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.QuantityIn).HasPrecision(18, 3);
            entity.Property(x => x.QuantityOut).HasPrecision(18, 3);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ItemPriceHistory>(entity =>
        {
            entity.HasIndex(x => new { x.BranchId, x.ItemId, x.CreatedAt });
            entity.Property(x => x.OldSellingPrice).HasPrecision(18, 2);
            entity.Property(x => x.NewSellingPrice).HasPrecision(18, 2);
            entity.Property(x => x.OldWholesalePrice).HasPrecision(18, 2);
            entity.Property(x => x.NewWholesalePrice).HasPrecision(18, 2);
        });
    }

    private static void ConfigureMasterData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.CreditLimit).HasPrecision(18, 2);
            entity.Property(x => x.CurrentDebt).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
        });
    }

    private static void ConfigurePurchase(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.PurchaseNumber }).IsUnique();
            entity.Property(x => x.PurchaseNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
            entity.HasMany(x => x.Lines).WithOne(x => x.Purchase).HasForeignKey(x => x.PurchaseId);
            entity.HasMany(x => x.Payments).WithOne(x => x.Purchase).HasForeignKey(x => x.PurchaseId);
        });

        modelBuilder.Entity<PurchaseLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PurchasePayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });
    }

    private static void ConfigureSale(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.SaleNumber }).IsUnique();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.Property(x => x.SaleNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.ChangeAmount).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(x => x.CoinDeducted).HasPrecision(18, 2);
            entity.HasMany(x => x.Lines).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId);
            entity.HasMany(x => x.Payments).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId);
        });

        modelBuilder.Entity<SaleLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SalePayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });
    }

    private static void ConfigureBilling(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoinTopup>(entity =>
        {
            entity.HasIndex(x => x.TopupNumber).IsUnique();
            entity.HasIndex(x => x.ProviderReference).IsUnique();
            entity.Property(x => x.MoneyAmount).HasPrecision(18, 2);
            entity.Property(x => x.CoinAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CoinLedger>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.CreatedAt });
            entity.Property(x => x.ReferenceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.CoinIn).HasPrecision(18, 2);
            entity.Property(x => x.CoinOut).HasPrecision(18, 2);
            entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        });

        modelBuilder.Entity<TopupPackage>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MoneyAmount).HasPrecision(18, 2);
            entity.Property(x => x.CoinAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PaymentWebhookLog>(entity =>
        {
            entity.HasIndex(x => new { x.Provider, x.Reference });
            entity.Property(x => x.Provider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Reference).HasMaxLength(160).IsRequired();
        });

        modelBuilder.Entity<PlatformSetting>(entity =>
        {
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Value).IsRequired();
            entity.Property(x => x.DataType).HasMaxLength(40).IsRequired();
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.CreatedAt });
            entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
        });
    }

    private static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(SeedData.Permissions);
        modelBuilder.Entity<PlatformSetting>().HasData(SeedData.PlatformSettings);
        modelBuilder.Entity<TopupPackage>().HasData(SeedData.TopupPackages);
    }
}
