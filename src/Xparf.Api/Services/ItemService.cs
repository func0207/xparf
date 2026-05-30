using Microsoft.EntityFrameworkCore;
using Xparf.Api.Contracts.Common;
using Xparf.Api.Contracts.Items;
using Xparf.Core.Abstractions;
using Xparf.Core.Entities;
using Xparf.Infrastructure.Persistence;

namespace Xparf.Api.Services;

public interface IItemService
{
    Task<PageResponse<ItemResponse>> GetItemsAsync(PageRequest request, CancellationToken cancellationToken);
    Task<ItemResponse> GetItemAsync(long id, CancellationToken cancellationToken);
    Task<ItemResponse> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken);
    Task<ItemResponse> UpdateItemAsync(long id, UpdateItemRequest request, CancellationToken cancellationToken);
    Task DeleteItemAsync(long id, CancellationToken cancellationToken);
}

public sealed class ItemService(XparfDbContext dbContext, ICurrentUserContext currentUserContext) : IItemService
{
    public async Task<PageResponse<ItemResponse>> GetItemsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Items.Where(x => x.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Barcode != null && x.Barcode.ToLower().Contains(search) || x.BaseUnit.ToLower().Contains(search));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "sku" => desc ? query.OrderByDescending(x => x.Sku) : query.OrderBy(x => x.Sku),
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "barcode" => desc ? query.OrderByDescending(x => x.Barcode) : query.OrderBy(x => x.Barcode),
            "baseunit" => desc ? query.OrderByDescending(x => x.BaseUnit) : query.OrderBy(x => x.BaseUnit),
            _ => desc ? query.OrderByDescending(x => x.Sku) : query.OrderBy(x => x.Sku)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return PageResponse<ItemResponse>.Create(items, page, pageSize, totalItems);
    }

    public async Task<ItemResponse> GetItemAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var item = await GetItemEntityAsync(companyId, id, cancellationToken);
        return ToResponse(item);
    }

    public async Task<ItemResponse> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var sku = NormalizeSku(request.Sku);
        if (await dbContext.Items.AnyAsync(x => x.CompanyId == companyId && x.Sku == sku, cancellationToken))
        {
            throw new InvalidOperationException("SKU item sudah ada.");
        }

        var item = new Item
        {
            CompanyId = companyId,
            Sku = sku,
            Barcode = NormalizeOptional(request.Barcode),
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            ItemType = request.ItemType,
            BaseUnit = request.BaseUnit.Trim(),
            IsActive = request.IsActive,
            IsDiscontinued = request.IsDiscontinued
        };

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(item);
    }

    public async Task<ItemResponse> UpdateItemAsync(long id, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var item = await GetItemEntityAsync(companyId, id, cancellationToken);
        item.Barcode = NormalizeOptional(request.Barcode);
        item.Name = request.Name.Trim();
        item.Description = NormalizeOptional(request.Description);
        item.ItemType = request.ItemType;
        item.BaseUnit = request.BaseUnit.Trim();
        item.IsActive = request.IsActive;
        item.IsDiscontinued = request.IsDiscontinued;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(item);
    }

    public async Task DeleteItemAsync(long id, CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var item = await GetItemEntityAsync(companyId, id, cancellationToken);
        var hasStock = await dbContext.BranchItems.AnyAsync(x => x.CompanyId == companyId && x.ItemId == id && x.QuantityOnHand != 0, cancellationToken);
        if (hasStock)
        {
            throw new InvalidOperationException("Item masih punya stok, tidak bisa dihapus.");
        }

        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private long GetCompanyId() => currentUserContext.CompanyId
        ?? throw new UnauthorizedAccessException("Company context tidak ditemukan.");

    private async Task<Item> GetItemEntityAsync(long companyId, long id, CancellationToken cancellationToken)
    {
        return await dbContext.Items.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Item tidak ditemukan.");
    }

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ItemResponse ToResponse(Item item) => new(
        item.Id,
        item.Sku,
        item.Barcode,
        item.Name,
        item.Description,
        item.ItemType,
        item.BaseUnit,
        item.IsActive,
        item.IsDiscontinued);
}
