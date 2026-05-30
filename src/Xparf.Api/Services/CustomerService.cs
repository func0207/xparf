using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Customers;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface ICustomerService
{
    Task<PageResponse<CustomerResponse>> GetCustomersAsync(PageRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse> GetCustomerAsync(long id, CancellationToken cancellationToken);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse> UpdateCustomerAsync(long id, UpdateCustomerRequest request, CancellationToken cancellationToken);
    Task DeleteCustomerAsync(long id, CancellationToken cancellationToken);
}

public sealed class CustomerService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : ICustomerService
{
    public async Task<PageResponse<CustomerResponse>> GetCustomersAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Customers.Where(x => x.CompanyId == companyId);

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

        return PageResponse<CustomerResponse>.Create(items, page, pageSize, totalItems);
    }

    public async Task<CustomerResponse> GetCustomerAsync(long id, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerEntityAsync(GetCompanyId(), id, cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var code = NormalizeCode(request.Code);
        await ValidateBranchAsync(companyId, request.BranchId, cancellationToken);
        if (await dbContext.Customers.AnyAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken)) throw new InvalidOperationException("Kode customer sudah ada.");
        var customer = new Customer { CompanyId = companyId, BranchId = request.BranchId, Code = code, Name = request.Name.Trim(), Phone = NormalizeOptional(request.Phone), Email = NormalizeOptional(request.Email), Address = NormalizeOptional(request.Address), CreditLimit = request.CreditLimit, IsActive = request.IsActive };
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateCustomerAsync(long id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        await ValidateBranchAsync(companyId, request.BranchId, cancellationToken);
        var customer = await GetCustomerEntityAsync(companyId, id, cancellationToken);
        customer.BranchId = request.BranchId;
        customer.Name = request.Name.Trim();
        customer.Phone = NormalizeOptional(request.Phone);
        customer.Email = NormalizeOptional(request.Email);
        customer.Address = NormalizeOptional(request.Address);
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(customer);
    }

    public async Task DeleteCustomerAsync(long id, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerEntityAsync(GetCompanyId(), id, cancellationToken);
        if (customer.CurrentDebt != 0) throw new InvalidOperationException("Customer masih punya piutang, tidak bisa dihapus.");
        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private long GetCompanyId() => currentUserContext.CompanyId ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");
    private async Task<Customer> GetCustomerEntityAsync(long companyId, long id, CancellationToken cancellationToken) => await dbContext.Customers.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken) ?? throw new InvalidOperationException("Customer tidak ditemukan.");
    private async Task ValidateBranchAsync(long companyId, long? branchId, CancellationToken cancellationToken)
    {
        if (branchId is null) return;
        if (!await dbContext.Branches.AnyAsync(x => x.CompanyId == companyId && x.Id == branchId, cancellationToken)) throw new InvalidOperationException("Branch customer tidak ditemukan.");
    }
    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static CustomerResponse ToResponse(Customer customer) => new(customer.Id, customer.BranchId, customer.Code, customer.Name, customer.Phone, customer.Email, customer.Address, customer.CreditLimit, customer.CurrentDebt, customer.IsActive);
}
