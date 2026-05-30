using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Complaints;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface ISaleComplaintService
{
    Task<IReadOnlyList<SaleComplaintResponse>> GetComplaintsAsync(CancellationToken cancellationToken);
    Task<SaleComplaintResponse> CreateComplaintAsync(CreateSaleComplaintRequest request, CancellationToken cancellationToken);
    Task<SaleComplaintResponse> ResolveComplaintAsync(long id, ResolveSaleComplaintRequest request, CancellationToken cancellationToken);
}

public sealed class SaleComplaintService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : ISaleComplaintService
{
    public async Task<IReadOnlyList<SaleComplaintResponse>> GetComplaintsAsync(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        return await dbContext.SaleComplaints
            .Include(x => x.Sale)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<SaleComplaintResponse> CreateComplaintAsync(CreateSaleComplaintRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var sale = await dbContext.Sales.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == request.SaleId, cancellationToken)
            ?? throw new InvalidOperationException("Sale tidak ditemukan.");
        var number = string.IsNullOrWhiteSpace(request.ComplaintNumber) ? $"CMP-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.ComplaintNumber.Trim().ToUpperInvariant();
        if (await dbContext.SaleComplaints.AnyAsync(x => x.CompanyId == companyId && x.ComplaintNumber == number, cancellationToken)) throw new InvalidOperationException("Nomor complain sudah ada.");
        var complaint = new SaleComplaint { CompanyId = companyId, SaleId = sale.Id, ComplaintNumber = number, Reason = request.Reason.Trim() };
        dbContext.SaleComplaints.Add(complaint);
        await dbContext.SaveChangesAsync(cancellationToken);
        complaint.Sale = sale;
        return ToResponse(complaint);
    }

    public async Task<SaleComplaintResponse> ResolveComplaintAsync(long id, ResolveSaleComplaintRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var complaint = await dbContext.SaleComplaints.Include(x => x.Sale).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Complain tidak ditemukan.");
        complaint.Resolution = request.Resolution.Trim();
        complaint.IsResolved = true;
        complaint.ResolvedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(complaint);
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new InvalidOperationException("Company context tidak ditemukan.");
    private static SaleComplaintResponse ToResponse(SaleComplaint x) => new(x.Id, x.SaleId, x.Sale.SaleNumber, x.ComplaintNumber, x.Reason, x.Resolution, x.IsResolved, x.ResolvedAt, x.CreatedAt);
}
