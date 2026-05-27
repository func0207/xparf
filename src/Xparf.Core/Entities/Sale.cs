using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class Sale : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public SaleType SaleType { get; set; } = SaleType.Retail;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal CoinDeducted { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Draft;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? IdempotencyKey { get; set; }

    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
}
