using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Suppliers;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface ISupplierService
{
    Task<PageResponse<SupplierResponse>> GetSuppliersAsync(PageRequest request, CancellationToken cancellationToken);
    Task<SupplierResponse> GetSupplierAsync(long id, CancellationToken cancellationToken);
    Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
    Task<SupplierResponse> UpdateSupplierAsync(long id, UpdateSupplierRequest request, CancellationToken cancellationToken);
    Task DeleteSupplierAsync(long id, CancellationToken cancellationToken);
}

public sealed class SupplierService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : ISupplierService
{
    public async Task<PageResponse<SupplierResponse>> GetSuppliersAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Suppliers.Where(x => x.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Phone != null && x.Phone.ToLower().Contains(search) || x.Email != null && x.Email.ToLower().Contains(search));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "code" => desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "phone" => desc ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            _ => desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return PageResponse<SupplierResponse>.Create(items, page, pageSize, totalItems);
    }

    public async Task<SupplierResponse> GetSupplierAsync(long id, CancellationToken cancellationToken) => ToResponse(await GetSupplierEntityAsync(GetCompanyId(), id, cancellationToken));
    public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var code = NormalizeCode(request.Code);
        if (await dbContext.Suppliers.AnyAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken)) throw new InvalidOperationException("Kode supplier sudah ada.");
        var supplier = new Supplier { CompanyId = companyId, Code = code, Name = request.Name.Trim(), Phone = NormalizeOptional(request.Phone), Email = NormalizeOptional(request.Email), Address = NormalizeOptional(request.Address), IsActive = request.IsActive };
        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(supplier);
    }
    public async Task<SupplierResponse> UpdateSupplierAsync(long id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await GetSupplierEntityAsync(GetCompanyId(), id, cancellationToken);
        supplier.Name = request.Name.Trim();
        supplier.Phone = NormalizeOptional(request.Phone);
        supplier.Email = NormalizeOptional(request.Email);
        supplier.Address = NormalizeOptional(request.Address);
        supplier.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(supplier);
    }
    public async Task DeleteSupplierAsync(long id, CancellationToken cancellationToken)
    {
        var supplier = await GetSupplierEntityAsync(GetCompanyId(), id, cancellationToken);
        dbContext.Suppliers.Remove(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<Supplier> GetSupplierEntityAsync(long companyId, long id, CancellationToken cancellationToken) => await dbContext.Suppliers.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Supplier tidak ditemukan.");
    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static SupplierResponse ToResponse(Supplier supplier) => new(supplier.Id, supplier.Code, supplier.Name, supplier.Phone, supplier.Email, supplier.Address, supplier.IsActive);
}
