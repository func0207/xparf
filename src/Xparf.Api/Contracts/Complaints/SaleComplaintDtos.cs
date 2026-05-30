namespace Xparf.Api.Contracts.Complaints;

public sealed record SaleComplaintResponse(long Id, long SaleId, string SaleNumber, string ComplaintNumber, string Reason, string? Resolution, bool IsResolved, DateTime? ResolvedAt, DateTime CreatedAt);
public sealed record CreateSaleComplaintRequest(long SaleId, string ComplaintNumber, string Reason);
public sealed record ResolveSaleComplaintRequest(string Resolution);
