using EnsyInc.Enclave.Api.Models;

using FluentValidation;

namespace EnsyInc.Enclave.Api.Validators;

public sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Role).IsInEnum();
    }
}

public sealed class InviteUsersRequestValidator : AbstractValidator<InviteUsersRequest>
{
    public InviteUsersRequestValidator()
    {
        RuleFor(x => x.Users).NotEmpty();
        RuleForEach(x => x.Users).SetValidator(new InviteUserRequestValidator());
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Role).IsInEnum();
    }
}
