using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Billing;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Core.Enums;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IBillingService
{
    Task<BillingCoinBalanceResponse> GetCoinBalanceAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CoinLedgerResponse>> GetCoinLedgersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TopupPackageResponse>> GetTopupPackagesAsync(CancellationToken cancellationToken);
    Task<CoinTopupResponse> CreateTopupAsync(CreateTopupRequest request, CancellationToken cancellationToken);
    Task<CoinTopupResponse> GetTopupAsync(long id, CancellationToken cancellationToken);
    Task<QrisWebhookResponse> ProcessQrisWebhookAsync(QrisWebhookRequest request, CancellationToken cancellationToken);
}

public sealed class BillingService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IBillingService
{
    public async Task<BillingCoinBalanceResponse> GetCoinBalanceAsync(CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(cancellationToken);
        return new BillingCoinBalanceResponse(company.CoinBalance, company.IsFrozen);
    }

    public async Task<IReadOnlyList<CoinLedgerResponse>> GetCoinLedgersAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.CoinLedgers
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TopupPackageResponse>> GetTopupPackagesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TopupPackages
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new TopupPackageResponse(x.Id, x.Name, x.MoneyAmount, x.CoinAmount, x.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<CoinTopupResponse> CreateTopupAsync(CreateTopupRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var package = await dbContext.TopupPackages.FirstOrDefaultAsync(x => x.Id == request.TopupPackageId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Paket topup tidak ditemukan.");

        var topup = new CoinTopup
        {
            CompanyId = companyId,
            TopupNumber = await GenerateTopupNumberAsync(cancellationToken),
            MoneyAmount = package.MoneyAmount,
            CoinAmount = package.CoinAmount,
            Status = TopupStatus.Pending,
            PaymentProvider = "QRIS",
            ProviderReference = $"QRIS-{Guid.NewGuid():N}",
            QrCodeText = $"QRIS|XPARF|{package.MoneyAmount:0.##}|{Guid.NewGuid():N}",
            ExpiredAt = DateTime.UtcNow.AddHours(2)
        };

        dbContext.CoinTopups.Add(topup);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(topup);
    }

    public async Task<CoinTopupResponse> GetTopupAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var topup = await dbContext.CoinTopups.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Topup tidak ditemukan.");
        return ToResponse(topup);
    }

    public async Task<QrisWebhookResponse> ProcessQrisWebhookAsync(QrisWebhookRequest request, CancellationToken cancellationToken)
    {
        var payload = string.IsNullOrWhiteSpace(request.PayloadJson) ? $"{{\"reference\":\"{request.Reference}\",\"amount\":{request.Amount},\"status\":\"{request.Status}\"}}" : request.PayloadJson;
        var log = new PaymentWebhookLog { Provider = "QRIS", Reference = request.Reference.Trim(), PayloadJson = payload, Signature = request.Signature, IsValid = false };
        dbContext.PaymentWebhookLogs.Add(log);

        var topup = await dbContext.CoinTopups.FirstOrDefaultAsync(x => x.ProviderReference == request.Reference.Trim(), cancellationToken);
        if (topup is null)
        {
            log.ErrorMessage = "Topup tidak ditemukan.";
            await dbContext.SaveChangesAsync(cancellationToken);
            return new QrisWebhookResponse(false, log.ErrorMessage);
        }

        log.IsValid = string.Equals(request.Status, "paid", StringComparison.OrdinalIgnoreCase) && request.Amount == topup.MoneyAmount;
        if (!log.IsValid)
        {
            log.ErrorMessage = "Webhook QRIS tidak valid.";
            await dbContext.SaveChangesAsync(cancellationToken);
            return new QrisWebhookResponse(false, log.ErrorMessage);
        }

        if (topup.Status == TopupStatus.Paid)
        {
            log.IsProcessed = true;
            log.ProcessedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new QrisWebhookResponse(true, "Topup sudah paid.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var company = await dbContext.Companies.FirstAsync(x => x.Id == topup.CompanyId, cancellationToken);
        var before = company.CoinBalance;
        company.CoinBalance += topup.CoinAmount;
        topup.Status = TopupStatus.Paid;
        topup.PaidAt = DateTime.UtcNow;
        dbContext.CoinLedgers.Add(new CoinLedger
        {
            CompanyId = company.Id,
            TransactionType = CoinTransactionType.Topup,
            ReferenceType = "CoinTopup",
            ReferenceId = topup.Id,
            CoinIn = topup.CoinAmount,
            BalanceBefore = before,
            BalanceAfter = company.CoinBalance,
            Note = topup.TopupNumber
        });
        log.IsProcessed = true;
        log.ProcessedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new QrisWebhookResponse(true, "Topup paid diproses.");
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<Company> GetCompanyAsync(CancellationToken cancellationToken) => await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == GetCompanyId(), cancellationToken) ?? throw new InvalidOperationException("Company tidak ditemukan.");
    private async Task<string> GenerateTopupNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await dbContext.CoinTopups.CountAsync(x => x.TopupNumber.StartsWith($"TU-{today}"), cancellationToken) + 1;
        return $"TU-{today}-{count:0000}";
    }
    private static CoinLedgerResponse ToResponse(CoinLedger x) => new(x.Id, x.TransactionType, x.ReferenceType, x.ReferenceId, x.CoinIn, x.CoinOut, x.BalanceBefore, x.BalanceAfter, x.Note, x.CreatedAt);
    private static CoinTopupResponse ToResponse(CoinTopup x) => new(x.Id, x.TopupNumber, x.MoneyAmount, x.CoinAmount, x.Status, x.PaymentProvider, x.ProviderReference, x.QrCodeText, x.QrCodeImageUrl, x.ExpiredAt, x.PaidAt);
}
