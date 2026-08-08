using EnsyInc.Enclave.Api.Models;

using FluentValidation;

namespace EnsyInc.Enclave.Api.Validators;

public sealed class CreateOrgRequestValidator : AbstractValidator<CreateOrgRequest>
{
    public CreateOrgRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}

public sealed class UpdateOrgRequestValidator : AbstractValidator<UpdateOrgRequest>
{
    public UpdateOrgRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}
