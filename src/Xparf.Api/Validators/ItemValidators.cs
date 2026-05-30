using FluentValidation;
using Xparf.Api.Contracts.Items;
using Xparf.Core.Enums;

namespace Xparf.Api.Validators;

public sealed class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Barcode).MaximumLength(120);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ItemType).IsInEnum().NotEqual(ItemType.Unknown);
        RuleFor(x => x.BaseUnit).NotEmpty().MaximumLength(30);
    }
}

public sealed class UpdateItemRequestValidator : AbstractValidator<UpdateItemRequest>
{
    public UpdateItemRequestValidator()
    {
        RuleFor(x => x.Barcode).MaximumLength(120);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ItemType).IsInEnum().NotEqual(ItemType.Unknown);
        RuleFor(x => x.BaseUnit).NotEmpty().MaximumLength(30);
    }
}
