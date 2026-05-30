namespace Xparf.Api.Contracts.Branches;

public sealed record BranchResponse(
    long Id,
    string Code,
    string Name,
    string? Address,
    string? Phone,
    bool IsActive);

public sealed record CreateBranchRequest(
    string Code,
    string Name,
    string? Address,
    string? Phone,
    bool IsActive);

public sealed record UpdateBranchRequest(
    string Name,
    string? Address,
    string? Phone,
    bool IsActive);
