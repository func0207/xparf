using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Branches;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IBranchService
{
    Task<PageResponse<BranchResponse>> GetBranchesAsync(PageRequest request, CancellationToken cancellationToken);
    Task<BranchResponse> GetBranchAsync(long id, CancellationToken cancellationToken);
    Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken);
    Task<BranchResponse> UpdateBranchAsync(long id, UpdateBranchRequest request, CancellationToken cancellationToken);
    Task DeleteBranchAsync(long id, CancellationToken cancellationToken);
}

public sealed class BranchService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IBranchService
{
    public async Task<PageResponse<BranchResponse>> GetBranchesAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Branches.Where(x => x.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Phone != null && x.Phone.ToLower().Contains(search) || x.Address != null && x.Address.ToLower().Contains(search));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "code" => desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "phone" => desc ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
            "address" => desc ? query.OrderByDescending(x => x.Address) : query.OrderBy(x => x.Address),
            _ => desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return PageResponse<BranchResponse>.Create(items, page, pageSize, totalItems);
    }

    public async Task<BranchResponse> GetBranchAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var branch = await GetBranchEntityAsync(companyId, id, cancellationToken);
        return ToResponse(branch);
    }

    public async Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Branches.AnyAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("Kode branch sudah ada.");
        }

        var branch = new Branch
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            Address = request.Address,
            Phone = request.Phone,
            IsActive = request.IsActive
        };
        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(branch);
    }

    public async Task<BranchResponse> UpdateBranchAsync(long id, UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var branch = await GetBranchEntityAsync(companyId, id, cancellationToken);
        branch.Name = request.Name.Trim();
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(branch);
    }

    public async Task DeleteBranchAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var branch = await GetBranchEntityAsync(companyId, id, cancellationToken);
        var hasStock = await dbContext.BranchItems.AnyAsync(x => x.BranchId == id && x.QuantityOnHand != 0, cancellationToken);
        if (hasStock)
        {
            throw new InvalidOperationException("Branch masih punya stok, tidak bisa dihapus.");
        }

        dbContext.Branches.Remove(branch);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private long GetCompanyId() => currentUserContext.CompanyId
        ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");

    private async Task<Branch> GetBranchEntityAsync(long companyId, long id, CancellationToken cancellationToken)
    {
        return await dbContext.Branches.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Branch tidak ditemukan.");
    }

    private static BranchResponse ToResponse(Branch branch) => new(
        branch.Id,
        branch.Code,
        branch.Name,
        branch.Address,
        branch.Phone,
        branch.IsActive);
}
