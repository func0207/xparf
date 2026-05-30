using FluentValidation;
using Xparf.Api.Contracts.Inventory;

namespace Xparf.Api.Validators;

public sealed class CreateBranchItemRequestValidator : AbstractValidator<CreateBranchItemRequest>
{
    public CreateBranchItemRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LastCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WholesalePrice).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateBranchItemRequestValidator : AbstractValidator<UpdateBranchItemRequest>
{
    public UpdateBranchItemRequestValidator()
    {
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LastCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WholesalePrice).GreaterThanOrEqualTo(0);
    }
}

public sealed class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.QuantityChange).NotEqual(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
