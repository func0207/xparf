using FluentValidation;
using Xparf.Api.Contracts.Purchases;

namespace Xparf.Api.Validators;

public sealed class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.PurchaseNumber).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePurchaseRequestValidator : AbstractValidator<UpdatePurchaseRequest>
{
    public UpdatePurchaseRequestValidator()
    {
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePurchaseLineRequestValidator : AbstractValidator<CreatePurchaseLineRequest>
{
    public CreatePurchaseLineRequestValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(30);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePurchaseLineRequestValidator : AbstractValidator<UpdatePurchaseLineRequest>
{
    public UpdatePurchaseLineRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(30);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePurchasePaymentRequestValidator : AbstractValidator<CreatePurchasePaymentRequest>
{
    public CreatePurchasePaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
