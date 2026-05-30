using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Admin;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IAdminService
{
    Task<IReadOnlyList<AdminCompanyResponse>> GetCompaniesAsync(CancellationToken cancellationToken);
    Task<AdminCompanyResponse> FreezeCompanyAsync(long id, CancellationToken cancellationToken);
    Task<AdminCompanyResponse> UnfreezeCompanyAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminTopupPackageResponse>> GetTopupPackagesAsync(CancellationToken cancellationToken);
    Task<AdminTopupPackageResponse> CreateTopupPackageAsync(CreateAdminTopupPackageRequest request, CancellationToken cancellationToken);
    Task<AdminTopupPackageResponse> UpdateTopupPackageAsync(long id, UpdateAdminTopupPackageRequest request, CancellationToken cancellationToken);
    Task DeleteTopupPackageAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminPlatformSettingResponse>> GetPlatformSettingsAsync(CancellationToken cancellationToken);
    Task<AdminPlatformSettingResponse> CreatePlatformSettingAsync(CreateAdminPlatformSettingRequest request, CancellationToken cancellationToken);
    Task<AdminPlatformSettingResponse> UpdatePlatformSettingAsync(long id, UpdateAdminPlatformSettingRequest request, CancellationToken cancellationToken);
    Task DeletePlatformSettingAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminCoinLedgerResponse>> GetCoinLedgersAsync(CancellationToken cancellationToken);
    Task<AdminCoinLedgerResponse> CreateCoinAdjustmentAsync(AdminCoinAdjustmentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminPaymentWebhookLogResponse>> GetPaymentWebhookLogsAsync(CancellationToken cancellationToken);
}

public sealed class AdminService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IAdminService
{
    public async Task<IReadOnlyList<AdminCompanyResponse>> GetCompaniesAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return await dbContext.Companies
            .OrderBy(x => x.Name)
            .Select(x => new AdminCompanyResponse(x.Id, x.Name, x.Email, x.Phone, x.SubscriptionPlan, x.CoinBalance, x.IsFrozen, x.Users.Count, x.Branches.Count, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminCompanyResponse> FreezeCompanyAsync(long id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var company = await GetCompanyAsync(id, cancellationToken);
        company.IsFrozen = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetCompanyResponseAsync(id, cancellationToken);
    }

    public async Task<AdminCompanyResponse> UnfreezeCompanyAsync(long id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var company = await GetCompanyAsync(id, cancellationToken);
        company.IsFrozen = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetCompanyResponseAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminTopupPackageResponse>> GetTopupPackagesAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return await dbContext.TopupPackages.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<AdminTopupPackageResponse> CreateTopupPackageAsync(CreateAdminTopupPackageRequest request, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var package = new TopupPackage { Name = request.Name.Trim(), MoneyAmount = request.MoneyAmount, CoinAmount = request.CoinAmount, IsActive = request.IsActive, SortOrder = request.SortOrder };
        dbContext.TopupPackages.Add(package);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(package);
    }

    public async Task<AdminTopupPackageResponse> UpdateTopupPackageAsync(long id, UpdateAdminTopupPackageRequest request, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var package = await dbContext.TopupPackages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Paket topup tidak ditemukan.");
        package.Name = request.Name.Trim(); package.MoneyAmount = request.MoneyAmount; package.CoinAmount = request.CoinAmount; package.IsActive = request.IsActive; package.SortOrder = request.SortOrder;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(package);
    }

    public async Task DeleteTopupPackageAsync(long id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var package = await dbContext.TopupPackages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Paket topup tidak ditemukan.");
        dbContext.TopupPackages.Remove(package);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminPlatformSettingResponse>> GetPlatformSettingsAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return await dbContext.PlatformSettings.OrderBy(x => x.Key).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<AdminPlatformSettingResponse> CreatePlatformSettingAsync(CreateAdminPlatformSettingRequest request, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var key = request.Key.Trim();
        if (await dbContext.PlatformSettings.AnyAsync(x => x.Key == key, cancellationToken)) throw new InvalidOperationException("Platform setting key sudah ada.");
        var setting = new PlatformSetting { Key = key, Value = request.Value.Trim(), DataType = request.DataType.Trim(), Description = Normalize(request.Description) };
        dbContext.PlatformSettings.Add(setting);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(setting);
    }

    public async Task<AdminPlatformSettingResponse> UpdatePlatformSettingAsync(long id, UpdateAdminPlatformSettingRequest request, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var setting = await dbContext.PlatformSettings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Platform setting tidak ditemukan.");
        setting.Value = request.Value.Trim(); setting.DataType = request.DataType.Trim(); setting.Description = Normalize(request.Description);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(setting);
    }

    public async Task DeletePlatformSettingAsync(long id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var setting = await dbContext.PlatformSettings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Platform setting tidak ditemukan.");
        dbContext.PlatformSettings.Remove(setting);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminCoinLedgerResponse>> GetCoinLedgersAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return await dbContext.CoinLedgers.Include(x => x.Company).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    public async Task<AdminCoinLedgerResponse> CreateCoinAdjustmentAsync(AdminCoinAdjustmentRequest request, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var company = await GetCompanyAsync(request.CompanyId, cancellationToken);
        var before = company.CoinBalance;
        var after = before + request.Amount;
        if (after < 0) throw new InvalidOperationException("Coin balance tidak boleh minus.");
        company.CoinBalance = after;
        var ledger = new CoinLedger { CompanyId = company.Id, TransactionType = CoinTransactionType.ManualAdjustment, ReferenceType = "AdminCoinAdjustment", CoinIn = request.Amount > 0 ? request.Amount : 0, CoinOut = request.Amount < 0 ? Math.Abs(request.Amount) : 0, BalanceBefore = before, BalanceAfter = after, Note = request.Note.Trim() };
        dbContext.CoinLedgers.Add(ledger);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        ledger.Company = company;
        return ToResponse(ledger);
    }

    public async Task<IReadOnlyList<AdminPaymentWebhookLogResponse>> GetPaymentWebhookLogsAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return await dbContext.PaymentWebhookLogs.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Select(x => ToResponse(x)).ToListAsync(cancellationToken);
    }

    private void EnsureSuperAdmin() { if (!currentUserContext.IsSuperAdmin) throw new UnauthorizedAccessException("Super admin wajib."); }
    private async Task<Company> GetCompanyAsync(long id, CancellationToken cancellationToken) => await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Company tidak ditemukan.");
    private async Task<AdminCompanyResponse> GetCompanyResponseAsync(long id, CancellationToken cancellationToken) => await dbContext.Companies.Where(x => x.Id == id).Select(x => new AdminCompanyResponse(x.Id, x.Name, x.Email, x.Phone, x.SubscriptionPlan, x.CoinBalance, x.IsFrozen, x.Users.Count, x.Branches.Count, x.CreatedAt)).FirstAsync(cancellationToken);
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static AdminTopupPackageResponse ToResponse(TopupPackage x) => new(x.Id, x.Name, x.MoneyAmount, x.CoinAmount, x.IsActive, x.SortOrder);
    private static AdminPlatformSettingResponse ToResponse(PlatformSetting x) => new(x.Id, x.Key, x.Value, x.DataType, x.Description);
    private static AdminCoinLedgerResponse ToResponse(CoinLedger x) => new(x.Id, x.CompanyId, x.Company.Name, x.TransactionType, x.ReferenceType, x.ReferenceId, x.CoinIn, x.CoinOut, x.BalanceBefore, x.BalanceAfter, x.Note, x.CreatedAt);
    private static AdminPaymentWebhookLogResponse ToResponse(PaymentWebhookLog x) => new(x.Id, x.Provider, x.Reference, x.Signature, x.IsValid, x.IsProcessed, x.ProcessedAt, x.ErrorMessage, x.CreatedAt);
}
