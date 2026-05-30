namespace Xparf.Api.Contracts.Suppliers;

public sealed record SupplierResponse(long Id, string Code, string Name, string? Phone, string? Email, string? Address, bool IsActive);
public sealed record CreateSupplierRequest(string Code, string Name, string? Phone, string? Email, string? Address, bool IsActive);
public sealed record UpdateSupplierRequest(string Name, string? Phone, string? Email, string? Address, bool IsActive);
