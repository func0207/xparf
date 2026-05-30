using FluentValidation;
using Xparf.Api.Contracts.Users;

namespace Xparf.Api.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThan(0);
        RuleFor(x => x.BranchIds).NotNull();
    }
}

public sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThan(0);
        RuleFor(x => x.BranchIds).NotNull();
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}

public sealed class UpdateUserBranchesRequestValidator : AbstractValidator<UpdateUserBranchesRequest>
{
    public UpdateUserBranchesRequestValidator()
    {
        RuleFor(x => x.BranchIds).NotNull();
    }
}
