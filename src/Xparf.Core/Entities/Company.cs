using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
    public decimal CoinBalance { get; set; }
    public bool IsFrozen { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
