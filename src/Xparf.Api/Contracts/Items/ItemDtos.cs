using Xparf.Core.Enums;

namespace Xparf.Api.Contracts.Items;

public sealed record ItemResponse(
    long Id,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    ItemType ItemType,
    string BaseUnit,
    bool IsActive,
    bool IsDiscontinued);

public sealed record CreateItemRequest(
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    ItemType ItemType,
    string BaseUnit,
    bool IsActive,
    bool IsDiscontinued);

public sealed record UpdateItemRequest(
    string? Barcode,
    string Name,
    string? Description,
    ItemType ItemType,
    string BaseUnit,
    bool IsActive,
    bool IsDiscontinued);
