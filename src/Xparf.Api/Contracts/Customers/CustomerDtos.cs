namespace Xparf.Api.Contracts.Customers;

public sealed record CustomerResponse(long Id, long? BranchId, string Code, string Name, string? Phone, string? Email, string? Address, decimal CreditLimit, decimal CurrentDebt, bool IsActive);
public sealed record CreateCustomerRequest(long? BranchId, string Code, string Name, string? Phone, string? Email, string? Address, decimal CreditLimit, bool IsActive);
public sealed record UpdateCustomerRequest(long? BranchId, string Name, string? Phone, string? Email, string? Address, decimal CreditLimit, bool IsActive);
