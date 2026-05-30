using FluentValidation;
using Xparf.Api.Contracts.Billing;

namespace Xparf.Api.Validators;

public sealed class CreateTopupRequestValidator : AbstractValidator<CreateTopupRequest>
{
    public CreateTopupRequestValidator()
    {
        RuleFor(x => x.TopupPackageId).GreaterThan(0);
    }
}

public sealed class QrisWebhookRequestValidator : AbstractValidator<QrisWebhookRequest>
{
    public QrisWebhookRequestValidator()
    {
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Signature).MaximumLength(500);
    }
}
