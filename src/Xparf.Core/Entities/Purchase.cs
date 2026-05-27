using Xparf.Core.Enums;

namespace Xparf.Core.Entities;

public sealed class Purchase : AuditableEntity
{
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string PurchaseNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public ICollection<PurchaseLine> Lines { get; set; } = new List<PurchaseLine>();
    public ICollection<PurchasePayment> Payments { get; set; } = new List<PurchasePayment>();
}
