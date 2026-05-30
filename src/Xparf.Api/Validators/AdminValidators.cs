using FluentValidation;
using Xparf.Api.Contracts.Admin;

namespace Xparf.Api.Validators;

public sealed class CreateAdminTopupPackageRequestValidator : AbstractValidator<CreateAdminTopupPackageRequest>
{
    public CreateAdminTopupPackageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.MoneyAmount).GreaterThan(0);
        RuleFor(x => x.CoinAmount).GreaterThan(0);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateAdminTopupPackageRequestValidator : AbstractValidator<UpdateAdminTopupPackageRequest>
{
    public UpdateAdminTopupPackageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.MoneyAmount).GreaterThan(0);
        RuleFor(x => x.CoinAmount).GreaterThan(0);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateAdminPlatformSettingRequestValidator : AbstractValidator<CreateAdminPlatformSettingRequest>
{
    public CreateAdminPlatformSettingRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateAdminPlatformSettingRequestValidator : AbstractValidator<UpdateAdminPlatformSettingRequest>
{
    public UpdateAdminPlatformSettingRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class AdminCoinAdjustmentRequestValidator : AbstractValidator<AdminCoinAdjustmentRequest>
{
    public AdminCoinAdjustmentRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.Amount).NotEqual(0);
        RuleFor(x => x.Note).NotEmpty().MaximumLength(500);
    }
}
