using EnsyInc.Enclave.Api.Models;

using FluentValidation;

namespace EnsyInc.Enclave.Api.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Status).IsInEnum();
    }
}
